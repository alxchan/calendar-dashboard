using System;
using System.Text.Json;
using CalendarDashboard.Models;
using CalendarDashboard.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;

namespace CalendarDashboard
{
    public class Program
    {
        public static System.Threading.Tasks.Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<CalendarServiceHandler>();
            builder.Services.AddScoped<TokenServiceHandler>();

            //Uses PostgreSQL
            //builder.Services.AddDbContext<CalendarDBContext>(options =>
            //options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ??
            //          Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")));

            //Uses SQLite
            builder.Services.AddDbContext<CalendarDBContext>(options =>
            options.UseSqlite("Data Source=calendar.db"));



            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            }).AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                //options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
            }).AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Google:ClientId"]!;
                options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
                options.Scope.Add(CalendarService.Scope.Calendar);
                options.SaveTokens = true;
                options.CallbackPath = "/signin-google";
                options.AccessType = "offline";

                options.Events.OnCreatingTicket = async context =>
                {
                    var accessToken = context.AccessToken;
                    var refreshToken = context.RefreshToken;
                    DateTime? expiresAt = context.ExpiresIn.HasValue ? DateTime.UtcNow.Add(context.ExpiresIn.Value) : null;
                    var userJSON = context.User.TryGetProperty("sub", out JsonElement sub);
                    string? userId = sub.GetString();

                    //To Do: Implement SQL Integration
                    var db = context.HttpContext.RequestServices.GetRequiredService<CalendarDBContext>();
                    var existing = db.UserTokens.FirstOrDefault(x => x.UserId == userId);
                    if (existing != null)
                    {
                        existing.AccessToken = !string.IsNullOrEmpty(accessToken) ? AesGcmEncryptor.encrypt(accessToken!, Convert.FromBase64String(builder.Configuration["API_KEY"]!)) : null;
                        existing.RefreshToken = !string.IsNullOrEmpty(refreshToken) ? AesGcmEncryptor.encrypt(refreshToken, Convert.FromBase64String(builder.Configuration["API_KEY"]!)) : existing.RefreshToken;
                        db.UserTokens.Update(existing);
                    }
                    else
                    {
                        db.UserTokens.Add(new UserToken
                        {
                            UserId = userId!,
                            AccessToken = !string.IsNullOrEmpty(accessToken) ? AesGcmEncryptor.encrypt(accessToken!, Convert.FromBase64String(builder.Configuration["API_KEY"]!)) : null,
                            RefreshToken = AesGcmEncryptor.encrypt(refreshToken!, Convert.FromBase64String(builder.Configuration["API_KEY"]!)),
                        });
                    }

                    await db.SaveChangesAsync();
                };
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CalendarDBContext>();
                db.Database.Migrate();
            }

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
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
