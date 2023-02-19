using DataAccessLibrary.DBAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<ISqlDataAccess, SqlDataAccess>();
builder.Services.AddSingleton<IProfileData, ProfileData>();
builder.Services.AddSingleton<IEmoteData, EmoteData>();


builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{
    options.Cookie.Name = "MyCookieAuth";
    options.LoginPath = "/Home/LogIn";
    options.AccessDeniedPath = "/Home/LogIn";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("VerifiedOnly", policy => policy.RequireClaim("Verified"));
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("Admin"));
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
