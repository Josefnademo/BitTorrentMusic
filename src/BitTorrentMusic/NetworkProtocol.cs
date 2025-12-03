using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BitTorrentMusic
{
    public class NetworkProtocol : IProtocol
    {
        private readonly FileTransferService fileTransferService;
        private readonly Dictionary<string, List<ISong>> onlineCatalogs;
        private readonly List<string> onlineMediatheques;
        private IMqttClient client;
        private readonly string myName;

        private const string ONLINE_TOPIC = "music/online";
        private const string CATALOG_TOPIC = "music/catalog";
        private const string FILE_REQUEST_TOPIC = "music/file/request";
        private const string FILE_CHUNK_TOPIC = "music/file/chunk";

        public NetworkProtocol(string name)
        {
            myName = name;
            fileTransferService = new FileTransferService();
            onlineCatalogs = new Dictionary<string, List<ISong>>();
            onlineMediatheques = new List<string>();
            _ = InitializeMqtt();
        }

        private async Task InitializeMqtt()
        {
            var factory = new MqttFactory();
            client = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId(myName)
                .WithTcpServer("localhost", 1883)
                .Build();

            client.UseConnectedHandler(async _ =>
            {
                await client.SubscribeAsync(ONLINE_TOPIC);
                await client.SubscribeAsync(CATALOG_TOPIC);
                await client.SubscribeAsync(FILE_REQUEST_TOPIC);
                await client.SubscribeAsync(FILE_CHUNK_TOPIC);
            });

            client.UseApplicationMessageReceivedHandler(OnMessage);
            await client.ConnectAsync(options);
        }

        private void OnMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            string json = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            var msg = JsonSerializer.Deserialize<Message>(json);
            if (msg == null || msg.Sender == myName) return;

            switch (msg.Action)
            {
                case "ONLINE":
                    if (!onlineMediatheques.Contains(msg.Sender))
                        onlineMediatheques.Add(msg.Sender);
                    break;

                case "CATALOG_REQUEST":
                    SendCatalog(msg.Sender);
                    break;

                case "CATALOG_RESPONSE":
                    if (msg.SongList != null)
                        onlineCatalogs[msg.Sender] = msg.SongList;
                    break;

                case "FILE_REQUEST":
                    if (msg.StartByte.HasValue && msg.EndByte.HasValue)
                        SendMedia(msg.Sender, msg.StartByte.Value, msg.EndByte.Value, msg.Hash);
                    break;

                case "FILE_CHUNK":
                    if (msg.SongData != null && msg.StartByte.HasValue)
                    {
                        byte[] chunk = Convert.FromBase64String(msg.SongData);
                        fileTransferService.AddChunk(msg.Hash, msg.StartByte.Value, chunk);
                    }
                    break;
            }
        }

        private async void Publish(string topic, Message msg)
        {
            if (client == null || !client.IsConnected) return;

            string json = JsonSerializer.Serialize(msg);
            var mqttMsg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(json)
                .WithAtLeastOnceQoS()
                .Build();

            await client.PublishAsync(mqttMsg);
        }

        public string[] GetOnlineMediatheque() => onlineMediatheques.ToArray();

        public void SayOnline()
        {
            var msg = new Message { Action = "ONLINE", Sender = myName };
            Publish(ONLINE_TOPIC, msg);
            Console.WriteLine("Announcing online status to network...");
        }

        public List<ISong> AskCatalog(string name)
        {
            Publish(CATALOG_TOPIC, new Message
            {
                Action = "CATALOG_REQUEST",
                Sender = myName,
                Recipient = name
            });

            return onlineCatalogs.ContainsKey(name) ? onlineCatalogs[name] : new List<ISong>();
        }

        public void SendCatalog(string name)
        {
            // Берём локальный каталог из Form (через callback или DataGridView)
            List<ISong> localCatalog = LocalCatalogProvider?.Invoke() ?? new List<ISong>();

            Publish(CATALOG_TOPIC, new Message
            {
                Action = "CATALOG_RESPONSE",
                Sender = myName,
                Recipient = name,
                SongList = localCatalog
            });

            Console.WriteLine($"Sent catalog to {name}");
        }

        public void AskMedia(string name, int startByte, int endByte, string hash)
        {
            Publish(FILE_REQUEST_TOPIC, new Message
            {
                Action = "FILE_REQUEST",
                Sender = myName,
                Recipient = name,
                StartByte = startByte,
                EndByte = endByte,
                Hash = hash
            });

            Console.WriteLine($"Requested media from {name}: {startByte}-{endByte}");
        }

        public void SendMedia(string name, int startByte, int endByte, string hash)
        {
            string path = LocalPathProvider?.Invoke(hash);
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            byte[] bytes = File.ReadAllBytes(path);
            byte[] range = bytes.Skip(startByte).Take(endByte - startByte + 1).ToArray();

            Publish(FILE_CHUNK_TOPIC, new Message
            {
                Action = "FILE_CHUNK",
                Sender = myName,
                Recipient = name,
                StartByte = startByte,
                EndByte = endByte,
                SongData = Convert.ToBase64String(range),
                Hash = hash
            });

            Console.WriteLine($"Sent media chunk {startByte}-{endByte} to {name}");
        }

        public void ReceiveChunk(string hash, int index, byte[] chunk)
        {
            fileTransferService.AddChunk(hash, index, chunk);
        }

        public bool FinalizeReceivedFile(string hash, string expectedHash, string savePath)
        {
            return fileTransferService.TryAssembleFile(hash, expectedHash, savePath);
        }

        // ==================== CALLBACKS ====================
        // В твоей форме нужно будет присвоить эти делегаты, чтобы NetworkProtocol брал данные из DataGridView
        public Func<List<ISong>>? LocalCatalogProvider { get; set; }
        public Func<string, string>? LocalPathProvider { get; set; }
    }
}
