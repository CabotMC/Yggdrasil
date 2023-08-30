using Docker.DotNet;

namespace Yggdrasil.Model;

public class InstanceManager
{
    private static DockerClient _client;

    public static void Init()
    {
        _client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
            .CreateClient();
    }
}