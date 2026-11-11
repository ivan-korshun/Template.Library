// Copyright © Korshun Ivan. All rights reserved.

using System.Globalization;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Template.Library.Common;

namespace Template.Library.IntegrationTests.Metrics;

[TestFixture]
public class PrometheusMetricsTests : IntegrationTestsBase
{
    private string _metricsUrl = null!;
    private HttpClient _client = null!;

    public override async Task OnOneTimeSetUp()
    {
        await base.OnOneTimeSetUp();

        IConfiguration config = Factory.Services.GetRequiredService<IConfiguration>();
        int port = config.GetValue<int>($"{AppSection}:Metrics:ExporterPort");
        string endpoint = config[$"{AppSection}:Metrics:ExporterEndpoint"].EnsureNotEmpty();

        _metricsUrl = $"http://0.0.0.0:{port}{endpoint}";

        _client = Factory.CreateClient();
    }

    public override Task OnOneTimeTearDown()
    {
        _client.Dispose();
        return base.OnOneTimeTearDown();
    }

    [Test]
    public async Task Metrics_500ErrorReturned_RecordedInMetrics()
    {
        // Act
        using HttpResponseMessage response = await _client.GetAsync(new Uri("/Error", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        List<string> metrics = await GetMetrics();

        AssertMetricPositive(
            metrics,
            "http_requests_received_total{code=\"500\",method=\"GET\",controller=\"Error\",action=\"Get\",endpoint=\"Error\"}");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.AddControllers().AddApplicationPart(typeof(PrometheusMetricsTests).Assembly);
        });
    }

    private static void AssertMetricPositive(List<string> metrics, string metricName)
    {
        AssertMetric(metrics, metricName, (name, value) => value.Should().BeGreaterThan(0, name));
    }

    private static void AssertMetric(List<string> metrics, string metricName, Action<string, double> assertAction)
    {
        string? metric = metrics.FirstOrDefault(line => line.StartsWith(metricName, StringComparison.Ordinal));

        metric.Should().NotBeNull(metricName);

        AssertMetric(metric, assertAction);
    }

    private static void AssertMetric(string metric, Action<string, double> assertAction)
    {
        string[] parts = metric.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        string metricName = parts[0];
        string metricValue = parts[1];

        double.TryParse(metricValue, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out double valueNumber)
            .Should()
            .BeTrue($"Metric value should be double, but found: {metricValue}");

        assertAction(metricName, valueNumber);
    }

    private async Task<List<string>> GetMetrics()
    {
        string content = await GetPrometheusContent();

        return content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !line.StartsWith('#'))
            .ToList();
    }

    private async Task<string> GetPrometheusContent()
    {
        using HttpResponseMessage response = await _client.GetAsync(_metricsUrl);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}

[Route("[controller]")]
public sealed class ErrorController : Controller
{
    [HttpGet]
    public IActionResult Get()
    {
        return StatusCode(StatusCodes.Status500InternalServerError);
    }
}
