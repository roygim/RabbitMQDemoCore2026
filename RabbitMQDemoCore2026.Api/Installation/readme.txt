For RabbitMQ:
1. install docker
2. open powershell
3. go to - cd ../RabbitMQDemoCore2026/Docker
4. run - docker compose -p rabbitmqdemo up -d

add to appsettings.json file:
"RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "admin",
    "Password": "admin123",
    "VirtualHost": "/",
    "ProductsQueue": "products_queue"
  }