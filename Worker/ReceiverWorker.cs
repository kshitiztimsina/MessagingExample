using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace MessagingExample.Worker;
public class ReceiverWorker : BackgroundService
{
    private readonly ILogger<ReceiverWorker> _logger;
    private IConnection _connection;
    private IModel _channel;

    public ReceiverWorker(ILogger<ReceiverWorker> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "email_queue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var emailRequest = JsonSerializer.Deserialize<MessageRequest>(message);

                if (emailRequest != null)
                {
                    _SendEmailAsync(emailRequest);
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: "email_queue",
                                 autoAck: false,
                                 consumer: consumer);
        

        return Task.CompletedTask;
    }

    private void _SendEmailAsync(MessageRequest request)
    {
        _logger.LogInformation("{@request}", request);
        _logger.LogInformation("Message send successfully");

    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}

public class MessageRequest
{
    public string From { get; set; }
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}



