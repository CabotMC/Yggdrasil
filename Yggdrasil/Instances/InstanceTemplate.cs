namespace Yggdrasil.Model;

public class InstanceTemplate
{
    public string Name;
    public string DockerImage;
    public IsolationMode IsolationMode;
    public string? WorldName;
}

public enum IsolationMode
{
    None,
    NoSwapTo,
    NoSwapFrom,
    Both
}