# LMS Credit Engine Worker

Worker responsável por processar filas do RabbitMQ e fazer requisições para a API de crédito.

## Funcionalidades

- ✅ Consumer de filas RabbitMQ
- ✅ Processamento de solicitações de crédito
- ✅ Integração com API de crédito (com mocks para desenvolvimento)
- ✅ Logging estruturado com Serilog
- ✅ Tratamento de erros e retry automático
- ✅ Configuração flexível via appsettings.json
- ✅ Suporte a execução como serviço do Windows

## Estrutura do Projeto

```
lms-credit-engine-worker/
├── CreditEngineWorker/
│   ├── Configuration/          # Classes de configuração
│   ├── Models/                # Modelos de dados
│   ├── Services/              # Serviços de negócio
│   ├── appsettings.json       # Configurações de produção
│   ├── appsettings.Development.json
│   ├── CreditEngineWorker.csproj
│   ├── Program.cs
│   └── Worker.cs
├── lms-credit-engine-worker.sln
└── README.md
```

## Configuração

### RabbitMQ

Configure as seguintes propriedades no `appsettings.json`:

```json
{
  "MessageSettings": {
    "Hostname": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "Url": "amqp://guest:guest@localhost:5672/",
    "Queue": "credit-engine-queue",
    "Exchange": "credit-engine-exchange",
    "RoutingKey": "credit.request"
  }
}
```

### API de Crédito

```json
{
  "CreditEngineApi": {
    "BaseUrl": "https://api.credit-engine.com/api/",
    "ApiKey": "YOUR_API_KEY_HERE",
    "TimeoutSeconds": 30
  }
}
```

## Modelos de Dados

### CreditRequest
```json
{
  "id": "string",
  "customerId": "string",
  "amount": 1000.00,
  "currency": "BRL",
  "requestType": "string",
  "metadata": {},
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### CreditResponse
```json
{
  "success": true,
  "requestId": "string",
  "creditScore": 750,
  "approved": true,
  "approvedAmount": 800.00,
  "interestRate": 0.12,
  "message": "Crédito aprovado",
  "errors": [],
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### QueueMessage
```json
{
  "messageId": "string",
  "correlationId": "string",
  "creditRequest": {},
  "retryCount": 0,
  "maxRetries": 3,
  "timestamp": "2024-01-01T00:00:00Z"
}
```

## Como Executar

### Desenvolvimento

```bash
# Navegar para o diretório do projeto
cd lms-credit-engine-worker/CreditEngineWorker

# Restaurar dependências
dotnet restore

# Executar em modo console
dotnet run --console
```

### Produção

```bash
# Publicar aplicação
dotnet publish -c Release -o ./publish

# Executar
dotnet ./publish/CreditEngineWorker.dll
```

### Como Serviço do Windows

```bash
# Instalar como serviço
sc create "LMS-CreditEngine-Worker" binPath="C:\path\to\CreditEngineWorker.exe"

# Iniciar serviço
sc start "LMS-CreditEngine-Worker"

# Parar serviço
sc stop "LMS-CreditEngine-Worker"
```

## Logs

Os logs são salvos em:
- Console (desenvolvimento)
- Arquivo: `logs/credit-engine-worker.log`
- Rotação diária automática

### Níveis de Log

- **Information**: Operações normais
- **Warning**: Situações que requerem atenção
- **Error**: Erros que não impedem o funcionamento
- **Fatal**: Erros críticos que param a aplicação

## Implementação da API Real

### Substituir Mocks

1. **CreditEngineService.cs**:
   - Remover métodos `MockCreditProcessingAsync` e `MockValidationAsync`
   - Implementar chamadas HTTP reais para sua API
   - Configurar autenticação e headers necessários

2. **Exemplo de implementação real**:

```csharp
public async Task<CreditResponse> ProcessCreditRequestAsync(CreditRequest request)
{
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    var response = await _httpClient.PostAsync("/credit/process", content);
    response.EnsureSuccessStatusCode();
    
    var responseJson = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<CreditResponse>(responseJson);
}
```

## Monitoramento

### Health Checks

O worker verifica automaticamente:
- Conexão com RabbitMQ
- Processamento de mensagens
- Reconexão automática em caso de falha

### Métricas Importantes

- Mensagens processadas por minuto
- Taxa de sucesso/erro
- Tempo de processamento médio
- Conexões ativas com RabbitMQ

## Troubleshooting

### Problemas Comuns

1. **Erro de conexão RabbitMQ**:
   - Verificar se o RabbitMQ está rodando
   - Validar credenciais e configurações
   - Verificar firewall e rede

2. **Mensagens não processadas**:
   - Verificar se a fila existe
   - Validar formato das mensagens
   - Verificar logs de erro

3. **API de crédito não responde**:
   - Verificar URL e credenciais
   - Testar conectividade de rede
   - Validar timeout configurado

### Logs de Debug

Para mais detalhes, altere o nível de log no `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

## Desenvolvimento

### Adicionando Novos Tipos de Solicitação

1. Estender `CreditRequest` com novos campos
2. Atualizar `CreditEngineService` para processar novos tipos
3. Implementar validações específicas
4. Atualizar documentação

### Testes

```bash
# Executar testes unitários
dotnet test

# Executar com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request

## Licença

Este projeto está sob a licença MIT. Veja o arquivo LICENSE para mais detalhes.
