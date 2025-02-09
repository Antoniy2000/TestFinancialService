using Microsoft.Extensions.Options;
using TestFinancialService.API.Options;
using TestFinancialService.API.Services;

namespace TestFinancialService.API;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.Configure<BinanceOptions>(builder.Configuration.GetSection("Binance"));
        builder.Logging.AddSimpleConsole(x =>
        {
            x.IncludeScopes = true;
            x.TimestampFormat = "[dd.MM.yyyy hh:mm:ss.ms]";
        });

        builder.Services.AddHostedService<BinanceHostedService>();
        builder.Services.AddSingleton<TickersService>();
        builder.Services.AddSingleton<WebSocketsService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapControllers();
        app.UseWebSockets(new WebSocketOptions
        {
            //KeepAliveInterval = TimeSpan.FromSeconds(30),
        });

        app.Run();
    }
}
