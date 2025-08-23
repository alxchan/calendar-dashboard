using System;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text.Json;
using CalendarDashboard.Models;
using CalendarDashboard.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
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
            builder.Services.AddTransient<AccessTokenHandler>();
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            builder.Services.AddAntiforgery(options => {
                options.HeaderName = "X-CSRF-TOKEN";
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", policy =>
                {
                    policy.WithOrigins("https://localhost:7107")
                          .AllowCredentials()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddHttpClient<ApiClientService>(client => {
                client.BaseAddress = new Uri("https://localhost:7107");
            })
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var handler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = new CookieContainer(),

                    // Ignore SSL certs for dev
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                return handler;
            })
            .AddHttpMessageHandler<AccessTokenHandler>();

            builder.Services.AddHttpClient("CalendarAPI", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7107");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).ConfigurePrimaryHttpMessageHandler(sp =>
            {
                return CookieHandler.AttachCookie(sp, "https://localhost:7107");
            });


            builder.Services.AddHttpClient("CSRF", options =>
            {
                options.BaseAddress = new Uri("https://localhost:7107");
            }).ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var handler = new HttpClientHandler
                {

                    // Ignore SSL certs for dev
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                return handler;
            });

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
                    var claimsIdentity = (ClaimsIdentity)context.Principal!.Identity!;
                    if (!claimsIdentity.HasClaim(c => c.Type == "service"))
                    {
                        claimsIdentity.AddClaim(new Claim("service", "google"));
                    }


                    var accessToken = context.AccessToken;
                    var refreshToken = context.RefreshToken;
                    DateTime? expiresAt = context.ExpiresIn.HasValue ? DateTime.UtcNow.Add(context.ExpiresIn.Value) : null;
                    var userJSON = context.User.TryGetProperty("email", out JsonElement sub);
                    string? email = sub.GetString()!.ToLower();

                    //To Do: Implement SQL Integration
                    var db = context.HttpContext.RequestServices.GetRequiredService<CalendarDBContext>();
                    var existing = db.UserTokens.FirstOrDefault(x => x.Email == email && x.Service == "google");
                    if (existing != null)
                    {
                        existing.AccessToken = !string.IsNullOrEmpty(accessToken) ? accessToken! : null;
                        existing.RefreshToken = !string.IsNullOrEmpty(refreshToken) ? AesGcmEncryptor.encrypt(refreshToken, Convert.FromBase64String(builder.Configuration["API_KEY"]!)) : existing.RefreshToken;
                        db.UserTokens.Update(existing);
                    }
                    else
                    {
                        db.UserTokens.Add(new UserToken
                        {
                            Email = email!,
                            Service = "google",
                            AccessToken = !string.IsNullOrEmpty(accessToken) ? accessToken! : null,
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
            app.UseCors("AllowSpecificOrigin");
            //app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapBlazorHub();
            app.Run();
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
