namespace Yggdrasil.Model;

public class Instance
{
    public string Name { get; set; }
    public string Host { get; set; }
    public string Image { get; }
    /**
     * The amount of memory in MB
     */
    public int Memory { get; set; }
    public InstanceStatus Status { get; set; }
    
}

public enum InstanceStatus
{
    Provisioning,
    Starting,
    Ready,
    Stopping,
    Frozen
}