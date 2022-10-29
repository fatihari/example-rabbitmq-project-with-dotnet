using RabbitMQ.Client;

namespace CreateFileWorkerService.Services;

public class RabbitMqClientService : IDisposable
    {
    private readonly ConnectionFactory _connectionFactory;
    private IConnection _connection;
    private IModel _channel;

    public static string QueueName = "queue-excel-file";

    private readonly ILogger<RabbitMqClientService> _logger;

    public RabbitMqClientService(ConnectionFactory connectionFactory, ILogger<RabbitMqClientService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public IModel Connect()
    {
        _connection = _connectionFactory.CreateConnection();


        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        _channel = _connection.CreateModel();

        _logger.LogInformation("Connection to RabbitMQ established...");

        return _channel;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();

        _connection?.Close();
        _connection?.Dispose();

        _logger.LogInformation("Connection to RabbitMQ terminated...");

    }
}
