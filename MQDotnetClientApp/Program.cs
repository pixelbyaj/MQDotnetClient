using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQService;
using MQService.Model;
using static MQService.QueueManager;

namespace MQDotnetClientApp
{
    public class Program
    {
        #region const members
        private const string QUEUEMANAGER_SECTION = "QueueManager";
        private const string QUEUE_MANAGER_NAME = "QUEUE_MANAGER_NAME";
        private const string QUEUEMANAGER_RETRY_POLICY_SECTION = "RetryPolicy";
        private const string MESSAGE_ENCODING_SECTION = "MESSAGE_ENCODING";
        private const string MESSAGE_TIMOUT_IN_MS_SECTION = "MESSAGE_TIMOUT_IN_MS";
        private const string USE_ENCODING_LEADING_BYTES_SECTION = "USE_ENCODING_LEADING_BYTES";
        #endregion

     
        static void Main(string[] args)
        {

            var config = ConfigHelper.SetConfiguration();
            QueueManagerModel? queueManagerModel = GetQueueManagerConfig(config);
            var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.AddConfiguration(config.GetSection("Logging"));
                builder.AddConsole();
            })
            .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<QueueManager>>();
            QueueManager queueManager = new(queueManagerModel, logger);
            queueManager.Connect();

            if (logger != null && queueManagerModel != null)
            {
                for (int i = 0; i < 100; i++)
                {
                    PutMessage(queueManager, logger, $"This is my test message {i}");
                }

                GetMessages(queueManager, logger);
            }
            queueManager.Dispose();
            Console.ReadLine();
        }

        private static void GetMessages(QueueManager queueManager, ILogger logger)
        {
            
            foreach(string  message in queueManager.GetMessages("DEV.QUEUE.1"))
            {
                Console.WriteLine($"My Message: {message}");
            }

        }

        private static void PutMessage(QueueManager queueManager, ILogger logger, string message)
        {
            byte[] correlationId = Guid.NewGuid().ToByteArray();
            queueManager.PutMessage("DEV.QUEUE.1", new QueueMessage
            {
                Content = message,
                CorrelationId = correlationId,
            });

        }
        private static QueueManagerModel? GetQueueManagerConfig(IConfiguration config)
        {

            var queueManager = config.GetSection(QUEUEMANAGER_SECTION);
            var retryPolicyDic = ConfigHelper.GetDictionary(queueManager.GetSection(QUEUEMANAGER_RETRY_POLICY_SECTION));
            List<QueueConfig> queueConfigs = new();
            QueueManagerModel? queueManagerModel = null;
            var queueManagerConfig = queueManager.GetChildren().ToDictionary(x => x.Key, x => x.Value ?? string.Empty);
                if (queueManagerConfig != null)
                {
                    MessageEncoding messageEncoding;
                    switch (queueManagerConfig[MESSAGE_ENCODING_SECTION].ToUpper())
                    {
                        case "UTF8":
                            messageEncoding = MessageEncoding.UTF8;
                            break;
                        case "UTF16":
                            messageEncoding = MessageEncoding.UTF16;
                            break;
                        case "UNICODE":
                            messageEncoding = MessageEncoding.UTF16;
                            break;
                        default:
                            messageEncoding = MessageEncoding.OTHER;
                            break;
                    }
                    queueManagerModel = new QueueManagerModel
                    {
                        ManagerName = queueManagerConfig[QUEUE_MANAGER_NAME],
                        MessageEncoding = messageEncoding,
                        Settings = queueManagerConfig,
                        MessageTimeOutInMs = Convert.ToInt32(queueManagerConfig[MESSAGE_TIMOUT_IN_MS_SECTION]),
                        UseEncodingLeadBytes = Convert.ToBoolean(queueManagerConfig[USE_ENCODING_LEADING_BYTES_SECTION]),
                        RetryPolicy = GetRetryPolicy(retryPolicyDic)
                    };
            }
            return queueManagerModel;
        } 
        private static QueueRetryPolicy GetRetryPolicy(IDictionary<string, string>? retryPolicyDic)
        {
            QueueRetryPolicy retryPolicy;
            if (retryPolicyDic != null && retryPolicyDic.Any())
            {
                retryPolicy = new()
                {
                    WaitAndRetrySeconds = Convert.ToInt32(retryPolicyDic["WaitAndRetrySeconds"]),
                    RetryCount = Convert.ToInt32(retryPolicyDic["RetryCount"]),
                    CircuitBreakerExceptionAllowedCount = Convert.ToInt32(retryPolicyDic["CircuitBreakerExceptionAllowedCount"]),
                    CircutBreakerTimeoutSeconds = Convert.ToInt32(retryPolicyDic["CircutBreakerTimeoutSeconds"]),
                };
            }
            else
            {
                retryPolicy = new();
            }

            return retryPolicy;

        }

    }
}
