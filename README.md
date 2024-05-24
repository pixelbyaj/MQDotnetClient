# IBM MQ Dotnet Client Library
Dotnet wrapper on top of official IBMMQDotnetClient library. Built with resilience and transient-fault-handling

[![NuGet Downloads](https://img.shields.io/nuget/dt/MQDotnetclient)]([www.google.com](https://nuget.org/packages/MQDotnetclient))
![GitHub License](https://img.shields.io/github/license/pixelbyaj/mqdotnetclient)


## Installation via NuGet

To install the client library via NuGet:

* Search for `MQDotnetClient` in the NuGet Library, or
* Type `Install-Package MQDotnetClient` into the Package Manager Console.

* Type `dotnet add package MQDotnetClient` into the dotnet CLI

## Dependencies
*   IBMMQDotnetClient (>= 9.3.5.1)
*   Microsoft.Extensions.Logging.Abstractions (>= 8.0.1)
*   Polly (>= 8.3.1)


## Configuration
## Docker MQ Configuration
Open **docker-componse.yml** file and copy and create the compose file.

Goto the directory and run below command:
```
docker compose up -d
```
### What you've done so far
You downloaded the pre-built Docker image and ran the container to get MQ running on RHEL. The IBM MQ objects and permissions that the client applications need to connect to a queue manager and to be able to put and get messages to and from the queue are created automatically. Docker and MQ are using your host computer resources and connectivity.

Inside the container, the MQ installation on RHEL has the following objects:

* Queue manager QM1 
* Queue DEV.QUEUE.1
* Channel: DEV.APP.SVRCONN
* Listener: **SYSTEM.LISTENER.TCP.1** on port 1414

The **queue** that you will be using, **DEV.QUEUE.1**, "lives" on the queue manager **QM1**. The queue manager also has a listener that listens for incoming connections, for example, on port 1414. Client applications can connect to the queue manager and can open, put, and get messages, and close the queue.

Applications use an **MQ channel** to connect to the queue manager. Access to these three objects is restricted in different ways. For example, user **"app"**, who is a member of the group "mqclient" is permitted to use the channel **DEV.APP.SVRCONN** and user **admin** is permitted to use the channel **DEV.ADMIN.SVRCONN** to connect to the queue manager QM1 and is authorized to put and get messages to and from the queue **DEV.QUEUE.1**.

## Get Started

### Configure QueueManager:
Configure IBM MQ QueueManager configuration in your appsettings.json
*   **SET_MQCONN_PROPERTIES**: If set to false all the MQ configuration will get ignore and only the Environment Variable configuration will get used
*   **MQSERVER**: Configure MQ connection details
    *   channel_name/protocol/servername(port)
*   **MQCNO_RECONNECT**: You can make your client reconnect automatically to a queue manager during an unexpected connection break.
*  **MESSAGE_TIMOUT_IN_MS**: Message read timeout if message not found
*  **USE_ENCODING_LEADING_BYTES**: remove message encoding leading bytes
*  **MESSAGE_ENCODING**: Message Encoding
*  **USE_MQCSP_AUTHENTICATION_PROPERTY**: Set to true if user has explicit username and password
*   **USER_ID_PROPERTY**: User Id for authentication. It only use if USE_MQCSP_AUTHENTICATION_PROPERTY is true
*   **PASSWORD_PROPERTY**: Password for authentication. It only use if USE_MQCSP_AUTHENTICATION_PROPERTY is true
*   **SET_MQCONN_SSL**: Set to true if MQ connect with SSL
*   **MQSSLKEYR**:  MQSSLKEYR specifies the location of the key repository that holds the digital certificate belonging to the user, in stem format.

```
*USER: IBM MQ.NET accesses the current user's certificate store to retrieve the client certificates.
*SYSTEM": IBM MQ.NET accesses the local computer account to retrieve the certificates.  
```
*   **MQSSLPEERNAME**: The SSLPEERNAME attribute is used to check the Distinguished Name (DN) of the certificate from the peer queue manager.
*   **MQCERTLABEL**: This attribute specifies the certificate label of the channel definition.
*   **MQSSLCIPHERSPEC**: The CipherSpec settings for an application are used during the handshake with the MQ Manager server.
```Note
 If the CipherSpec value supplied by the application is not a CipherSpec known to IBM MQ, then the IBM MQ managed client disregards it and negotiates the connection based on the Windows system's group policy.
```
*   **MQSSLRESET**:  It represents the number of unencrypted bytes sent and received on a TLS channel before the secret key is renegotiated. It is optional
*   **MQSSLCERTREVOCATIONCHECK**: The SSLStream class supports certificate revocation checking. It is optional

* **RetryPolicy**
  * **RetryCount**: Retry count
  * **WaitAndRetrySeconds**: Retry, waiting a specified duration between each retry. The wait is imposed on catching the failure, before making the next try.
  * **CircuitBreakerExceptionAllowedCount**: Exceptions Allowed Before Breaking Circuit
  * **CircutBreakerTimeoutSeconds**:  Waiting a specified duration between each circut breaker duration.

### appsettings.json example
```json
"QueueManager": 
        {
            "SET_MQCONN_PROPERTIES": "true",
            "MQSERVER": "DEV.ADMIN.SVRCONN/TCP/localhost(1414)",
            "MQCNO_RECONNECT": "false",
            "MESSAGE_TIMOUT_IN_MS": "1000", // read timeout if message not found
            "USE_ENCODING_LEADING_BYTES": "true",
            "MESSAGE_ENCODING": "UTF16",
            "USE_MQCSP_AUTHENTICATION_PROPERTY": "false",
            "USER_ID_PROPERTY": "admin", // it only use if USE_MQCSP_AUTHENTICATION_PROPERTY is true
            "PASSWORD_PROPERTY": "passw0rd",
            "SET_MQCONN_SSL": "false",
            "MQSSLKEYR": "*USER",
            //*USER: IBM� MQ.NET accesses the current user's certificate store to retrieve the client certificates.
            //*SYSTEM": IBM MQ.NET accesses the local computer account to retrieve the certificates.                
            "MQSSLPEERNAME": "PEERNAME",
            "MQCERTLABEL": "ibmwebspheremqlogonuserID",
            "MQSSLCIPHERSPEC": "TLS_RSA_WITH_AES_128_CBC_SHA",
            "MQSSLRESET": "500000",
            "MQSSLCERTREVOCATIONCHECK": "false",
            "RetryPolicy": {
              "RetryCount": 3,
              "WaitAndRetrySeconds": 60,
              "CircuitBreakerExceptionAllowedCount": 3,
              "CircutBreakerTimeoutSeconds": 5
            }
        }
```
### Code Example

```c#

static void Main(string[] args)
{

    private const string QUEUEMANAGER_SECTION = "QueueManager";
    private const string QUEUE_MANAGER_NAME = "QUEUE_MANAGER_NAME";
    private const string QUEUEMANAGER_RETRY_POLICY_SECTION = "RetryPolicy";
    private const string MESSAGE_ENCODING_SECTION = "MESSAGE_ENCODING";
    private const string MESSAGE_TIMOUT_IN_MS_SECTION = "MESSAGE_TIMOUT_IN_MS";
    private const string USE_ENCODING_LEADING_BYTES_SECTION = "USE_ENCODING_LEADING_BYTES";
    
    var config = ConfigHelper.SetConfiguration();
    
    var serviceProvider = new ServiceCollection()
    .AddLogging(builder =>
    {
        builder.AddConfiguration(config.GetSection("Logging"));
        builder.AddConsole();
    })
    .BuildServiceProvider();

    var logger = serviceProvider.GetService<ILogger<QueueManager>>();
    
    // Read QueueManagerModel
    QueueManagerModel? queueManagerModel = GetQueueManagerConfig(config);

    //Connect your Queue Manager
    QueueManager queueManager = new(queueManagerModel, logger);
    queueManager.Connect();

    //Put Message to Queue
    for (int i = 0; i < 100; i++)
    {
        PutMessage(queueManager, logger, $"This is my test message {i}");
    }

    // Read the message from the queue
    GetMessages(queueManager, logger);
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

```
