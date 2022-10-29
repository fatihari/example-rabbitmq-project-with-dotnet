using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace WatermarkApp.Services;

public class RabbitMQProducer
{
    private readonly RabbitMQClientService _rabbitMqClientService;

    public RabbitMQProducer(RabbitMQClientService rabbitMqClientService)
    {
        _rabbitMqClientService = rabbitMqClientService;
    }

    //event gonderme islemi
    public void Publish(ProductImageCreatedEvent productImageCreatedEvent)
    {
        var channel = _rabbitMqClientService.Connect();

        var bodyString = JsonSerializer.Serialize(productImageCreatedEvent);
        var bodyByte = Encoding.UTF8.GetBytes(bodyString);

        //messagelar kalıcı olarak durması için restart atılsa dahi
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName, routingKey: RabbitMQClientService.RoutingWatermark, basicProperties: properties, body: bodyByte);
    }
}
