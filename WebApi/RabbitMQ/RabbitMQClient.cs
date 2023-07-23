using System.Text;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebApi.Hub;


namespace WebApi.RabbitMQ;

public class RabbitMqClient
{
    private readonly ConnectionFactory _connectionFactory;
    private IConnection _connection;
    private IModel _channel;
    private readonly IHubContext<ContadorHub> _hubContext;

    public RabbitMqClient(IHubContext<ContadorHub> hubContext)
    {
        _hubContext = hubContext;

        _connectionFactory = new ConnectionFactory()
        {
            HostName = "rabbitmq-1",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            Port = 5672
        };
    }

    public void Connect()
    {
        _connection = _connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "minha-fila",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _hubContext.Clients.All.SendAsync("ReceiveMessage", message);

            Console.WriteLine(" [x] Received {0}", message);
        };

        _channel.BasicConsume(queue: "minha-fila",
            true,
            consumer: consumer);
    }

    public void Disconnect()
    {
        _channel.Close();
        _connection.Close();
    }

    public void SendMessageToRabbitMq(string minhaFila, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "",
            routingKey: "minha-fila",
            basicProperties: null,
            body: body);
    }
}
