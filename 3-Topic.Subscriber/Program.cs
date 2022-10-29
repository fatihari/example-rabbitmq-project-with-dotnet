using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory();

//https://customer.cloudamqp.com/instance/ adresindeki instance'a tıklanıp AMQP Url kopyalanır.
factory.Uri = new Uri("amqps://irmzupyn:SOCHv673h3Yq-VdJWSj97fnJHz23uDC6@moose.rmq.cloudamqp.com/irmzupyn");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

//prefetchSize:  0 => herhangi boyuttaki mesajı gönderebilirsin.
//prefetchCount: 6 => 6'şar 6'şar YA DA toplam 6 tane göndersin. hangisini seçeceğini "global" belirler.
//global: false    => her bir consumer'a 6'şar tane gönderilir.
//global: true     => tüm consumerlar'a toplam 6 tane gönderilir. 2 consumer varsa, ilkine 3, ikincisine 3 tane.
//biz takibi kolay olsun diye 1 tane gönderiyoruz şimdilik.
//2 adet consumer oluşturmak için 2 adet terminalde subscriber konumunda "dotnet run" çalıştırılır.(Socket Program)
//ÖNEMLİ: 2 consumera aynı anda aynı datayı gönderir!
channel.BasicQos(0,1,false);

var consumer = new EventingBasicConsumer(channel);

//ÖNEMLİ: Kuyruk Consumer'da oluşturulduğu için, ilk Consumer sonra Producer çalıştırılmalı.
var queueName = channel.QueueDeclare().QueueName; //random kuyruk ismi CONSUMER'da oluşturulur.
var routeKey = "*.Error.*"; // *.Error.* => başı ve sonunu dikkat etmeksizin, ortasında Error olan route'u dinlemek istiyoruz.
var routeKey2 = "*.*.Warning"; // sonunu Warning olan route'u dinlemek istiyoruz.
var routeKey3 = "Information.#"; // başı Information sonu önemsiz (* => tek bir metin, # bir sürü metin, karakter)
channel.QueueBind(queueName, "logs-topic", routeKey3); //producer'da oluşturmuştur.

Console.WriteLine("Logs Listening...");

//message consumer'a (subscriber'a) ulaştığında event fırlatılır.
consumer.Received += (sender, e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.ToArray());
    Thread.Sleep(500);
    Console.WriteLine($"Incoming Log is: {message}");

    // File.AppendAllText("log-error.txt", message+"\n"); //Release içerisine çıkartır.

    //e.DeliveryTag   => ilgili tag rabbitmq'ya gönderilir. RabbitMq bu tage sahip mesajı bulup siler.
    //multiple: true  => memory'de işlenmiş ama rabbitmq'ya gitmemiş mesajlar da varsa, o bilgileri rabbitmq'ya haberdar eder.
    //multiple: false => yalnızca ilgili mesajın durumunu rabbitmq'ya bildirir.
    channel.BasicAck(e.DeliveryTag, false);
};
//autoAck: true => message consumer'a geldiği gibi otomatik silmesi için.
//autoAck: false=> rabbitmq sen mesajı silme ben sana haber edeceğim zaman mesajı sil.
channel.BasicConsume(queueName, false, consumer);
Console.ReadLine();

