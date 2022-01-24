using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Receive
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", DispatchConsumersAsync = true };
            var randomGenerator = new Random();

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

                    int consumer1Count = 0;
                    int consumer2Count = 0;

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.Received += async (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body.ToArray());
                        Console.WriteLine(" [x] Consumer 1 - Started {0}", message);
                        await Task.Delay(5000);
                        consumer1Count++;
                        Console.WriteLine(" [x] Consumer 1 - Finished {0}", message);
                        Console.WriteLine($"Consumer 1: {consumer1Count}");
                    };
                    channel.BasicQos(0, 10, true);
                    channel.BasicConsume(
                        queue: "hello",
                        autoAck: true,
                        consumer: consumer);

                    using (var channel2 = connection.CreateModel())
                    {
                        var consumer2 = new AsyncEventingBasicConsumer(channel2);
                        consumer2.Received += async (model, ea) =>
                        {
                            var body = ea.Body;
                            var message = Encoding.UTF8.GetString(body.ToArray());
                            Console.WriteLine(" [x] Consumer 2 - Started {0}", message);
                            Console.WriteLine(" [x] Consumer 2 - Finished {0}", message);
                            consumer2Count++;
                            Console.WriteLine($"Consumer 2: {consumer2Count}");
                        };
                        channel2.BasicConsume(
                            queue: "hello",
                            autoAck: true,
                            consumer: consumer2);



                        int i = 0;
                        while (i < 100)
                        {
                            channel.BasicPublish(exchange: "", routingKey: "hello", channel.CreateBasicProperties(), Encoding.UTF8.GetBytes(i.ToString()));
                            i++;
                            Console.WriteLine("Published " + i.ToString());
                        }

                        Console.ReadLine();
                    }
                }
            }
        }
    }
}
