using Microsoft.AspNetCore.Mvc;
using Yggdrasil.Model;

namespace Yggdrasil.Controllers;

[ApiController]
[Route("/api/instance")]
public class InstanceController
{
    [HttpGet]
    public async Task<List<Instance>> GetAllInstances()
    {
        return await InstanceManager.GetAllInstances();
    }

    [HttpPost]
    public async Task<Instance> CreateInstance([FromBody] InstanceCreateRequest request)
    {
        return await InstanceManager.CreateInstance(request);
    }
    
}