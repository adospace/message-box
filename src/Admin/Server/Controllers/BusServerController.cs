using MessageBox.Server.Tcp.Host.Shared;
using Microsoft.AspNetCore.Mvc;

namespace MessageBox.Server.Tcp.Host.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class BusServerController : ControllerBase
{
    private readonly IBusServerControl _busServerControl;

    public BusServerController(IBusServerControl busServerControl)
    {
        _busServerControl = busServerControl;
    }

    [HttpGet("queues")]
    public IEnumerable<QueueControlModel> GetQueues()
    {
        return _busServerControl.GetQueues().Select(_ => new QueueControlModel(_.Id, _.Name));
    }
    
    [HttpGet("exchanges")]
    public IEnumerable<ExchangeControModel> GetExchanges()
    {
        var exchanges = _busServerControl.GetExchanges();

        return exchanges.Select(_ => new ExchangeControModel(_.Key,
            _.GetSubscribers().Select(queue => new QueueControlModel(queue.Id, queue.Name)).ToArray()));
    }
}