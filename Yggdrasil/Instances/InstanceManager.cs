using Docker.DotNet;
using Docker.DotNet.Models;
using Yggdrasil.Model;
using Yggdrasil.Redis;

namespace Yggdrasil.Instances;

public abstract class InstanceManager
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
        await db.Set("instance:" + instance.Name, instance);
        db.Publish("instanceStatusChanged", instance.Name + ":" + instance.Status);
        var env = new [] { "INSTANCE=" + instance.Name }
            .Concat(request.EnviromentVars.Select(k => k.Key + "=" + k.Value))
            .ToList();
        var bindings = new Dictionary<string, IList<PortBinding>>();
        foreach (var kv in request.OpenPorts)
        {
            bindings.Add(kv.Key + "/tcp", new List<PortBinding>()
            {
                new()
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
            HostConfig = new HostConfig
            {
                AutoRemove = true,
                PortBindings = bindings
            },
            Env = env,
            Labels = new Dictionary<string, string>
            {
                { "yggdrasil.instance", instance.Name },
                { "yggdrasil.externalAddress", instance.ExternalAddress },
                { "yggdrasil", "managed" }
            }
        };
        if (request.AttachedVolume != null)
        {
            containerStartInfo.HostConfig
                .Binds = new List<string> { request.AttachedVolume + ":/data" };
        }

        var container = await _client.Containers.CreateContainerAsync(
            containerStartInfo
        );
        
        await _client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
        await _client.Networks.ConnectNetworkAsync("mcnet", new NetworkConnectParameters()
        {
            Container = container.ID
        });

        instance.Status = InstanceStatus.Starting;
        await db.Set("instance:" + instance.Name, instance);
        db.Publish("instanceStatusChanged", instance.Name + ":" + instance.Status);
        return instance;
    }

    public static async Task<List<Instance>> GetAllInstances()
    {
        var db = RedisManager.Connection.GetDatabase();
        var runningContainers = await _client.Containers.ListContainersAsync(new ContainersListParameters()
        {
            // only list containers with the yggdrasil label
            Filters = new Dictionary<string, IDictionary<string, bool>>
            {
                {
                    "label", new Dictionary<string, bool>
                    {
                        { "yggdrasil", true }
                    }
                }
            }
        });
        if (runningContainers == null)
        {
            return new List<Instance>();
        }
        var scanResult = runningContainers.Select(c => c.Names[0][1..]);
        return (from key in scanResult select db.Get<Instance>("instance:" + key)).ToList();
    }

    public static async Task<IList<ImagesListResponse>> GetAvailableImages()
    {
        var images = await _client.Images.ListImagesAsync(new ImagesListParameters()
        {
            Filters = new Dictionary<string, IDictionary<string, bool>>()
            {
                {
                    "label", new Dictionary<string, bool>()
                    {
                        { "minecraftVersion", true }
                    }
                }
            }
        });
        return images ?? new List<ImagesListResponse>();
    }
}