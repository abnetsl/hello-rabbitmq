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

                    channel.BasicQos(0, 1, false);

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.Received += async (model, ea) =>
                    {
                        _ = Task.Run(async () =>
                        {
                            var body = ea.Body;
                            var message = Encoding.UTF8.GetString(body.ToArray());
                            Console.WriteLine(" [x] Consumer 1 - Started {0}", message);
                            await Task.Delay(5000);
                            consumer1Count++;
                            Console.WriteLine(" [x] Consumer 1 - Finished {0}", message);
                            Console.WriteLine($"Consumer 1: {consumer1Count}");

                            channel.BasicAck(ea.DeliveryTag, false);

                        });
                    };

                    channel.BasicConsume(
                        queue: "hello",
                        autoAck: false,
                        consumer: consumer);

                    var consumer2 = new AsyncEventingBasicConsumer(channel);
                    consumer2.Received += async (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body.ToArray());
                        Console.WriteLine(" [x] Consumer 2 - Started {0}", message);
                        Console.WriteLine(" [x] Consumer 2 - Finished {0}", message);
                        consumer2Count++;
                        await Task.Delay(100);
                        Console.WriteLine($"Consumer 2: {consumer2Count}");
                        channel.BasicAck(ea.DeliveryTag, false);
                    };

                    channel.BasicConsume(
                        queue: "hello",
                        autoAck: false,
                        consumer: consumer2);

                    int i = 0;
                    using (var channel3 = connection.CreateModel())
                    {
                        while (i < 100)
                        {
                            channel3.BasicPublish(exchange: "", routingKey: "hello", channel.CreateBasicProperties(), Encoding.UTF8.GetBytes(i.ToString()));
                            i++;
                            Console.WriteLine("Published " + i.ToString());
                        }
                    }

                    while (true)
                    {

                    }
                }
            }

        }
    }
}
