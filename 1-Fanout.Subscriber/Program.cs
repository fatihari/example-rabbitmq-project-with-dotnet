using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory();

//https://customer.cloudamqp.com/instance/ adresindeki instance'a tıklanıp AMQP Url kopyalanır.
factory.Uri = new Uri("amqps://irmzupyn:SOCHv673h3Yq-VdJWSj97fnJHz23uDC6@moose.rmq.cloudamqp.com/irmzupyn");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

//exclusive="true"  => bu kuyruğa yalnızca yaratılan channel üzerindem erişilir.
//exclusive="false" => ise bu kuyruğa subscriber(farklı kanallar) üzerinden de erişilebilir.
//autoDelete="true" => subscribe channelı silinirse bu kuyruk silinsin mi ? hayır diyoruz.

//durable = "false" => memory üzerinde tutulur, rabbitmq restart atılırsa memory'deki bu exchange kaybolur.
//durable = "true"  => fiziksel olarak kaydedilir, rabmq restart atılırsa dahi bu exchange kaybolmaz.
//publisherda zaten oluşturduk, ancak burada da olması(parametreler aynı olmalı) ha ta vermez. Boşuna çalıştırmayalım yine de.
// channel.ExchangeDeclare("logs-fanout", durable: true, type: ExchangeType.Fanout);

var randomQueueName = channel.QueueDeclare().QueueName; //rasgele kuyruk ismi oluşturur.
// var randomQueueName = "log-database-save-queue"; //kalıcı kuyruk ismi
// channel.QueueDeclare(randomQueueName, true, false, false); //kalıcı bir kuyruk yaratmak için

channel.QueueBind(randomQueueName, "logs-fanout", "", null); //kapanınca kuyruğu siler.

//prefetchSize:  0 => herhangi boyuttaki mesajı gönderebilirsin.
//prefetchCount: 6 => 6'şar 6'şar YA DA toplam 6 tane göndersin. hangisini seçeceğini "global" belirler.
//global: false    => her bir consumer'a 6'şar tane gönderilir.
//global: true     => tüm consumerlar'a toplam 6 tane gönderilir. 2 consumer varsa, ilkine 3, ikincisine 3 tane.
//biz takibi kolay olsun diye 1 tane gönderiyoruz şimdilik.
//2 adet consumer oluşturmak için 2 adet terminalde subscriber konumunda "dotnet run" çalıştırılır.(Socket Program)
//ÖNEMLİ: tüm logların hepsini 2 consumera da gönderir!
channel.BasicQos(0,1,false);

var consumer = new EventingBasicConsumer(channel);

//autoAck: true => message consumer'a geldiği gibi otomatik silmesi için.
//autoAck: false=> rabbitmq sen mesajı silme ben sana haber edeceğim zaman mesajı sil.
channel.BasicConsume(randomQueueName, false, consumer);

Console.WriteLine("Loglar Dinleniyor...");

//message consumer'a (subscriber'a) ulaştığında event fırlatılır.
consumer.Received += (sender, e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.ToArray());
    Thread.Sleep(1000);
    Console.WriteLine($"Incoming Message is: {message}");

    //e.DeliveryTag   => ilgili tag rabbitmq'ya gönderilir. RabbitMq bu tage sahip mesajı bulup siler.
    //multiple: true  => memory'de işlenmiş ama rabbitmq'ya gitmemiş mesajlar da varsa, o bilgileri rabbitmq'ya haberdar eder.
    //multiple: false => yalnızca ilgili mesajın durumunu rabbitmq'ya bildirir.
    channel.BasicAck(e.DeliveryTag, false);
};
Console.ReadLine();

