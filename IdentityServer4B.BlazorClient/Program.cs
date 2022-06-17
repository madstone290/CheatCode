using IdentityServer4B.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("All", builder =>
     builder.WithOrigins(Constants.ServerAddress)
                  .WithMethods("Post")
                  //.AllowAnyHeader()
                  .AllowCredentials()
                  );
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = Constants.Cookie;
    options.DefaultChallengeScheme = Constants.OpenIdConnect;
})
    .AddCookie(Constants.Cookie, options =>
    {
        options.Cookie.Name = "IdentityServer.BlazorClient.Cookie";
    })
    .AddOpenIdConnect(Constants.OpenIdConnect, options =>
    {
        options.Authority = Constants.ServerAddress;
        options.ClientId = Constants.Client_3_Id;
        options.ClientSecret = Constants.Client_3_Secret;
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
        // refresh_token�� ��û�Ѵ�.
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
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseCors("All");

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
