using StackExchange.Redis;

namespace Yggdrasil.Redis;

public class RedisManager
{
    private static ConnectionMultiplexer? connection;

    public static void Init(string address)
    {
        connection = ConnectionMultiplexer.Connect(address);
        connection.ConnectionFailed += (a, b) => Console.WriteLine(a);
        connection.ConnectionRestored += (_, _) => Console.WriteLine("Redis Connected");
    }

    public void GetInstance(string name)
    {
        
    }
}