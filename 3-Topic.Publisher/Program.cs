using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory();

//https://customer.cloudamqp.com/instance/ adresindeki instance'a tıklanıp AMQP Url kopyalanır.
factory.Uri = new Uri("amqps://irmzupyn:SOCHv673h3Yq-VdJWSj97fnJHz23uDC6@moose.rmq.cloudamqp.com/irmzupyn");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

//durable = "false" => memory üzerinde tutulur, rabbitmq restart atılırsa memory'deki bu exchange kaybolur.
//durable = "true"  => fiziksel olarak kaydedilir, rabmq restart atılırsa dahi bu exchange kaybolmaz.
//exchange'i her zaman producer oluşturur.
channel.ExchangeDeclare("logs-topic", durable: true, type: ExchangeType.Topic);

Random rnd = new();
//tek sefer calistiginda 50 tane message gidecek.
Enumerable.Range(1,50).ToList().ForEach(x =>
{
    LogNames log1 = (LogNames) rnd.Next(1, 5);
    LogNames log2 = (LogNames) rnd.Next(1, 5);
    LogNames log3 = (LogNames) rnd.Next(1, 5);

    var routeKey = $"{log1}.{log2}.{log3}"; //routeKey'de ne yazıyorsa queue'da onun adı yazılır.

    string message = $"log-type: {log1}-{log2}-{log3}"; //rabbitmq'ya byte array şeklinde gönderilir. pdf, resim hersey boyle gonderilebilir.
    var messageBody = Encoding.UTF8.GetBytes(message);

    //queue başlangıçta kullanmazsak o yüzden routingKey'i boş bırakırız. Ancak kuyruk olmadığı zaman mesajlar boşa gider.
    //kuyruk burada consumer tarafında tanımlanacak.
    channel.BasicPublish("logs-topic", routeKey, null, messageBody);
    Console.WriteLine($"The Log has been sent. {message}");
});
Console.ReadLine();

public enum LogNames
{
    Critical = 1,
    Error = 2,
    Warning = 3,
    Information = 4
}
