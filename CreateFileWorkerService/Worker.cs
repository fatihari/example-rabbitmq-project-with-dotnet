using System.Data;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using CreateFileWorkerService.Models;
using CreateFileWorkerService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using Product = CreateFileWorkerService.Models.Product;

namespace CreateFileWorkerService;

public class Worker : BackgroundService
{
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMqClientService _rabbitMqClientService;
        private readonly IServiceProvider _serviceProvider; //Adventureworks scope olarak programa eklendiği için alamıyoruz.
        private IModel _channel;

        public Worker(ILogger<Worker> logger, RabbitMqClientService rabbitMqClientService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rabbitMqClientService = rabbitMqClientService;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMqClientService.Connect();
            _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }

        protected override  Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += Consumer_Received;
            _channel.BasicConsume(queue: RabbitMqClientService.QueueName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            await Task.Delay(5000);

            //kuyruktan mesaj alınır. Bize bu mesaj FileId doner
            var createExcelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));

            using var ms = new MemoryStream(); //oluşturulan excel memorystreamde (RAM'de) tutulur.

            var wb = new XLWorkbook();
            var ds = new DataSet();
            ds.Tables.Add(GetTable("products"));
            wb.Worksheets.Add(ds);
            wb.SaveAs(ms);

            MultipartFormDataContent multipartFormDataContent = new();

            //filescontroller'daki upload parametresi olan "file" ile aynı olmalı
            multipartFormDataContent.Add(new ByteArrayContent(ms.ToArray()), "file", Guid.NewGuid().ToString() + ".xlsx");

            const string baseUrl = "https://localhost:7093/api/files";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync($"{baseUrl}?fileId={createExcelMessage.FileId}", multipartFormDataContent);

                if(response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("File(Id : {FileId}) was created by successful", createExcelMessage.FileId);
                    // _logger.LogInformation($"File ( Id : {createExcelMessage.FileId}) was created by successful");
                    _channel.BasicAck(@event.DeliveryTag, false);
                }
            }
        }

        private DataTable GetTable(string tableName)
        {
            List<Product> products;

            using (var scope= _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AdventureworksContext>();

                products = context.Products.ToList();
            }

            var table = new DataTable { TableName = tableName };

            //onca sütun arasından 4 tanesini seçiyoruz.
            table.Columns.Add("ProductId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("ProductNumber", typeof(string));
            table.Columns.Add("Color", typeof(string));

            products.ForEach(x =>
            {
                table.Rows.Add(x.Productid, x.Name, x.Productnumber, x.Color);
            });
            return table;
        }
    }
