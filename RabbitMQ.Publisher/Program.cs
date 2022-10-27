using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory();

//https://customer.cloudamqp.com/instance/ adresindeki instance'a tıklanıp AMQP Url kopyalanır.
factory.Uri = new Uri("yukarıdaki urlden kopyala");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();
//durable = "false" => memory üzerinde tutulur, rabbitmq restart atılırsa memory'deki bu kuyruk gider.
//durable = "true"  => fiziksel olarak kaydedilir, rabmq restart atılırsa dahi bu kuyruk kaybolmaz.
//exclusive="true"  => bu kuyruğa yalnızca yaratılan channel üzerindem erişilir.
//exclusive="false" => ise bu kuyruğa subscriber(farklı kanallar) üzerinden de erişilebilir.
//autoDelete="true" => subscribe channelı silinirse bu kuyruk silinsin mi ? hayır diyoruz.
channel.QueueDeclare("hello-queue", true, false, false);

//tek sefer calistiginda 50 tane message gidecek.
Enumerable.Range(1,50).ToList().ForEach(x =>
{
    string message = $"Message {x}"; //rabbitmq'ya byte array şeklinde gönderilir. pdf, resim hersey boyle gonderilebilir.

    var messageBody = Encoding.UTF8.GetBytes(message);

    //exchange başlangıçta kullanmıyoruz o yüzden empty deriz.
    channel.BasicPublish(string.Empty, "hello-queue", null, messageBody);
    Console.WriteLine($"Your message has been sent. {message}");
});
Console.ReadLine();


