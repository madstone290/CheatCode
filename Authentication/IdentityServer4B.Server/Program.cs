using IdentityServer4B.Server;
using IdentityServer4B.Server.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.EntityFramework.Mappers;
using System.Security.Cryptography.X509Certificates;
using IdentityServer4;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using IdentityServer4B.Server.Identity;
using IdentityServer4B.Shared;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("LocalDb");

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    //options.UseInMemoryDatabase("Memory");
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 1;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddUserManager<ApplicationUserManager>()
    .AddUserStore<ApplicationUserStore>()
    .AddEntityFrameworkStores<AppIdentityDbContext>()
    .AddDefaultTokenProviders();

//builder.Services.AddScoped<IUserStore<ApplicationUser>, ApplicationUserStore>();

// Identity������ �� ��Ű ����
// AddAuthentication ���� �����.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "IdentityServer.Server.Cookie";
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
});

var assembly = typeof(AppIdentityDbContext).Assembly.GetName().Name;

//var filePath = Path.Combine(builder.Environment.ContentRootPath, "is_cert_secret.pfx");
//var certificate = new X509Certificate2(filePath, "password");

builder.Services.AddIdentityServer(options =>
{
})
    .AddAspNetIdentity<ApplicationUser>() // IdentityUser -> Token ��ȯ�� �����ش� 
    .AddConfigurationStore<AppConfigurationDbContext>(options =>
    {
        options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(assembly));
    })
    .AddOperationalStore<AppPersistedGrantDbContext>(options =>
    {
        options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(assembly));
        // ������ ��ū�� DB���� �����Ѵ�.
        options.EnableTokenCleanup = true;
    })
    //.AddInMemoryIdentityResources(Configuration.GetIdentityResources())
    //.AddInMemoryApiScopes(Configuration.GetScopes())
    //.AddInMemoryApiResources(Configuration.GetApis())
    //.AddInMemoryClients(Configuration.GetClients())
    //.AddSigningCredential(certificate);
    .AddDeveloperSigningCredential();

builder.Services.AddAuthentication()
    .AddGoogle("Google", options =>
    {
        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

        //# �������� �ٿ���� �� CheatCodeGoogleSecret.json���� ���� ��ġ�� ��
        //# link: https://drive.google.com/file/d/1DCfTWaSCUcCloaf-36jYovJAN38LF4Np/view?usp=sharing 

        var jObject = JObject.Parse(File.ReadAllText("Secrets/CheatCodeGoogleSecret.json"));
        options.ClientId = jObject.GetValue("ClientId").ToString();
        options.ClientSecret = jObject.GetValue("ClientSecret").ToString();

        // Google Console�� ��ϵ� Redirect Uri�� �����ؾ��Ѵ�. �̵��� ó���Ѵ�.
        options.CallbackPath = "/Account/Login/Google/Callback";

        options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents()
        {
            OnTicketReceived = async (context) =>
            {
                // Ŭ���� �߰� 
                //context.Principal.Identities.First().AddClaim(
                //    new Claim(SharedValues.IdServer4.UserIdClaim, "From Google"));
            }
        };

    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// �õ嵥���� �߰�
using(var scope = app.Services.CreateScope())
{
    // �⺻ ����� ���
    var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
    var user = await userManager.FindByNameAsync("bob");
    if(user == null)
    {
        user = new ApplicationUser() 
        {
            UserName ="bob",
            DisplayUserName = "I am bob", 
            Deleted = true 
        };

        var result = userManager.CreateAsync(user, "bob").GetAwaiter().GetResult();

        userManager.AddClaimAsync(user, new System.Security.Claims.Claim(Constants.Claim_Grandma, "big.cookie"))
            .GetAwaiter().GetResult();


        // �Ʒ� �ΰ��� Ŭ������ access_token�� ���Խ�Ű�� ���ؼ��� 
        // ApiResouce�� UserClaim�� �߰��ؾ� �Ѵ�.
        userManager.AddClaimAsync(user, new System.Security.Claims.Claim(Constants.Claim_ApiOne_UserId, "Bruce"))
            .GetAwaiter().GetResult();
        userManager.AddClaimAsync(user, new System.Security.Claims.Claim(Constants.Claim_ApiOne_UserGrade, "Admin"))
            .GetAwaiter().GetResult();
    }

    // IdentityServer4 �õ嵥���� �߰�
    scope.ServiceProvider.GetRequiredService<AppPersistedGrantDbContext>().Database.Migrate();

    var context = scope.ServiceProvider.GetRequiredService<AppConfigurationDbContext>();
    context.Database.Migrate();

    //��� �ο� ����
    context.Clients.RemoveRange(context.Clients.ToArray());
    context.ClientCorsOrigins.RemoveRange(context.ClientCorsOrigins.ToArray());
    context.IdentityResources.RemoveRange(context.IdentityResources.ToArray());
    context.ApiScopes.RemoveRange(context.ApiScopes.ToArray());
    context.ApiResources.RemoveRange(context.ApiResources.ToArray());
    context.SaveChanges();

    if (!context.Clients.Any())
    {
        foreach (var client in Configuration.GetClients())
        {
            context.Clients.Add(client.ToEntity());
        }
        context.SaveChanges();
    }

    if (!context.IdentityResources.Any())
    {
        foreach (var resource in Configuration.GetIdentityResources())
        {
            context.IdentityResources.Add(resource.ToEntity());
        }
        context.SaveChanges();
    }

    if (!context.ApiScopes.Any())
    {
        foreach (var resource in Configuration.GetScopes())
        {
            context.ApiScopes.Add(resource.ToEntity());
        }
        context.SaveChanges();
    }

    if (!context.ApiResources.Any())
    {
        foreach (var resource in Configuration.GetApis())
        {
            context.ApiResources.Add(resource.ToEntity());
        }
        context.SaveChanges();
    }
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

app.UseIdentityServer();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
