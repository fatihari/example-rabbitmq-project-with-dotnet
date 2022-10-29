namespace WatermarkApp.Services;

//RabbitMQ ya message ya da event yollar. Eventler gecmis zaman olmali.
public class ProductImageCreatedEvent
{
    public string? ImageName { get; set; }
}
