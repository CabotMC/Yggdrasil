namespace Yggdrasil.Model;

public class Instance
{
    public string Name { get; set; }
    public string Image { get; set;  }
    public InstanceStatus Status { get; set; }
    public IsolationMode IsolationMode { get; set; }
    public string? AttachedVolume { get; set; }
    
    public string ExternalAddress;

}

public enum InstanceStatus
{
    Provisioning,
    Starting,
    Ready,
    Stopping,
    Frozen
}
[Flags]
public enum IsolationMode
{
    ChatRestricted = 1,
    TransferRestricted = 2,
    NoDirectAccess = 4
}