using System.Threading;
using System.Threading.Tasks;

namespace MessageBox.Tests.Helpers
{
    public class SampleConsumer : 
        IHandler<SampleModel, SampleModelReply>,
        IHandler<SampleModelThatRaisesException>
    {
        public AutoResetEvent HandleCalled { get; } = new AutoResetEvent(false);
        public int HandleCallCount { get; private set; }

        public Task<SampleModelReply> Handle(IMessageContext<SampleModel> messageContext, CancellationToken cancellationToken = default)
        {
            HandleCallCount++;
            HandleCalled.Set();
            return Task.FromResult(new SampleModelReply($"Hello {messageContext.Model.Name} {messageContext.Model.Surname}!"));
        }

        public Task Handle(IMessageContext<SampleModelThatRaisesException> messageContext, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}