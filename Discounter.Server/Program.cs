using Discounter.Core;
using Discounter.Infra;
using Discounter.Infrastructure;
using Discounter.Server;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => {
    options.ListenAnyIP(5001, op => {
        op.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
        op.UseHttps();
    });
});

builder.Services.AddSignalR();

builder.Services.AddSingleton<IDiscountService, DiscountService>();
builder.Services.AddSingleton<IDiscountRepository, RedisDiscountRepository>();
builder.Services.AddSingleton<IRandomGenerator, FileSystemRandomGenerator>();

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));

var app = builder.Build();

app.MapHub<DiscountHub>("/discountHub");

app.Run();