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
        var tokenRequest = await _httpClient.PostAsync("api/calendar/token", null);
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
            //Refresh the token
            //var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "/api/calendar/refresh");
            //var accessToken = await _httpClient.SendAsync(tokenRequest);
            
            //Retry the request
            var retryRequest = new HttpRequestMessage(HttpMethod.Get, apiEndpoint);
            apiResponse = await _httpClient.SendAsync(retryRequest);
        }

        return await _httpClient.GetFromJsonAsync<List<LocalEvent>>(apiEndpoint);
    }

    public async System.Threading.Tasks.Task<List<CalendarDashboard.Models.Task>> GetTasksAsync(string apiEndpoint)
    {

        var request = new HttpRequestMessage(HttpMethod.Get, apiEndpoint);

        var apiResponse = await _httpClient.SendAsync(request);

        if (apiResponse.StatusCode == HttpStatusCode.BadRequest)
        {
            await GetAntiForgeryToken();
            var retryRequest = new HttpRequestMessage(HttpMethod.Get, apiEndpoint);
            apiResponse = await _httpClient.SendAsync(retryRequest);
        }

        if (apiResponse.StatusCode != HttpStatusCode.OK)
        {
            //Refresh the token
            //var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "/api/calendar/refresh");
            //var accessToken = await _httpClient.SendAsync(tokenRequest);

            //Retry the request
            var retryRequest = new HttpRequestMessage(HttpMethod.Get, apiEndpoint);
            apiResponse = await _httpClient.SendAsync(retryRequest);
        }

        return await _httpClient.GetFromJsonAsync<List<CalendarDashboard.Models.Task>>(apiEndpoint);
    }

    public async System.Threading.Tasks.Task AddTask(CalendarDashboard.Models.Task localEvent, List<CalendarDashboard.Models.Task> currentTasks)
    {
        currentTasks.Add(localEvent);
        var response = await _httpClient.PostAsJsonAsync("/api/calendar/add_task", localEvent);
        if (response.IsSuccessStatusCode)
        {
            var addedEvent = await response.Content.ReadFromJsonAsync<CalendarDashboard.Models.Task>();
            localEvent.Id = addedEvent!.Id;
        }
        else
        {
            currentTasks.Remove(localEvent);
        }
    }

    public async System.Threading.Tasks.Task AddEvent(EventDTO localEvent, List<LocalEvent> currentEvents)
    {
        LocalEvent newEvent = new LocalEvent()
        {
            Name = localEvent.Name,
            Description = localEvent.Description,
            StartTime = new Google.Apis.Calendar.v3.Data.EventDateTime() { DateTimeDateTimeOffset = localEvent.StartTime },
            EndTime = new Google.Apis.Calendar.v3.Data.EventDateTime() { DateTimeDateTimeOffset = localEvent.EndTime },
            Email = localEvent.Email,
        };
        currentEvents.Add(newEvent);
        var response = await _httpClient.PostAsJsonAsync("/api/calendar/add_event", localEvent);
        if (response.IsSuccessStatusCode)
        {
            var addedEvent = await response.Content.ReadFromJsonAsync<LocalEvent>();
            newEvent.EventId = addedEvent!.EventId;
        }
        else
        {
            currentEvents.Remove(newEvent);
        }
    }

    public async System.Threading.Tasks.Task DeleteEvent(string eventId, List<LocalEvent> currentEvent)
    {
        var remove = currentEvent.FirstOrDefault(evt => evt.EventId == eventId);
        if (remove != null)
        {
            currentEvent.Remove(remove!);
            var response = await _httpClient.PostAsJsonAsync("/api/calendar/delete_event", eventId);
            if (!response.IsSuccessStatusCode)
            {
                currentEvent.Add(remove!);
            }
        }
    }

    public async System.Threading.Tasks.Task DeleteTask(int eventId, List<CalendarDashboard.Models.Task> currentEvent)
    {
        var remove = currentEvent.FirstOrDefault(evt => evt.Id == eventId);
        if (remove != null)
        {
            currentEvent.Remove(remove!);
            var response = await _httpClient.PostAsJsonAsync("/api/calendar/delete_task", eventId);
            if (!response.IsSuccessStatusCode)
            {
                currentEvent.Add(remove!);
            }
        }
    }
}