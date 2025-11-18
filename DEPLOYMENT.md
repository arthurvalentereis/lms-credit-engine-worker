# Guia de Deploy - LMS Credit Engine Worker

## Pré-requisitos

- .NET 9.0 Runtime
- RabbitMQ Server
- API de Crédito (configurada)

## Configuração do Ambiente

### 1. RabbitMQ

```bash
# Instalar RabbitMQ (Ubuntu/Debian)
sudo apt-get install rabbitmq-server

# Iniciar serviço
sudo systemctl start rabbitmq-server
sudo systemctl enable rabbitmq-server

# Criar usuário e vhost
sudo rabbitmqctl add_user creditworker password123
sudo rabbitmqctl add_vhost credit_engine
sudo rabbitmqctl set_permissions -p credit_engine creditworker ".*" ".*" ".*"
```

### 2. Configuração da Aplicação

Copie e ajuste o arquivo `appsettings.json`:

```json
{
  "MessageSettings": {
    "Hostname": "localhost",
    "Port": 5672,
    "Username": "creditworker",
    "Password": "password123",
    "Url": "amqp://creditworker:password123@localhost:5672/credit_engine",
    "Queue": "credit-engine-queue",
    "Exchange": "credit-engine-exchange",
    "RoutingKey": "credit.request"
  },
  "CreditEngineApi": {
    "BaseUrl": "https://your-api.com/api/",
    "ApiKey": "your-api-key",
    "TimeoutSeconds": 30
  }
}
```

## Deploy

### Opção 1: Executável Standalone

```bash
# Publicar aplicação
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish

# Executar
./publish/CreditEngineWorker.exe
```

### Opção 2: Como Serviço do Windows

```bash
# Publicar
dotnet publish -c Release -o ./publish

# Instalar como serviço
sc create "LMS-CreditEngine-Worker" binPath="C:\path\to\publish\CreditEngineWorker.exe"

# Configurar para iniciar automaticamente
sc config "LMS-CreditEngine-Worker" start=auto

# Iniciar
sc start "LMS-CreditEngine-Worker"
```

### Opção 3: Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "CreditEngineWorker.dll"]
```

```bash
# Build da imagem
docker build -t lms-credit-engine-worker .

# Executar container
docker run -d --name credit-worker \
  -e MessageSettings__Hostname=rabbitmq \
  -e MessageSettings__Username=creditworker \
  -e MessageSettings__Password=password123 \
  lms-credit-engine-worker
```

## Monitoramento

### Logs

```bash
# Ver logs em tempo real
tail -f logs/credit-engine-worker.log

# Filtrar por nível
grep "ERROR" logs/credit-engine-worker.log
```

### Health Check

```bash
# Verificar se o worker está rodando
ps aux | grep CreditEngineWorker

# Verificar conexão RabbitMQ
rabbitmqctl list_connections
rabbitmqctl list_queues
```

## Troubleshooting

### Problemas Comuns

1. **Worker não inicia**:
   - Verificar se .NET 9.0 está instalado
   - Verificar permissões de arquivo
   - Verificar configurações do appsettings.json

2. **Não conecta no RabbitMQ**:
   - Verificar se RabbitMQ está rodando
   - Testar conectividade: `telnet localhost 5672`
   - Verificar credenciais e vhost

3. **Mensagens não processadas**:
   - Verificar se a fila existe
   - Verificar logs de erro
   - Testar com mensagem de exemplo

### Comandos Úteis

```bash
# Verificar status do serviço
sc query "LMS-CreditEngine-Worker"

# Parar serviço
sc stop "LMS-CreditEngine-Worker"

# Reiniciar serviço
sc stop "LMS-CreditEngine-Worker" && sc start "LMS-CreditEngine-Worker"

# Ver logs do Windows
Get-EventLog -LogName Application -Source "LMS-CreditEngine-Worker"
```

## Backup e Recuperação

### Backup de Configuração

```bash
# Backup das configurações
cp appsettings.json appsettings.json.backup
cp appsettings.Production.json appsettings.Production.json.backup
```

### Recuperação

```bash
# Restaurar configurações
cp appsettings.json.backup appsettings.json

# Reiniciar serviço
sc restart "LMS-CreditEngine-Worker"
```

## Atualizações

### Processo de Atualização

1. Parar o serviço atual
2. Fazer backup das configurações
3. Deploy da nova versão
4. Restaurar configurações
5. Iniciar o serviço
6. Verificar logs

```bash
# Script de atualização
sc stop "LMS-CreditEngine-Worker"
cp appsettings.json appsettings.json.backup
# ... deploy nova versão ...
cp appsettings.json.backup appsettings.json
sc start "LMS-CreditEngine-Worker"
```
