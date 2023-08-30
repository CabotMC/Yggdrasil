using Docker.DotNet;
using Docker.DotNet.Models;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Extensions;
using Yggdrasil.Redis;

namespace Yggdrasil.Model;

public class InstanceManager
{
    private static DockerClient _client;

    public static void Init()
    {
        _client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
            .CreateClient();
    }
    public static Instance GetInstance(string name)
    {
        return RedisManager.Connection.GetDatabase()
            .Get<Instance>("instance:" + name);
    }

    public static bool InstanceExists(string name)
    {
        return RedisManager.Connection.GetDatabase()
            .KeyExists(name);
    }

    public static async Task<Instance> CreateInstance(InstanceCreateRequest request)
    {
        var db = RedisManager.Connection.GetDatabase();
        string finalName;
        if (request.RequestedName != null && InstanceExists(request.RequestedName))
        {
            finalName = request.RequestedName + "-" + Guid.NewGuid().ToString().Split("-")[0];
        }
        else if (request.RequestedName != null)
        {
            finalName = request.RequestedName;
        }
        else
        {
            finalName = Guid.NewGuid().ToString().ToLower();
        }
        var instance = new Instance
        {
            Name = finalName,
            Image = request.DockerImage,
            Status = InstanceStatus.Provisioning,
            IsolationMode = request.IsolationMode,
            AttachedVolume = request.AttachedVolume,
            ExternalAddress = request.ExternalAddress ?? finalName
        };
        db.HashSet("addresses", instance.ExternalAddress, instance.Name);    
        db.Set("instance:" + instance.Name, instance);
        db.Publish("instanceStatusChanged", instance.Name + ":" + instance.Status);
        var env = (new string[] { "INSTANCE=" + instance.Name })
            .Concat(request.EnviromentVars.Select(k => k.Key + "=" + k.Value))
            .ToList();
        var bindings = new Dictionary<string, IList<PortBinding>>();
        foreach (var kv in request.OpenPorts)
        {
            bindings.Add(kv.Key + "/tcp", new List<PortBinding>()
            {
                new PortBinding()
                {
                    HostIP = "0.0.0.0",
                    HostPort = kv.Value.ToString()
                }
            });
        }
        var containerStartInfo = new CreateContainerParameters()
        {
            Name = instance.Name,
            Image = request.DockerImage,
            HostConfig = new HostConfig()
            {
                AutoRemove = true,
                PortBindings = bindings
            },
            Env = env
        };
        if (request.AttachedVolume != null)
        {
            containerStartInfo.HostConfig
                .Binds = new List<string> {request.AttachedVolume + ":/data"};
        }
        var container = await _client.Containers.CreateContainerAsync(
            containerStartInfo
        );
        await _client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
        instance.Status = InstanceStatus.Starting;
        db.Set("instance:" + instance.Name, instance);
        db.Publish("instanceStatusChanged", instance.Name + ":" + instance.Status);
        return instance;
    }

    public static async Task<List<Instance>> GetAllInstances()
    {
        var db = RedisManager.Connection.GetDatabase();
        var scanResult = await db.ExecuteAsync("SCAN", 0, "MATCH", "instance:*");
        if (scanResult.IsNull)
        {
            Console.WriteLine("No instances running");
            return new List<Instance>();
        }
        return (from key in (RedisResult[])scanResult select db.Get<Instance>(key.ToString())).ToList();
    }
    
}