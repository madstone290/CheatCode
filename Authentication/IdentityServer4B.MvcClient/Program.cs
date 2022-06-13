using IdentityServer4B.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = Constants.Cookie;
    options.DefaultChallengeScheme = Constants.OpenIdConnect;
})
    .AddCookie(Constants.Cookie, options =>
    {
        options.Cookie.Name = "IdentityServer.MvcClient.Cookie";
    })
    .AddOpenIdConnect(Constants.OpenIdConnect, options =>
    {
        options.Authority = Constants.ServerAddress;
        options.ClientId = Constants.Client_2_Id;
        options.ClientSecret = Constants.Client_2_Secret;
        options.SaveTokens = true;
 
        options.ResponseType = Constants.ResponceType_Code;

        // cookie claim ���� ����
        // Ư�� claim�� ������ �� �ִ�.
        options.ClaimActions.DeleteClaim("amr");
        // ����������� ��ȸ�� �� ����� ���� json���� ������ Ű���� �����ؼ� cookie claim���� �����Ѵ�.
        // OpenIdConnectOptions.GetClaimsFromUserInfoEndpoint�Ӽ��� true�ϋ� ����ȴ�.
        options.ClaimActions.MapUniqueJsonKey(Constants.Claim_Grandma, Constants.Claim_Grandma);

        // id_token�� ����� ������ ���Խ�Ű�� �ʰ�
        // ����� ������ ��ȸ�ϱ� ���� endpoint�� �ѹ��� �ٳ�´�.
        options.GetClaimsFromUserInfoEndpoint = true;

        options.Scope.Add(Constants.Scope_OpenId);
        options.Scope.Add(Constants.Scope_ApiOne);
        options.Scope.Add(Constants.Scope_ApiTwo);
        options.Scope.Add(Constants.Scope_CustomClaim);
        options.Scope.Add(Constants.Scope_OfflineAccess);


        options.Events = new OpenIdConnectEvents()
        {
            OnUserInformationReceived = c =>
            {
                // *** ��Űũ��� ���õ� ������ �߻����� �ʴ��� OpenIdConnectOptions.SaveTokens = true�� ����� �� ***
                // OpenIdConnectOptions.SaveTokens = true �� ���� ����
                // SaveTokens���� �� 6���� ��ū�� ���Եȴ�.
                // AuthenticationProeperties�� ��ū���� �����Ѵ�.
                // �ʿ��� ��ū�� �߰��Ѵ�.
                // **����** �� �迭�� �����ϴ� ��� ��� ��ū�� �����ȴ�

                //var saveTokens = new AuthenticationToken[]
                //{
                //    new AuthenticationToken
                //    {
                //        Name = "id_token",
                //        Value = c.ProtocolMessage.IdToken
                //    },
                //    new AuthenticationToken
                //    {
                //        Name = "access_token",
                //        Value = c.ProtocolMessage.AccessToken
                //    },
                //    new AuthenticationToken
                //    {
                //        Name = "refresh_token",
                //        Value = c.ProtocolMessage.RefreshToken
                //    }
                //};
                //c.Properties.StoreTokens(saveTokens);
                return Task.CompletedTask;
            },
            OnTokenResponseReceived = c =>
            {
                return Task.CompletedTask;
            },
            OnTokenValidated = c =>
            {
               
                return Task.CompletedTask;
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
