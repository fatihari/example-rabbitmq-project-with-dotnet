using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Shared;

namespace CreateExcelApp.Services;

public class RabbitMqProducer
{
    private readonly RabbitMqClientService _rabbitMqClientService;

    public RabbitMqProducer(RabbitMqClientService rabbitMqClientService)
    {
        _rabbitMqClientService = rabbitMqClientService;
    }

    //event gonderme islemi
    public void Publish(CreateExcelMessage createExcelMessage)
    {
        var channel = _rabbitMqClientService.Connect();

        var bodyString = JsonSerializer.Serialize(createExcelMessage);
        var bodyByte = Encoding.UTF8.GetBytes(bodyString);

        //messagelar kalıcı olarak durması için restart atılsa dahi
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(exchange: RabbitMqClientService.ExchangeName, routingKey: RabbitMqClientService.RoutingExcel, basicProperties: properties, body: bodyByte);
    }
}
