namespace Yggdrasil.Model;

public class Instance
{
    public string Name { get; set; }
    public string Image { get; set;  }
    public InstanceStatus Status { get; set; }
    public IsolationMode IsolationMode { get; set; }
    public string? AttachedVolume { get; set; }
    
}

public enum InstanceStatus
{
    Provisioning,
    Starting,
    Ready,
    Stopping,
    Frozen
}
public enum IsolationMode
{
    /**
     * Instance added to velocity and can be connected to directly or by transferring from another server
     */
    None,
    /**
     * Players cannot be transferred to or from this image, but universal chat is still available
     */
    ChatOnly,
    /**
     * This instance is completely isolated from the rest of the network, and can only be connected to directly
     */
    All
}