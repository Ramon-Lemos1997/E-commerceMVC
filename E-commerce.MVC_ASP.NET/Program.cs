using E_commerce.MVC_ASP.NET.Domain.Interfaces;
using E_commerce.MVC_ASP.NET.Infra.SendGrind;
using E_commerce.MVC_ASP.NET.Infrastructure.DataContext;
using E_commerce.MVC_ASP.NET.Services.Account;
using E_commerce.MVC_ASP.NET.Services.IdentityRoles;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddJsonFile("appsettings.json");


builder.Services.AddControllersWithViews();
var connection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connection));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
          .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders(); 



//builder.Services.Configure<IdentityOptions>(options =>
//{
//    options.Password.RequiredLength = 10;
//    options.Password.RequiredUniqueChars = 3;
//    options.Password.RequireNonAlphanumeric = false;
//});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AspNetCore.Cookies";
        options.ExpireTimeSpan = TimeSpan.FromSeconds(20);
        options.SlidingExpiration = false;
    });


builder.Services.AddScoped<IAccountInterface, AccountService>();
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


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