using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();

app.MapGet("/read", async () =>
{
    var factory = new ConnectionFactory { HostName = "rabbitmq", Port = 5672 };
    using var connection = await factory.CreateConnectionAsync();
    using var channel = await connection.CreateChannelAsync();

    await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false,
        arguments: null);

    var consumer = new AsyncEventingBasicConsumer(channel);
    consumer.ReceivedAsync += (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine($" [x] Received {message}");
        return Task.CompletedTask;
    };

    var result = await channel.BasicConsumeAsync("hello", autoAck: true, consumer: consumer);

    return Results.Ok(result);
})
.WithName("ReadQueue");

app.Run();

