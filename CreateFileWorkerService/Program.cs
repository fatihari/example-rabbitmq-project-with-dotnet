using CreateFileWorkerService;
using CreateFileWorkerService.Models;
using CreateFileWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

IHost host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices((hostContext, services) =>
                  {
                      IConfiguration Configuration = hostContext.Configuration;

                      services.AddDbContext<AdventureworksContext>(options =>
                      {
                          options.UseNpgsql(Configuration.GetConnectionString("Npgsql"));
                      });
                      services.AddSingleton(sp => new ConnectionFactory()
                      {
                          //https://customer.cloudamqp.com/instance/ adresindeki instance'a tıklanıp AMQP Url kopyalanır.
                          //DispatchConsumersAsync = true Backgroundservise'de consumer async yazıldığı için
                          Uri = new Uri(Configuration.GetConnectionString("RabbitMQ")), DispatchConsumersAsync = true
                      });
                      services.AddSingleton<RabbitMqClientService>();
                      services.AddHostedService<Worker>();
                  })
                 .Build();

await host.RunAsync();
