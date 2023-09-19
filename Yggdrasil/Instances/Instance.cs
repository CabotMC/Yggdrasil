namespace Yggdrasil.Instances;

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
    Provisioning = 0,
    Starting = 1,
    Ready = 2,
    Stopping = 3,
    Frozen = 4
}
[Flags]
public enum IsolationMode
{
    ChatRestricted = 1,
    TransferRestricted = 2,
    NoDirectAccess = 4
}