using System;
using System.Text;
using System.Text.Json;
using Receive;

var rabbitMQBenchmark = new RabbitMQBenchmark();
Console.WriteLine("=======Cenário 1=======");
var scenario1Result = await rabbitMQBenchmark.Run(singleChannel: true, prefetchCount: 0, autoAck: false);
Console.WriteLine(JsonSerializer.Serialize(scenario1Result));

Console.WriteLine("=======Cenário 2=======");
var scenario2Result = await rabbitMQBenchmark.Run(singleChannel: false, prefetchCount: 0, autoAck: false);
Console.WriteLine(JsonSerializer.Serialize(scenario2Result));

Console.WriteLine("=======Cenário 3=======");
var scenario3Result = await rabbitMQBenchmark.Run(singleChannel: true, prefetchCount: 1, autoAck: false);
Console.WriteLine(JsonSerializer.Serialize(scenario3Result));

Console.WriteLine("=======Cenário 4=======");
var scenario4Result = await rabbitMQBenchmark.Run(singleChannel: false, prefetchCount: 1, autoAck: false);
Console.WriteLine(JsonSerializer.Serialize(scenario4Result));

Console.WriteLine("=======Cenário 5=======");
var scenario5Result = await rabbitMQBenchmark.Run(singleChannel: false, prefetchCount: 1, autoAck: true);
Console.WriteLine(JsonSerializer.Serialize(scenario5Result));

Console.ReadLine();