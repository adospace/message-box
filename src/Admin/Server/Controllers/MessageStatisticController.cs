using MessageBox.Server.Tcp.Host.Shared;
using Microsoft.AspNetCore.Mvc;

namespace MessageBox.Server.Tcp.Host.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageStatisticController : ControllerBase
    {
        private readonly IBusServerControl _busServerControl;
        private readonly ILogger<MessageStatisticController> _logger;

        public MessageStatisticController(IBusServerControl busServerControl, ILogger<MessageStatisticController> logger)
        {
            _busServerControl = busServerControl;
            _logger = logger;
        }

        [HttpGet("message-count")]
        public ServerMessageCountStatistic GetMessageCount()
        {
            var queueControls = _busServerControl.GetQueues();
            var exchangeControls = _busServerControl.GetExchanges();
            var timeStamp = DateTime.UtcNow;

            return new ServerMessageCountStatistic(
                queueControls.Select(_ => new QueueMessageCountStatistic(_.Id, timeStamp, _.GetTotalMessageCount()))
                    .ToArray(),
                exchangeControls
                    .Select(_ => new ExchangeMessageCountStatistic(_.Key, timeStamp, _.GetTotalMessageCount()))
                    .ToArray());
        }
        
        [HttpGet("queues-message-count")]
        public IEnumerable<QueueMessageCountStatistic> GetQueuesMessageCount()
        {
            var queueControls = _busServerControl.GetQueues();
            
            var timeStamp = DateTime.UtcNow;

            return queueControls.Select(_ => new QueueMessageCountStatistic(_.Id, timeStamp, _.GetTotalMessageCount()));
        }
        
        [HttpGet("exchanges-message-count")]
        public IEnumerable<ExchangeMessageCountStatistic> GetExchangesMessageCount()
        {
            var exchangeControls = _busServerControl.GetExchanges();
            
            var timeStamp = DateTime.UtcNow;

            return exchangeControls.Select(_ => new ExchangeMessageCountStatistic(_.Key, timeStamp, _.GetTotalMessageCount()));
        }
                
        [HttpGet("queue-message-count")]
        public IActionResult GetQueueMessageCount(Guid id)
        {
            var queueControl = _busServerControl.GetQueues().FirstOrDefault(_ => _.Id == id);

            if (queueControl == null)
            {
                return NotFound();
            }
            
            return Ok(new QueueMessageCountStatistic(queueControl.Id, DateTime.UtcNow, queueControl.GetTotalMessageCount()));
        }
        
        [HttpGet("exchange-message-count")]
        public IActionResult GetExchangeMessageCount(string key)
        {
            var exchangeControl = _busServerControl.GetExchanges().FirstOrDefault(_ => _.Key == key);

            if (exchangeControl == null)
            {
                return NotFound();
            }

            return Ok(new ExchangeMessageCountStatistic(exchangeControl.Key, DateTime.UtcNow, exchangeControl.GetTotalMessageCount()));
        }
        
    }
}