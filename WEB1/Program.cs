using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews(config =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    config.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddAuthentication
    (options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "oidc";
    })
              .AddCookie(options =>
              {
                  options.Cookie.HttpOnly = true;
                  options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                  options.LoginPath = "/Auth/Login";
                  options.AccessDeniedPath = "/Auth/AccessDenied";
                  options.SlidingExpiration = true;
              })
              .AddOpenIdConnect("oidc", options => {
                  options.Authority = "https://localhost:7003/";
                  options.GetClaimsFromUserInfoEndpoint = true;
                  options.ClientId = "jdmtv7067";
                  options.ClientSecret = "jdmsecretjdmtv7067";
                  options.ResponseType = "code";

                  options.TokenValidationParameters.NameClaimType = "name";
                  options.TokenValidationParameters.RoleClaimType = "role";
                  options.Scope.Add("magic");
                  options.SaveTokens = true;

                  options.ClaimActions.MapJsonKey("role", "role");

                  options.Events = new OpenIdConnectEvents
                  {
                      OnRemoteFailure = context =>
                      {
                          context.Response.Redirect("/");
                          context.HandleResponse();
                          return Task.FromResult(0);
                      }
                  };
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
