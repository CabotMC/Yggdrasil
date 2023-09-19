using StackExchange.Redis;
using Yggdrasil.Instances;

namespace Yggdrasil.Redis;

public abstract class RedisManager
{
    public static ConnectionMultiplexer Connection;

    public static void Init(string address)
    {
        Connection = ConnectionMultiplexer.Connect(address);
        Connection.ConnectionFailed += (a, b) => Console.WriteLine(a);
        Connection.ConnectionRestored += (_, _) => Console.WriteLine("Redis Connected");
        Connection.GetSubscriber().Subscribe("instanceStatusChanged", (channel, value) =>
        {
            var split = value.ToString().Split(":");
            var instance = split[0];
            var status = int.Parse(split[1]);
        });
    }

    
}