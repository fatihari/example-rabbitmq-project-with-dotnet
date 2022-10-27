using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory();

//https://customer.cloudamqp.com/instance/ adresindeki instance'a tıklanıp AMQP Url kopyalanır.
factory.Uri = new Uri("yukarıdaki urlden kopyala");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

//durable = "false" => memory üzerinde tutulur, rabbitmq restart atılırsa memory'deki bu exchange kaybolur.
//durable = "true"  => fiziksel olarak kaydedilir, rabmq restart atılırsa dahi bu exchange kaybolmaz.
channel.ExchangeDeclare("logs-fanout", durable: true, type: ExchangeType.Fanout);

//tek sefer calistiginda 50 tane message gidecek.
Enumerable.Range(1,50).ToList().ForEach(x =>
{
    string message = $"log {x}"; //rabbitmq'ya byte array şeklinde gönderilir. pdf, resim hersey boyle gonderilebilir.

    var messageBody = Encoding.UTF8.GetBytes(message);

    //queue başlangıçta kullanmazsak o yüzden routingKey'i boş bırakırız. Ancak kuyruk olmadığı zaman mesajlar boşa gider.
    channel.BasicPublish("logs-fanout", "", null, messageBody);
    Console.WriteLine($"Your message has been sent. {message}");
});
Console.ReadLine();


