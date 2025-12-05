using BitTorrentMusic.Models;
using BitTorrentMusic.Protocol;
using BitTorrentMusic.Services;
using MQTTnet;
using System.Text;
using System.Text.Json;


public class NetworkProtocol : IProtocol, IDisposable
{
    private readonly FileTransferService fileTransferService = new();
    private IMqttClient client;
    private readonly string myName;
    private readonly Dictionary<string, List<Song>> onlineCatalogs = new();
    private readonly HashSet<string> onlinePeers = new();

    public Func<List<Song>>? LocalCatalogProvider { get; set; }
    public Func<string, string>? LocalPathProvider { get; set; }
    public event Action<string, string>? FileReceived; // hash, path

    private const string TOPIC_MAIN = "BitRuisseau";

    public NetworkProtocol(string name)
    {
        myName = name;
        _ = InitializeMqttAsync();
    }

    private async Task InitializeMqttAsync()
    {
        var factory = new MqttFactory();
        client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("mqtt.blue.section-inf.ch", 1883)
            .WithCredentials("ict", "321")
            .WithClientId(myName + "_" + Guid.NewGuid().ToString("N")[..6])
            .WithCleanSession()
            .Build();

        client.UseConnectedHandler(async e =>
        {
            Console.WriteLine($"[Connected] {myName}");
            await client.SubscribeAsync(TOPIC_MAIN);
            SayOnline();
        });

        client.UseApplicationMessageReceivedHandler(OnMessage);

        await client.ConnectAsync(options);
    }

    private void OnMessage(MqttApplicationMessageReceivedEventArgs e)
    {
        string json = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        var msg = JsonSerializer.Deserialize<Message>(json);
        if (msg == null || msg.Sender == myName) return;

        // Игнорируем сообщения не для нас и не для всех
        if (msg.Recipient != "*" && msg.Recipient != myName) return;

        switch (msg.Action?.ToLower())
        {
            case "online":
                onlinePeers.Add(msg.Sender);
                break;

            case "requestcatalog":
                SendCatalog(msg.Sender);
                break;

            case "sendcatalog":
                if (msg.SongList != null)
                    onlineCatalogs[msg.Sender] = msg.SongList;
                break;

            case "requestchunk":
                if (msg.StartByte.HasValue && msg.EndByte.HasValue)
                    SendChunk(msg.Sender, msg.Hash, msg.StartByte.Value, msg.EndByte.Value);
                break;

            case "sendchunk":
                if (msg.SongData != null && msg.StartByte.HasValue)
                {
                    byte[] chunk = Convert.FromBase64String(msg.SongData);
                    fileTransferService.AddChunk(msg.Hash, msg.StartByte.Value, chunk);

                    // Проверка полной сборки
                    string savePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                        "BitTorrentMusic_Downloads",
                        Path.GetFileName(LocalPathProvider?.Invoke(msg.Hash) ?? msg.Hash + ".mp3")
                    );

                    if (fileTransferService.TryAssembleFile(msg.Hash, msg.Hash, savePath))
                        FileReceived?.Invoke(msg.Hash, savePath);
                }
                break;
        }
    }


    private async void Publish(Message msg)
    {
        if (client == null || !client.IsConnected) return;

        string json = JsonSerializer.Serialize(msg);
        var mqttMsg = new MqttApplicationMessageBuilder()
            .WithTopic(TOPIC_MAIN)
            .WithPayload(json)
            .WithAtLeastOnceQoS()
            .Build();

        await client.PublishAsync(mqttMsg);
    }

    public void SayOnline()
    {
        Publish(new Message
        {
            Action = "online",
            Sender = myName,
            Recipient = "*"
        });
    }

    public void AskCatalog(string recipient = "*")
    {
        Publish(new Message
        {
            Action = "requestCatalog",
            Sender = myName,
            Recipient = recipient
        });
    }

    public void SendCatalog(string recipient)
    {
        var catalog = LocalCatalogProvider?.Invoke() ?? new List<Song>();
        Publish(new Message
        {
            Action = "sendCatalog",
            Sender = myName,
            Recipient = recipient,
            SongList = catalog
        });
    }


    public void AskChunk(string peer, string hash, int start, int end) =>
        Publish(new Message
        {
            Action = "requestChunk",
            Sender = myName,
            Recipient = peer,
            Hash = hash,
            StartByte = start,
            EndByte = end
        });



    public void AskMedia(string peer, string hash)
    {
        string path = LocalPathProvider?.Invoke(hash);
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

        var chunks = fileTransferService.SplitFile(path);
        int offset = 0;

        foreach (var chunk in chunks)
        {
            Publish(new Message
            {
                Action = "sendChunk",
                Sender = myName,
                Recipient = peer,
                Hash = hash,
                StartByte = offset,
                EndByte = offset + chunk.Length - 1,
                SongData = Convert.ToBase64String(chunk)
            });
            offset += chunk.Length;
        }
    }

    public void SendChunk(string recipient, string hash, int startByte, int endByte)
    {
        string path = LocalPathProvider?.Invoke(hash);
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

        byte[] bytes = File.ReadAllBytes(path);
        byte[] chunk = bytes.Skip(startByte).Take(endByte - startByte + 1).ToArray();

        Publish(new Message
        {
            Action = "sendChunk",
            Sender = myName,
            Recipient = recipient,
            Hash = hash,
            StartByte = startByte,
            EndByte = endByte,
            SongData = Convert.ToBase64String(chunk)
        });
    }


    public List<Song> GetAllKnownSongs()
    {
        var list = new List<Song>();
        foreach (var kv in onlineCatalogs)
            list.AddRange(kv.Value);
        return list;
    }

    // Utility to get an aggregated catalog (all peers) for UI convenience
    public List<(ISong song, string peer)> GetAggregatedCatalog()
    {
        var list = new List<(ISong, string)>();
        foreach (var kv in onlineCatalogs)
        {
            foreach (var s in kv.Value)
                list.Add((s, kv.Key));
        }
        return list;
    }

    public void Dispose()
    {
        client?.DisconnectAsync().Wait();
        client?.Dispose();
    }
}
