
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MessagingExample.Controllers;

using Microsoft.AspNetCore.Mvc;

[Route("api/messages")]
[ApiController]
public class MessageController : ControllerBase
{
    private readonly ConnectionFactory _factory;

    public MessageController()
    {
        _factory = new ConnectionFactory() { HostName = "localhost" };
    }

    [HttpPost]
    public IActionResult SendMessage([FromBody] MessageDto message)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: "email_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        channel.BasicPublish(exchange: "", routingKey: "email_queue", basicProperties: null, body: body);

        return Ok(new { status = "Message sent to queue" });
    }
}

public class MessageDto
{
    public string From { get; set; }
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}
