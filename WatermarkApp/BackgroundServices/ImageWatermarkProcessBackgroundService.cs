using System.Drawing;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WatermarkApp.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WatermarkApp.BackgroundServices;

public class ImageWatermarkProcessBackgroundService:BackgroundService
{
    private readonly RabbitMQClientService _rabbitMqClientService;
    private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;
    private IModel _channel; //constructroda set etmediğimiz için readonly eklemiyoruz.

    public ImageWatermarkProcessBackgroundService(RabbitMQClientService rabbitMqClientService, ILogger<ImageWatermarkProcessBackgroundService> logger)
    {
        _rabbitMqClientService = rabbitMqClientService;
        _logger = logger;
    }
    //override yazıp ekleriz.
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        //RabbitMQ'ya Bağlanma
        _channel = _rabbitMqClientService.Connect();
        // prefetchSize = 0 => message boyutu onemli değil. prefetchCount: 1 => 1'er 1'er alacağız
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, false);
        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //Consumer'ı oluşturma.
        var consumer = new AsyncEventingBasicConsumer(_channel); //consumer'ı producer'un channel'ına bağlarız.

        //Consumer tarafında Event Dinleme
        consumer.Received += Consumer_Received;

        _channel.BasicConsume(queue: RabbitMQClientService.QueueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
    {
        Task.Delay(5000);
        try
        {
            var productImageCreatedEvent =
                JsonSerializer.Deserialize<ProductImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", productImageCreatedEvent.ImageName);
            var siteName = "www.mysite.com";
            //adding Watermark
            using var img = Image.FromFile(path);
            using var graphic = Graphics.FromImage(img);
            var font = new Font(FontFamily.GenericMonospace, 40, FontStyle.Bold, GraphicsUnit.Pixel);
            var textSize = graphic.MeasureString(siteName, font);
            var color = Color.FromArgb(128, 255, 255, 255);
            var brush = new SolidBrush(color);
            var position = new Point(img.Width - ((int) textSize.Width + 30), img.Height - ((int) textSize.Height + 30));
            graphic.DrawString(siteName, font, brush, position);
            img.Save("wwwroot/Images/watermarks/" + productImageCreatedEvent.ImageName);

            //işlemler bitince sonlandırırız memoryde yer tutmaması için.
            img.Dispose();
            graphic.Dispose();
            _channel.BasicAck(@event.DeliveryTag, false); //sadece bu rabbitmq'ye mesajı bildirir.
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return Task.CompletedTask;

    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}
