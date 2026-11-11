// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using NUnit.Framework;
using Template.Library.E2ETests.TestUtil;

namespace Template.Library.E2ETests;

[SetUpFixture]
public class GlobalSetup
{
    public static HttpClient GenericClient { get; private set; } = null!;

    public static HttpClient SutClient { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        GenericClient = HttpUtil.CreateClient();

        string accessToken = await HttpUtil.GetAccessToken(GenericClient);

        SutClient = HttpUtil.CreateClient(Settings.LibraryHost, accessToken);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        SutClient?.Dispose();
        GenericClient?.Dispose();
    }
}
