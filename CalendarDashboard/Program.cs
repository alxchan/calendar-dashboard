using CalendarDashboard.Models;
using CalendarDashboard.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

namespace CalendarDashboard
{
    public class Program
    {
        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<CalendarDBContext>(options =>
            options.UseSqlite("Data Source=calendar.db"));

            builder.Services.AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            }).AddCookie(options => {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;                   
            }).AddGoogle(options => {
                options.ClientId = builder.Configuration["Google:ClientId"];
                options.ClientSecret = builder.Configuration["Google:ClientSecret"];
                options.Scope.Add(CalendarService.Scope.Calendar);
                options.SaveTokens = true;
                options.CallbackPath = "/signin-google";
            });

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<GoogleCalendarService>(sp =>
            {
                var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
                var accessToken = httpContext.GetTokenAsync("access_token").GetAwaiter().GetResult();

                var credential = GoogleCredential.FromAccessToken(accessToken);

                var calendarService =  new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Integrated Calendar Dashboard"
                });
                return new GoogleCalendarService(calendarService);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
