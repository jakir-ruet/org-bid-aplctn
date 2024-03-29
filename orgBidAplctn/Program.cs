using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using orgBidAplctn.Filters;
using orgBidAplctn.Models;
using System;
using System.Net;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(o => {
                                            o.Filters.Add(new ViewBagActionFilter());
                                            o.Filters.Add(new AuthorizeFilter());
                                        });
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Accessing Session from Views ...
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
                            {
                                options.IdleTimeout = TimeSpan.FromMinutes(60);
                            });

builder.Services.AddScoped<ViewBagActionFilter>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("AppDb"))
                                                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
                                    {
                                        options.Cookie.Name = "Bid.Cookie";
                                        // Cookie Expiring Option Start =========
                                        // https://stackoverflow.com/questions/34979680/asp-net-core-mvc-setting-expiration-of-identity-cookie/34981457
                                        options.ExpireTimeSpan = TimeSpan.FromHours(24); // TimeSpan.FromDays(2); Default Is 14 Days
                                        options.SlidingExpiration = true;
                                        // Cookie Expiring Option End =========
                                        options.LoginPath = "/Security/SignInApp";
                                        options.LogoutPath = "/Security/SignOutApp";
                                        options.AccessDeniedPath = "/Security/AccessDenied";
                                    });

var app = builder.Build();

if(!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "text/html";

            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature.Error;

            await context.Response.WriteAsync("<html lang=\"en\"><body>\r\n");
            await context.Response.WriteAsync($"<h1>Error: {exception.Message}</h1><br>\r\n");
            await context.Response.WriteAsync($"<h2>Stack Trace:</h2><br>\r\n");
            await context.Response.WriteAsync($"{exception.StackTrace}\r\n");
            await context.Response.WriteAsync("</body></html>\r\n");
        });
    });
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // app.UseHsts();
}
var cachePeriod = app.Environment.IsDevelopment() ? "60" : "86400"; // Numbers In Seconds ...
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
    }
});
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Security}/{action=SignInApp}/{id?}");
// pattern: "{controller=Bidding}/{action=BidMngr}/{id?}");

app.Run();