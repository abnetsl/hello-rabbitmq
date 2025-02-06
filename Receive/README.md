# Teste de concorr�ncia entre consumers
No teste a ser realizado, ser�o utilizados 2 consumers para consumir mensagens de uma mesma fila. Ser�o publicadas 100 mensagens nesta fila e espera-se que as mensagens sejam consumidas no menor tempo poss�vel. O 1� consumer representa um processo que apresentou lentid�o, por exemplo por conta de um "lock". O 2� consumer representa um processo que est� funcionando normalmente e consumindo mensagens rapidamente. O comportamento ideal esperado � que a lentid�o do primeiro consumer n�o afete o segundo consumer e por consequ�ncia o tempo total de processamento das mensagens.

# Cen�rio 1
- Utilizando apenas 1 channel
- Basic Qos: 0
- Auto ack: false

Resultado: 15069ms
```bash
[12] Consumer 1 - Started 0
[3017] Consumer 1 - Finished 0
[3020] Consumer 2 - Started 1
[3020] Consumer 2 - Finished 1
[3020] Consumer 1 - Started 2
[6025] Consumer 1 - Finished 2
[6025] Consumer 2 - Started 3
[6025] Consumer 2 - Finished 3
[6025] Consumer 1 - Started 4
[9039] Consumer 1 - Finished 4
[9039] Consumer 2 - Started 5
[9039] Consumer 2 - Finished 5
[9039] Consumer 1 - Started 6
[12050] Consumer 1 - Finished 6
[12050] Consumer 2 - Started 7
[12050] Consumer 2 - Finished 7
[12050] Consumer 1 - Started 8
[15054] Consumer 1 - Finished 8
[15054] Consumer 2 - Started 9
[15054] Consumer 2 - Finished 9
{"SingleChannel":true,"PrefetchCount":0,"AutoAck":false,"TotalTime":15069.1867,"Consumer1Count":5,"Consumer2Count":5}
```

Neste caso observa-se que houve um round robin entre os consumers, sendo distribu�das 5 mensagens para cada um deles, no entanto elas foram todas processadas sequencialmente, sem concorr�ncia.

# Cen�rio 2
- Utilizando 2 channels
- Basic Qos: 0
- Auto ack: false
Resultado: 15030ms
```bash
[1] Consumer 1 - Started 0
[2] Consumer 2 - Started 1
[2] Consumer 2 - Finished 1
[2] Consumer 2 - Started 3
[2] Consumer 2 - Finished 3
[2] Consumer 2 - Started 5
[2] Consumer 2 - Finished 5
[2] Consumer 2 - Started 7
[2] Consumer 2 - Finished 7
[3] Consumer 2 - Started 9
[3] Consumer 2 - Finished 9
[3008] Consumer 1 - Finished 0
[3008] Consumer 1 - Started 2
[6012] Consumer 1 - Finished 2
[6012] Consumer 1 - Started 4
[9022] Consumer 1 - Finished 4
[9022] Consumer 1 - Started 6
[12022] Consumer 1 - Finished 6
[12022] Consumer 1 - Started 8
[15030] Consumer 1 - Finished 8
[15030] Consumer 2 - Started 8
[15030] Consumer 2 - Finished 8
{"SingleChannel":false,"PrefetchCount":0,"AutoAck":false,"TotalTime":15030.5915,"Consumer1Count":5,"Consumer2Count":6}
```
Neste caso observa-se que h� concorr�ncia no processamento das mensagens, de modo que o consumer2 processa todas suas mensagens antes do consumer 1 processar a sua 1�. Como n�o havia limite de prefetch(0) pode-se observar que as mensagens foram distribu�das equitativamente entre os 2 consumers. � curioso que a mensagem 8 foi processada por ambos consumers.

## An�lise dos resultados
- H� uma diferen�a no comportamento dos consumers quando est�o em um mesmo channel ou n�o. Quando est�o em um mesmo channel a ordem de processamento de mensagens � respeitada, de modo que a lentid�o em 1 consumer impacta no outro. Vamos analisar o que ocorre se mantivermos em apenas 1 channel mas alterar o prefetch count para 1.

# Cen�rio 3
- Utilizando apenas 1 channel
- Basic Qos: 1
- Auto ack: false
Resultado: 15065ms
```bash
[3008] Consumer 1 - Finished 0
[3008] Consumer 2 - Started 1
[3008] Consumer 2 - Finished 1
[3016] Consumer 1 - Started 2
[6017] Consumer 1 - Finished 2
[6018] Consumer 2 - Started 3
[6018] Consumer 2 - Finished 3
[6020] Consumer 1 - Started 4
[9033] Consumer 1 - Finished 4
[9033] Consumer 2 - Started 5
[9033] Consumer 2 - Finished 5
[9036] Consumer 1 - Started 6
[12038] Consumer 1 - Finished 6
[12039] Consumer 2 - Started 7
[12039] Consumer 2 - Finished 7
[12042] Consumer 1 - Started 8
[15049] Consumer 1 - Finished 8
[15049] Consumer 2 - Started 9
[15049] Consumer 2 - Finished 9
{"SingleChannel":true,"PrefetchCount":1,"AutoAck":false,"TotalTime":15065.5101,"Consumer1Count":5,"Consumer2Count":5}
```
Conclus�o: obtivemos o mesmo resultado do cen�rio 1, com as mensagens sendo consumidas sequencialmente, de modo que a lentid�o no consumer 1 impede o consumo de mensagens pelo consumer 2 independentemente do prefetch count. Vamos analisar o que ocorre se separarmos em 2 channels mas com prefetch count 1.

# Cen�rio 4
- Utilizando 2 channels
- Basic Qos: 1
- Auto ack: false

Resultado: 3024ms
```bash
[0] Consumer 1 - Started 0
[1] Consumer 2 - Started 1
[1] Consumer 2 - Finished 1
[12] Consumer 2 - Started 2
[12] Consumer 2 - Finished 2
[14] Consumer 2 - Started 3
[14] Consumer 2 - Finished 3
[15] Consumer 2 - Started 4
[15] Consumer 2 - Finished 4
[16] Consumer 2 - Started 5
[16] Consumer 2 - Finished 5
[16] Consumer 2 - Started 6
[16] Consumer 2 - Finished 6
[17] Consumer 2 - Started 7
[17] Consumer 2 - Finished 7
[18] Consumer 2 - Started 8
[18] Consumer 2 - Finished 8
[19] Consumer 2 - Started 9
[19] Consumer 2 - Finished 9
[3008] Consumer 1 - Finished 0
{"SingleChannel":false,"PrefetchCount":1,"AutoAck":false,"TotalTime":3024.411,"Consumer1Count":1,"Consumer2Count":9}
```
Conclus�o: Neste caso, as mensagens foram distribu�das de forma independente entre os 2 consumers, de modo que enquanto o 1� consumia a 1� mensagem, o outro consumiu as outras 9. Apresentando o comportamento esperado e obtendo o menor tempo poss�vel. Vamos validar agora a influ�ncia do AutoAck.

# Cen�rio 5
- Utilizando 2 channels
- Basic Qos: 1
- Auto ack: true

- Resultado: 
```bash
[1] Consumer 2 - Started 1
[1] Consumer 2 - Finished 1
[1] Consumer 2 - Started 3
[1] Consumer 2 - Finished 3
[1] Consumer 2 - Started 5
[1] Consumer 2 - Finished 5
[1] Consumer 2 - Started 7
[1] Consumer 2 - Finished 7
[2] Consumer 2 - Started 9
[2] Consumer 2 - Finished 9
[3002] Consumer 1 - Finished 0
[3002] Consumer 1 - Started 2
[6014] Consumer 1 - Finished 2
[6015] Consumer 1 - Started 4
[9015] Consumer 1 - Finished 4
[9016] Consumer 1 - Started 6
[12016] Consumer 1 - Finished 6
[12016] Consumer 1 - Started 8
[15017] Consumer 1 - Finished 8
{"SingleChannel":false,"PrefetchCount":1,"AutoAck":true,"TotalTime":15033.1036,"Consumer1Count":5,"Consumer2Count":5}
```
Conclus�o: com auto ack true, aparentemente o Rabbit confirma o processamento da mensagem assim que envia para o consumer de modo que ignora o prefetch count. O comportamento � similar ao cen�rio 3, com as mensagens sendo divididas entre os 2 consumers.


# Conclus�o
- O prefetch count � um par�metro importante para o controle de concorr�ncia entre consumers, de modo que ele permite que as mensagens sejam distribu�das de forma equitativa entre os consumers.
- O AutoAck influencia no comportamento dos consumers, de modo que com ele ativado o prefetch count � ignorado pois as mensagens s�o confirmadas sem terem sido processadas.
- O uso de m�ltiplos channels � importante para garantir a concorr�ncia entre os consumers, de modo que eles possam processar as mensagens de forma independente.

Cen�rio|SingleChannel|PrefetchCount|AutoAck|TotalTime|Consumer1Count|Consumer2Count
-|-|-|-|-|-|-
1|true|0|false|15069.1867|5|5
2|false|0|false|15030.5915|5|6
3|true|1|false|15065.5101|5|5
4|false|1|false|3024.411|1|9
5|false|1|true|15033.1036|5|5
```


