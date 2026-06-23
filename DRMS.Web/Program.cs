using Microsoft.AspNetCore.Authentication.Cookies;
using DRMS.Application.Services.Interfaces;
using DRMS.Application.Services.Implementations;
using DRMS.Domain.Interfaces;
using DRMS.Infrastructure.Data;
using DRMS.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. DapperContext (singleton)
builder.Services.AddSingleton<DapperContext>();

// 2. Repositories (scoped)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IDeploymentRequestRepository, DeploymentRequestRepository>();
builder.Services.AddScoped<IDeploymentApprovalRepository, DeploymentApprovalRepository>();
builder.Services.AddScoped<IDeploymentHistoryRepository, DeploymentHistoryRepository>();
builder.Services.AddScoped<IMasterDataRepository, MasterDataRepository>();

// 3. Services (scoped)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDeploymentRequestService, DeploymentRequestService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();

// 4. Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// 5. Authorization with role policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DeveloperOnly", policy => policy.RequireRole("Developer"));
    options.AddPolicy("TechLeadOnly", policy => policy.RequireRole("TechLead"));
    options.AddPolicy("QAOnly", policy => policy.RequireRole("QA"));
    options.AddPolicy("DevOpsOnly", policy => policy.RequireRole("DevOps"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Approvers", policy => policy.RequireRole("TechLead", "QA", "DevOps"));
});

// 6. Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
