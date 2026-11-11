// Copyright © Korshun Ivan. All rights reserved.

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Template.Library.Common;
using Template.Library.Common.Http;
using Template.Library.DataSource.Abstraction;
using Template.Library.Models;

namespace Template.Library.DataSource.Organization;

public class BranchRepository(IHttpClientFactory clientFactory) : IBranchRepository
{
    private const string ServiceName = "Organization";
    private const string EntityName = "Branch";
    private const string BasePath = "api/v1/branches";

    private readonly HttpClient _httpClient = clientFactory.CreateClient(nameof(OrganizationServiceOptions));

    public async Task<ICollection<Branch>> GetAllAsync(CancellationToken token)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(
                new Uri(BasePath, UriKind.Relative), token);

            await response.EnsureSuccessAsync();

            DataFragment<Branch>? branches = await response.Content.ReadFromJsonAsync(
                BranchJsonContext.Default.DataFragmentBranch, token);

            return branches.EnsureNotNull().Data;
        }
        catch (HttpRequestException e)
        {
            throw new HttpOperationException(ServiceName, EntityName, HttpMethod.Get, e);
        }
    }

    public async Task<Branch?> GetAsync(int id, CancellationToken token)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(
                new Uri($"{BasePath}/{id}", UriKind.Relative), token);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            await response.EnsureSuccessAsync();

            return (await response.Content.ReadFromJsonAsync(BranchJsonContext.Default.Branch, token))
                .EnsureNotNull();
        }
        catch (HttpRequestException e)
        {
            throw new HttpOperationException(ServiceName, EntityName, HttpMethod.Get, e);
        }
    }

    public async Task<Branch> CreateAsync(BranchCreateDto branchDto, CancellationToken token)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                BasePath, branchDto, BranchJsonContext.Default.BranchCreateDto, token);

            await response.EnsureSuccessAsync();

            return (await response.Content.ReadFromJsonAsync(BranchJsonContext.Default.Branch, token))
                .EnsureNotNull();
        }
        catch (Exception e) when (e is HttpRequestException or InvalidOperationException)
        {
            throw new HttpOperationException(ServiceName, EntityName, HttpMethod.Put, e);
        }
    }

    public async Task<Branch?> UpdateAsync(int id, BranchUpdateDto branchDto, CancellationToken token)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.PatchAsJsonAsync(
                $"{BasePath}/{id}", branchDto, BranchJsonContext.Default.BranchUpdateDto, token);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            await response.EnsureSuccessAsync();

            return (await response.Content.ReadFromJsonAsync(BranchJsonContext.Default.Branch, token))
                .EnsureNotNull();
        }
        catch (Exception e) when (e is HttpRequestException or InvalidOperationException)
        {
            throw new HttpOperationException(ServiceName, EntityName, HttpMethod.Put, e);
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken token)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.DeleteAsync(
                new Uri($"{BasePath}/{id}", UriKind.Relative), token);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            await response.EnsureSuccessAsync();
            return true;
        }
        catch (HttpRequestException e)
        {
            throw new HttpOperationException(ServiceName, EntityName, HttpMethod.Delete, e);
        }
    }
}
