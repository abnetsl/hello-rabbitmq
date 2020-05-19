# Hello, RabbitMQ

This repository follows the [RabbitMQ tutorial for .NET.](https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html).

## Instructions

Make sure you have [Docker](https://www.docker.com/) and [.NET Core](https://dotnet.microsoft.com/download) installed before proceeding.

### Run a RabbitMQ container

At the project's root, use docker compose to start a RabbitMQ container.

```bash
docker-compose up
```

### Start receiver

The following command will run the Receive program, a console application that listens for messages in the `hello` queue.

```bash
dotnet run --project Receive
```

### Run sender

The following command will run the Send program, a console application that sends a message to the `hello` queue.

```bash
dotnet run --project Send
```
