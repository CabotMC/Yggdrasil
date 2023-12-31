using System.Net.NetworkInformation;
using System.Net.Sockets;
using Yggdrasil.Instances;
using Yggdrasil.Redis;

// Connect Redis
RedisManager.Init("redis:6379");
InstanceManager.Init();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger";
});

foreach (var i in NetworkInterface.GetAllNetworkInterfaces())
{
    
    var addrs = i.GetIPProperties()
        .UnicastAddresses
        .Select(u => u.Address)
        .Select(u => u.ToString())
        .ToList();
    Console.WriteLine(i.Name + ": " + (addrs.Count == 0 ? "null" : addrs[0]));
}
app.UseAuthorization();

app.MapControllers();

app.Run();