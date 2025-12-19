using BitTorrentMusic.Models;
using BitTorrentMusic.Protocol;
using BitTorrentMusic.Services;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol; 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;


// Alias to clarify using Model 'Message', not System.Windows.Forms.Message
using AppMessage = BitTorrentMusic.Models.Message;

namespace BitTorrentMusic
{
    /// <summary>
    /// Handles all network communication via MQTT. 
    /// Implements the IProtocol interface to integrate with the main application logic.
    /// </summary>
    public class NetworkProtocol : IProtocol, IDisposable
    {
        // --- Services and State ---
        // Service responsible for splitting files into chunks and assembling them back
        private readonly FileTransferService fileTransferService = new();
        
        // The MQTT client instance from the library
        private IMqttClient client;

        private readonly MqttClientFactory factory = new();
        // Unique identifier for this user
        private readonly string myName;
        
        // Cache of songs available from other peers. Key = Peer Name, Value = List of Songs
        private readonly Dictionary<string, List<Song>> onlineCatalogs = new();
        
        // Set of currently online peers (HashSet ensures uniqueness)
        private readonly HashSet<string> onlinePeers = new();


        // --- Configuration Constants ---
        private const string TOPIC_MAIN = "BitRuisseau";
        private const string BROKER_HOST = "mqtt.blue.section-inf.ch";
        private const int BROKER_PORT = 1883;
        private const string USERNAME = "ict";
        private const string PASSWORD = "321";

        // --- Delegates & Events ---

        // DELEGATES: These are functions provided by the UI (Form1) to give  data
        public Func<List<Song>>? LocalCatalogProvider { get; set; } // Gives the list of local files
        public Func<string, string>? LocalPathProvider { get; set; } // Gives  the full path of a file based on its Hash

        // Event triggered when a file is fully downloaded and assembled
        public event Action<string, string>? FileReceived; // Args: (FileHash, SavePath)


        // JSON settings configuration (required to handle ISong interface deserialization)
        private readonly JsonSerializerOptions jsonOptions;

        /// <summary>
        /// Constructor: Sets up the user name and JSON configuration
        /// </summary>
        public NetworkProtocol(string name)
        {
            myName = name;

            // Configure JSON to use our custom converter
            // Without this, System.Text.Json cannot deserialize "ISong" because it is an interface
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new SongJsonConverter() }
            };

            // Start the MQTT connection in the background
            _ = InitializeMqttAsync();
        }

        /// <summary>
        /// Asynchronously connects to the MQTT broker.
        /// </summary>
        private async Task InitializeMqttAsync()
        {
            client = factory.CreateMqttClient();

            // Configure connection options (Server, Port, Credentials)
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(BROKER_HOST, BROKER_PORT)
                .WithCredentials(USERNAME, PASSWORD)
                .WithClientId(myName + "_" + Guid.NewGuid().ToString("N")[..6])  // Unique ClientID
                .WithCleanSession()
                .Build();

            // Callback when connected successfully
            client.ConnectedAsync += async e =>
            {
                Console.WriteLine($"[Connected] {myName}");

                // Create a topic filter
                var topicFilter = new MqttTopicFilterBuilder()
                    .WithTopic(TOPIC_MAIN)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                // Create subscription options via the factory
                var subscribeOptions = factory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(topicFilter)
                    .Build();

                // SEND A SUBSCRIPTION REQUEST TO THE BROKER, From this point on, the broker will begin sending us messages
                // subscribe (use CancellationToken.None)
                await client.SubscribeAsync(subscribeOptions, CancellationToken.None);

                // Broadcast presence to others
                SayOnline();
            };

            // Callback when a message is received
            client.ApplicationMessageReceivedAsync += OnMessage;

            // Perform the connection
            await client.ConnectAsync(options);
        }

        /// <summary>
        /// Main handler for incoming MQTT messages.
        /// </summary>
        private Task OnMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                // Convert the binary payload to a JSON string
                string json = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                // Deserialize JSON into Message object
                var msg = JsonSerializer.Deserialize<AppMessage>(json, jsonOptions);

                // Ignore invalid messages or messages sent by ourselves
                if (msg == null || msg.Sender == myName) return Task.CompletedTask;

                // Ignore messages not intended for us (unless recipient is "*" for broadcast)
                // Allow "*", "0.0.0.0" and "ALL" as broadcast addresses
                bool isBroadcast = msg.Recipient == "*" || msg.Recipient == "0.0.0.0" || msg.Recipient == "ALL";
                if (!isBroadcast && msg.Recipient != myName) return Task.CompletedTask;

                // Handle specific actions
                switch (msg.Action) //(msg.Action?.ToLower()) // ?. → if the object on the left is null, don't call anything, just return null.
                {
                    case "online":
                    case "askOnline":
                        // Another peer announced they are online. Add to list.
                        onlinePeers.Add(msg.Sender);
                        if (msg.Action == "askOnline") SayOnline();
                        break;

                    case "askCatalog":
                    case "requestCatalog":
                        // Someone asked for our list of songs. Send it.
                        SendCatalog(msg.Sender);
                        break;

                    case "sendCatalog":
                        // We received a catalog from someone. Store it.
                        if (msg.SongList != null)
                        {
                            var receivedSongs = msg.SongList.OfType<Song>().ToList();
                            onlineCatalogs[msg.Sender] = receivedSongs;
                        }
                        break;

                    case "askMedia":
                    case "askFile":
                        // If no data, they are asking for file
                        if (string.IsNullOrEmpty(msg.SongData))
                        {
                            if (!string.IsNullOrEmpty(msg.Hash))
                                SendChunk(msg.Sender, msg.Hash, 0, int.MaxValue);
                        }
                        // If data exists, they are sending a file
                        else
                        {
                            ProcessReceivedChunk(msg);
                        }
                        break;


                    case "sendMedia":
                    case "sendChunk":
                        ProcessReceivedChunk(msg);
                        break;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        // Helper to avoid duplicate code
        private void ProcessReceivedChunk(AppMessage msg)
        {
            // We received a piece of a file we are downloading.
            if (msg.SongData != null && msg.StartByte.HasValue && !string.IsNullOrEmpty(msg.Hash))
            {
                // Convert Base64 string back to byte array
                byte[] chunk = Convert.FromBase64String(msg.SongData);

                // 1. Initialize receiving storage (if not already done), Large buffer for receiving
                fileTransferService.StartReceiving(msg.Hash, 20000);

                // 2. Add the chunk to memory. 
                // calculate the index by dividing start byte by 4096 (ChunkSize)
                int index = msg.StartByte.Value / 4096;
                fileTransferService.AddChunk(msg.Hash, index, chunk);

                // 3. Determine where to save the file
                var knownSong = GetAllKnownSongs().FirstOrDefault(s => s.Hash == msg.Hash);
                string fileName = knownSong != null ? $"{knownSong.Artist} - {knownSong.Title}.mp3" : $"{msg.Hash}.mp3";

                string savePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                    "BitTorrentMusic_Downloads",
                    fileName
                );

                // 4. Try to assemble the file. 
                // If we have all the chunks, this returns true and saves the file to disk.
                if (fileTransferService.TryAssembleFile(msg.Hash, msg.Hash, savePath))
                {
                    // Notify UI that download is complete
                    FileReceived?.Invoke(msg.Hash, savePath);
                }
            }
        }

        /// <summary>
        /// Helper to publish a message object to the MQTT broker.
        /// </summary>
        private async void Publish(AppMessage msg)
        {
            if (client == null || !client.IsConnected) return;

            // Serialize message to JSON
            string json = JsonSerializer.Serialize(msg, jsonOptions);

            var mqttMsg = new MqttApplicationMessageBuilder()
                .WithTopic(TOPIC_MAIN)
                .WithPayload(json)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce) // QoS 1 ensures delivery
                .Build();

            await client.PublishAsync(mqttMsg);
        }

        // ------------ IProtocol Interface Implementation ------------

        /// <summary>
        /// Broadcasts an "online" message to all peers.
        /// </summary>
        public void SayOnline()
        {
            Publish(new AppMessage { Action = "online", Sender = myName, Recipient = "*" });
        }

        /// <summary>
        /// Sends a request for a catalog to a specific recipient (or "*" for all).
        /// </summary>
        public List<ISong> AskCatalog(string recipient)
        {
            Publish(new AppMessage { Action = "requestCatalog", Sender = myName, Recipient = recipient });
            //  return empty here because the response is asynchronous (handled in OnMessage)
            return new List<ISong>();
        }

        /// <summary>
        /// Sends our local list of songs to a recipient.
        /// </summary>
        public void SendCatalog(string recipient)
        {
            // Get local songs using the delegate provided by the UI
            var catalog = LocalCatalogProvider?.Invoke() ?? new List<Song>();
            var listInterface = new List<ISong>(catalog);

            Publish(new AppMessage
            {
                Action = "sendCatalog",
                Sender = myName,
                Recipient = recipient,
                SongList = listInterface
            });
        }

        /// <summary>
        /// Initiates a download request for a specific file hash.
        /// </summary>
        public void AskMedia(string peer, string hash)
        {
            // Find the song details to get the total size
            var song = GetAllKnownSongs().FirstOrDefault(s => s.Hash == hash);
            int size = song != null ? song.Size : 100000000; // Default to large size if unknown

            // Request the FULL file (from byte 0 to End)
            Publish(new AppMessage
            {
                Action = "askMedia", // WAS "requestChunk" -> CHANGE TO "askMedia"
                Sender = myName,
                Recipient = peer,
                Hash = hash,
                StartByte = 0,
                EndByte = size
            });
        }

        /// <summary>
        /// Reads a file from disk, splits it into chunks, and sends them via MQTT.
        /// </summary>
        public void SendChunk(string recipient, string hash, int startByte, int endByte)
        {
            // Get the physical path of the requested file
            string path = LocalPathProvider?.Invoke(hash);
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            // Split file into small blocks (4KB)
            var chunks = fileTransferService.SplitFile(path);
            int currentOffset = 0;

            foreach (var chunkBytes in chunks)
            {
                // Send each chunk as a separate message
                Publish(new AppMessage
                {
                    Action = "sendMedia", // WAS "sendChunk" -> CHANGE TO "sendMedia"
                    Sender = myName,
                    Recipient = recipient,
                    Hash = hash,
                    StartByte = currentOffset,
                    EndByte = currentOffset + chunkBytes.Length - 1,
                    SongData = Convert.ToBase64String(chunkBytes) // Binary data must be Base64 encoded for JSON
                });
                
                currentOffset += chunkBytes.Length;
                
                // Small sleep to prevent flooding the network buffer
                System.Threading.Thread.Sleep(10);
            }
        }

        // Unused interface methods (required by IProtocol contract but not used in this logic)
        public void AskMedia(string name, int startByte, int endByte) { }
        public void SendMedia(string name, int startByte, int endByte) { }

        public string[] GetOnlineMediatheque() { return onlinePeers.ToArray(); }

        /// <summary>
        /// Helper: Flattens the dictionary of catalogs into a single list of songs.
        /// </summary>
        public List<Song> GetAllKnownSongs()
        {
            var list = new List<Song>();
            foreach (var kv in onlineCatalogs) list.AddRange(kv.Value);
            return list;
        }

        /// <summary>
        /// Helper: Returns a list of songs paired with their source peer name.
        /// </summary>
        public List<(ISong song, string peer)> GetAggregatedCatalog()
        {
            var list = new List<(ISong, string)>();
            foreach (var kv in onlineCatalogs)
            {
                foreach (var s in kv.Value) list.Add((s, kv.Key));
            }
            return list;
        }

        public void Dispose()
        {
            client?.DisconnectAsync().Wait();
            client?.Dispose();
        }
    }

    /// <summary>
    /// Custom JSON Converter.
    /// It tells the JSON serializer how to handle the 'ISong' interface by treating it as a 'Song' class.
    /// </summary>
    public class SongJsonConverter : JsonConverter<ISong>
    {
        public override ISong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<Song>(ref reader, options);
        }
        public override void Write(Utf8JsonWriter writer, ISong value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (Song)value, options);
        }
    }
}