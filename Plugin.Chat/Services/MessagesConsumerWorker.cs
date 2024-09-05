using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;



public class MessagesConsumerWorker : BackgroundService
{
    private readonly IMessagesConsumer _consumer;

    public MessagesConsumerWorker(IMessagesConsumer consumer)
    {
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.BeginConsumeAsync();
    }
}
