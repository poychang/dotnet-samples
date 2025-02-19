using SignalRSimpleApp.HostedService;
using SignalRSimpleApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddHostedService<TestEventHubService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapGet("/now", () => DateTime.Now.ToString());
app.MapHub<EventHub>("/eventHub");
app.MapHub<ChatHub>("/chatHub");

app.Run();
