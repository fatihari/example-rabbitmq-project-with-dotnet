using RabbitMQ.Client;

namespace CreateExcelApp.Services;

public class RabbitMqClientService:IDisposable
{
    private readonly ConnectionFactory _connectionFactory;
    private IConnection _connection;
    private IModel _channel;
    public static string ExchangeName = "excelDirectExchange";
    public static string RoutingExcel = "route-excel-file";
    public static string QueueName = "queue-excel-file";
    private readonly ILogger<RabbitMqClientService> _logger;

    // Bu client içerisinde direct exchange kullanılacak.
    // Kuyruğun ve exchange'in tanımlanması ve kuyruğun exch'e bind edilmesi hepsi burada yani Client'da olacak.

    public RabbitMqClientService(ConnectionFactory connectionFactory, ILogger<RabbitMqClientService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public IModel Connect()
    {
        _connection = _connectionFactory.CreateConnection();

        if(_channel is { IsOpen:true}) //if(_channel.IsOpen == true)
        {
            return _channel;
        }

        //0-Create channel
        _channel = _connection.CreateModel();

        //1-Create Exchange
        _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: true,  autoDelete: false);

        //2-Create Queue (exclusive: false => başka bir channel üzerinden erişilecek.)
        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete:false);

        //3-Create Mapping(Binding)
        _channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingExcel);

        _logger.LogInformation("Connection to RabbitMQ established...");

        return _channel;
    }

    public void Dispose()
    {
        _channel?.Close(); //channel null değilse, kapatılır.
        _channel?.Dispose(); //channel null değilse(var ise), dispose edilir.

        _connection?.Close();   //connection null değilse, kapatılır
        _connection?.Dispose(); //connection null değilse(var ise), dispose edilir.

        _logger.LogInformation("Connection with RabbitMQ was terminated...");

    }
}
