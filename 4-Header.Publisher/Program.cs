using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Shared;

var factory = new ConnectionFactory();

//https://customer.cloudamqp.com/instance/ adresindeki instance'a tıklanıp AMQP Url kopyalanır.
factory.Uri = new Uri("amqps://irmzupyn:SOCHv673h3Yq-VdJWSj97fnJHz23uDC6@moose.rmq.cloudamqp.com/irmzupyn");

using var connection = factory.CreateConnection();

var channel = connection.CreateModel();

//durable = "false" => memory üzerinde tutulur, rabbitmq restart atılırsa memory'deki bu exchange kaybolur.
//durable = "true"  => fiziksel olarak kaydedilir, rabmq restart atılırsa dahi bu exchange kaybolmaz.
//exchange'i her zaman producer oluşturur.
channel.ExchangeDeclare("header-exchange", durable: true, type: ExchangeType.Headers);
//bilgiler artık route'da değil, header olarak gönderilir.

Dictionary<string, object> headers = new();
headers.Add("format", "pdf");
// headers.Add("shape", "a4");

//consumerdaki header match-type "all" ise shape2 bulamadığından dolayı mesajı/datayı almaz. "any" ise alır.
headers.Add("shape2", "a4");

//PROPERTIES: fanout, direct ya da topic'te de kullanılabilir, mesajlar aşağıdaki gibi kalıcı hale getirilebilir.
var properties = channel.CreateBasicProperties();
properties.Headers = headers;

//mesajları kalıcı hale getirmek için bunu belirtiriz. RabbitMQ restart etse bile mesajlar kaybolmayacak.
properties.Persistent = true;

var product = new Product
{
    Id = 1, Name = "Book", Price = 30, Stock = 100
};
var productJsonString = JsonSerializer.Serialize(product);

//hız için küçük mesajlar göndermeye dikkat.
var yourMessage = Encoding.UTF8.GetBytes(productJsonString); //resim, pdf, metin ne gönderilecekse byte[]'e çevir, gönder.

//kuyruğu consumer tarafında oluşturacağız.(optional)
channel.BasicPublish("header-exchange", string.Empty, properties, yourMessage); //mesajın publish edilmesi.
Console.WriteLine("Your message has been sent");
Console.ReadLine();
