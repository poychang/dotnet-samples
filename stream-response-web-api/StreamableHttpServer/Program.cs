var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.MapGet("/stream", async (HttpContext context) =>
{
    context.Response.Headers.Append("Content-Type", "text/event-stream");

    for (int i = 1; i <= 10; i++)
    {
        var data = $"data: Server Time: {DateTime.Now:HH:mm:ss}\n\n";
        var bytes = System.Text.Encoding.UTF8.GetBytes(data);
        await context.Response.Body.WriteAsync(bytes);
        await context.Response.Body.FlushAsync(); // 強制 flush

        await Task.Delay(1000); // 每秒推送一次
    }
});

app.Run();
