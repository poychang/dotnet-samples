/**********************************************************
 * 此專案僅作為示範 ASP.NET Core 中啟用 Session 功能的範例
 * 開發 Web API，通常會建議使用 Token（如 JWT） 來取代 Session，
 * 因為它更符合 RESTful 的無狀態設計原則
 **********************************************************/
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDistributedMemoryCache(); // 用於儲存 Session 的記憶體快取
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session 過期時間
    options.Cookie.Name = "demo.session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseSession(); // 必須在 UseEndpoints 之前呼叫

app.MapGet("/enable-session", (HttpContext ctx) =>
{
    ctx.Session.SetString("State", Convert.ToHexString(RandomNumberGenerator.GetBytes(16)));
    ctx.Session.SetString("LastAccessed", DateTime.Now.ToString());

    return new
    {
        Now = DateOnly.FromDateTime(DateTime.Now),
        Random = Random.Shared.Next(0, 10),
        State = ctx.Session.GetString("State"),
        LastAccessed = ctx.Session.GetString("LastAccessed"),
    };
});

app.Run();
