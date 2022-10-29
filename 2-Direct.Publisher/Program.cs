using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory();

//https://customer.cloudamqp.com/instance/ adresindeki instance'a tıklanıp AMQP Url kopyalanır.
factory.Uri = new Uri("amqps://irmzupyn:SOCHv673h3Yq-VdJWSj97fnJHz23uDC6@moose.rmq.cloudamqp.com/irmzupyn");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

//durable = "false" => memory üzerinde tutulur, rabbitmq restart atılırsa memory'deki bu exchange kaybolur.
//durable = "true"  => fiziksel olarak kaydedilir, rabmq restart atılırsa dahi bu exchange kaybolmaz.
channel.ExchangeDeclare("logs-direct", durable: true, type: ExchangeType.Direct);

Enum.GetNames(typeof(LogNames)).ToList().ForEach(x =>
{
    var routeKey = $"route-{x}"; //routeKey'de ne yazıyorsa queue'da onun adı yazılır.
    var queueName = $"direct-queue-{x}"; //routekey'de yazan x değeri burada yazılmalıdır.

    //Kuyruğu bu sefer consumer değil, publisherda gerçekleştiriyoruz.(Tercih meselesi)
    //exclusive="true"  => bu kuyruğa yalnızca yaratılan channel üzerindem erişilir.
    //exclusive="false" => ise bu kuyruğa subscriber(farklı kanallar) üzerinden de erişilebilir.
    //autoDelete="true" => subscribe channelı silinirse bu kuyruk silinsin mi ? hayır diyoruz.
    channel.QueueDeclare(queueName, true, false, false);
    channel.QueueBind(queueName, "logs-direct", routeKey, null);
});

//tek sefer calistiginda 50 tane message gidecek.
Enumerable.Range(1,50).ToList().ForEach(x =>
{
    LogNames logName = (LogNames) new Random().Next(1, 5); //rasgele log seçilir.
    string message = $"log-type: {logName}"; //rabbitmq'ya byte array şeklinde gönderilir. pdf, resim hersey boyle gonderilebilir.

    var messageBody = Encoding.UTF8.GetBytes(message);

    var routeKey = $"route-{logName}";

    //queue başlangıçta kullanmazsak o yüzden routingKey'i boş bırakırız. Ancak kuyruk olmadığı zaman mesajlar boşa gider.
    channel.BasicPublish("logs-direct", routeKey, null, messageBody);
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
