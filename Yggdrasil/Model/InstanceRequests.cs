namespace Yggdrasil.Model;

public class InstanceCreateRequest
{
    public string? RequestedName { get; set; }
    public string DockerImage { get; set; } = "";
    public string? AttachedVolume { get; set; }
    public IsolationMode IsolationMode { get; set; } = 0;
    public Dictionary<string, string> EnviromentVars { get; set; } = new();
    public List<string> OpenPorts = new();
}

