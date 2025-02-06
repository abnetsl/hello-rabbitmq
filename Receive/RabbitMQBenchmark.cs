using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Receive
{
    public class RabbitMQBenchmark
    {
        public class RabbitMQBenchMarkResult
        {
            public bool SingleChannel { get; set; } = false;

            public ushort PrefetchCount { get; set; } = 0;

            public bool AutoAck { get; set; } = false;

            public double TotalTime { get; set; } = 0;
            public int Consumer1Count { get; set; } = 0;

            public int Consumer2Count { get; set; } = 0;
        }

        public async Task<RabbitMQBenchMarkResult> Run(bool singleChannel, ushort prefetchCount, bool autoAck)
        {
            var result = new RabbitMQBenchMarkResult();
            result.SingleChannel = singleChannel;
            result.PrefetchCount = prefetchCount;
            result.AutoAck = autoAck;

            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            var randomGenerator = new Random();

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();
            var stopWatch = new System.Diagnostics.Stopwatch();

            await channel.QueueDeclareAsync(
                queue: "hello",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            await channel.QueuePurgeAsync("hello");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());
                Console.WriteLine("[{0}] Consumer 1 - Started {1}",stopWatch.ElapsedMilliseconds, message);
                await Task.Delay(3000);
                result.Consumer1Count++;
                Console.WriteLine("[{0}] Consumer 1 - Finished {1}",stopWatch.ElapsedMilliseconds, message);
                if (!autoAck)
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
            };

            await channel.BasicQosAsync(0, prefetchCount, false);
            await channel.BasicConsumeAsync(
                queue: "hello",
                autoAck: autoAck,
                consumer: consumer);

            var channel2 = singleChannel ? channel : await connection.CreateChannelAsync();
            if (!singleChannel)
            {
                await channel2.BasicQosAsync(0, prefetchCount, false);
            }
            var consumer2 = new AsyncEventingBasicConsumer(channel2);
            consumer2.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());
                Console.WriteLine("[{0}] Consumer 2 - Started {1}", stopWatch.ElapsedMilliseconds, message);
                result.Consumer2Count++;
                if (!autoAck)
                {
                    await channel2.BasicAckAsync(ea.DeliveryTag, false);
                }
                Console.WriteLine("[{0}] Consumer 2 - Finished {1}", stopWatch.ElapsedMilliseconds, message);
            };
            await channel2.BasicConsumeAsync(
                queue: "hello",
                autoAck: autoAck,
                consumer: consumer2);

            int i = 0;
            
            stopWatch.Start();
            while (i < 10)
            {
                await channel.BasicPublishAsync(exchange: "", routingKey: "hello", Encoding.UTF8.GetBytes(i.ToString()));
                i++;
                Console.WriteLine("Published " + i.ToString());
            }

            while(result.Consumer1Count + result.Consumer2Count < 10)
            {
                await Task.Delay(10);
            }
            stopWatch.Stop();
            result.TotalTime = stopWatch.Elapsed.TotalMilliseconds;

            try
            {
                await channel.CloseAsync();
                if (!singleChannel)
                {
                    await channel2.CloseAsync();
                }
            }
            catch
            {
            }

            return result;
        }
    }
}
