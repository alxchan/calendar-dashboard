namespace CalendarDashboard.Services
{
    public class CalendarAPIService
    {
        private readonly HttpClient _httpClient;
        public CalendarAPIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

    }
}
