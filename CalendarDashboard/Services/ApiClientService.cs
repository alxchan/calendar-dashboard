using System.Net;
using System.Security.Claims;
using CalendarDashboard.Models;
using CalendarDashboard.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;

public class ApiClientService
{
    private readonly HttpClient _httpClient;
    private readonly HttpClientHandler _handler;

    public ApiClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _handler = new HttpClientHandler();
        // Add the certificate validation callback here (REMOVE OUTSIDE DEV)
        _handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    public async System.Threading.Tasks.Task GetAntiForgeryToken()
    {
        // Clear any existing CSRF token to prevent duplicate header exception.
        if (_httpClient.DefaultRequestHeaders.Contains("X-CSRF-TOKEN"))
        {
            _httpClient.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        }
        var tokenRequest = await _httpClient.GetAsync("api/calendar/token");
        tokenRequest.EnsureSuccessStatusCode();
        var requestToken = await tokenRequest.Content.ReadAsStringAsync();
        //Need to parse this string
        _httpClient.DefaultRequestHeaders.Add("X-CSRF-TOKEN", requestToken);
    }
    public async System.Threading.Tasks.Task<List<LocalEvent>> CallApiAsync(string apiEndpoint)
    {

        var request = new HttpRequestMessage(HttpMethod.Get, apiEndpoint);

        var apiResponse = await _httpClient.SendAsync(request);

        if (apiResponse.StatusCode == HttpStatusCode.BadRequest)
        {
            await GetAntiForgeryToken();
            var retryRequest = new HttpRequestMessage(HttpMethod.Get, apiEndpoint);
            apiResponse = await _httpClient.SendAsync(retryRequest);
        }

        if(apiResponse.StatusCode != HttpStatusCode.OK)
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "/api/calendar/refresh");
            var accessToken = await _httpClient.SendAsync(tokenRequest);
        }

        return await _httpClient.GetFromJsonAsync<List<LocalEvent>>(apiEndpoint);
    }

}