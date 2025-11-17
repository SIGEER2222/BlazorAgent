using System.Text.Json;
using Mom.RabbitMQ.Communication.RabbitMQ;
using Mom.RabbitMQ.Communication.RabbitMQ.Models;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace MudBlazorLab.Web.Services;

public class RabbitMQConsumerService : BackgroundService {
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly RabbitMQCommunicator _rabbitMQCommunicator;
    private readonly ISubject<QueenInfoMessage, QueenInfoMessage> _messageSubject;
    private const int MaxBufferedMessages = 1000;

    public RabbitMQConsumerService(
        ILogger<RabbitMQConsumerService> logger,
        ILoggerFactory loggerFactory) {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _rabbitMQCommunicator = new RabbitMQCommunicator();
        _messageSubject = new ReplaySubject<QueenInfoMessage>(bufferSize: MaxBufferedMessages);
    }

    public IObservable<QueenInfoMessage> Messages => _messageSubject.AsObservable();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try {
            _logger.LogInformation("Starting RabbitMQ Consumer Service...");

            var builder = new ConfigurationBuilder()
                       .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsetting.rabbitMQ.json", optional: true, reloadOnChange: true).Build();

            await _rabbitMQCommunicator.InitRabbitMQConfiguration(_loggerFactory, builder);

            // Subscribe to messages
            await _rabbitMQCommunicator.SubscribeAsync<JsonElement>(HandleMessage);

            _logger.LogInformation("RabbitMQ Consumer Service started successfully");

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error in RabbitMQ Consumer Service");
            // Do not rethrow to avoid crashing host when configuration is missing
        }
    }

    private async Task HandleMessage<T>(MessageEnvelope<T> msg) {
        try {
            var jsonBody = (JsonElement)(object)msg.MessageBody;

            if (msg.MessageHeader.RuleName.Equals("IQueenInfoMessage")) {
                var payload = jsonBody.Deserialize<QueenInfoMessage>();
                if (payload != null) {
                    _logger.LogInformation("Received IQueenInfoMessage: {ObjectType} - {ObjectName} - {InfoType}",
                        payload.ObjectType, payload.ObjectName, payload.InfoType);

                    // Push to Rx subject
                    _messageSubject.OnNext(payload);
                }
            }
            else if (msg.MessageHeader.RuleName.Equals("IQueenMessageBody")) {
                var payload = jsonBody.Deserialize<QueenMessageBody>();
                if (payload != null) {
                    _logger.LogInformation("Received IQueenMessageBody: {Content}", payload.Content);

                    // Convert to IQueenInfoMessage for consistency
                    var infoMessage = new QueenInfoMessage {
                        TimeStamp = DateTime.Now,
                        ObjectType = "Message",
                        ObjectName = "Body",
                        InfoType = "Content",
                        Content = payload.Content
                    };

                    _messageSubject.OnNext(infoMessage);
                }
            }
            else {
                _logger.LogInformation("Received unknown message type: {RuleName} - {Content}",
                    msg.MessageHeader.RuleName, msg.MessageBody?.ToString());
            }
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error handling RabbitMQ message");
        }
    }



    public override async Task StopAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Stopping RabbitMQ Consumer Service...");

        // Complete the subject
        _messageSubject.OnCompleted();

        // RabbitMQCommunicator 没有 Dispose，略过

        await base.StopAsync(cancellationToken);
        _logger.LogInformation("RabbitMQ Consumer Service stopped");
    }
}

// Message models (same as in the consumer project)
public class QueenMessageBody {
    public string Content { get; set; } = string.Empty;
}

public class QueenInfoMessage {
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    public string ObjectType { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string InfoType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
