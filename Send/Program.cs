using System;
using System.Text;
using RabbitMQ.Client;

namespace Send
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(
                        queue: "hello",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    string message = "Hello World!";

                    var body = Encoding.UTF8.GetBytes(message);

                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"));

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = false;

                    channel.BasicPublish(
                        exchange: "",
                        routingKey: "hello",
                        basicProperties: properties,
                        body: body);

                    Console.WriteLine(" [x] Send {0}", message);
                }
            }
        }
    }
}
