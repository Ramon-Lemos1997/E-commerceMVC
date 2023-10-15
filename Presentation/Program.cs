using Infra.Data.SendEmail;
using Application.Services.Account;
using Application.Services.IdentityRoles;
using Infra.Data.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Contracts.Interfaces.Identity;
using Contracts.Interfaces.Infra.Data;
using Contracts.Interfaces.Roles;
using Infra.Data.IdentityErrors;
using Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

//settings para usar o appsettinhs json no projeto
builder.Configuration.AddJsonFile("appsettings.json");

builder.Services.AddControllersWithViews();

var connection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connection));

//settings gerais para usar o identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
          .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders()
          .AddErrorDescriber<PortugueseMessages>();


builder.Services.Configure<IdentityOptions>(options =>
{
    //register settings
    options.Password.RequiredLength = 10;
    options.Password.RequiredUniqueChars = 3;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;

    //lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(1);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;


});

//configuração do cookie gerado pelo identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "AspNetCore.Cookies";
    options.ExpireTimeSpan = TimeSpan.FromDays(1); 
    options.SlidingExpiration = false;
});

////configuração de cookies de forma geral, ainda sem uso
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.Cookie.Name = "AspNetCore.Cookies";
//        options.ExpireTimeSpan = TimeSpan.FromSeconds(20);
//        options.SlidingExpiration = false;
//    });

//settinhs de roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin",
         policy => policy.RequireRole("Admin"));
});

//injeções de dependências gerais
builder.Services.AddScoped<IAccountInterface, AccountService>();
builder.Services.AddScoped<IAdminRoleInterface, AdminRoleService>();
builder.Services.AddScoped<IAdminUserInterface, AdminUserService>();
builder.Services.AddScoped<ISendEmail, SendEmailService>();
builder.Services.AddScoped<IUserRoleInitial, UserRoleInitial>();

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

//await CriarPerfisUsuariosAsync(app);

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "MinhaArea",
      pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}"
    );
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

//criar roles iniciais e usuários e claim's
//async Task CriarPerfisUsuariosAsync(WebApplication app)
//{
//    var scopedFactory = app.Services.GetService<IServiceScopeFactory>();
//    using (var scope = scopedFactory.CreateScope())
//    {
//        var service = scope.ServiceProvider.GetService<IUserRoleInitial>();
//        await service.RolesAsync();
//        //await service.UsersAsync();
//        //var service = scope.ServiceProvider.GetService<IUserClaimInital>();
//        //await service.UserClaims();
//    }
//}