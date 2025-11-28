using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text;
using System.Text.Json;
using MQTTnet;
//using MQTTnet.Client;
//using MQTTnet.Client.Options;
using System.Threading.Tasks;

namespace BitTorrentMusic
{
    /// <summary>
    /// Implements the IProtocol interface for sending/receiving song catalogs and media files.
    /// Integrates FileTransferService for safe file transfer with SHA256 verification.
    /// </summary>
    public class NetworkProtocol : IProtocol
    {
        private readonly FileTransferService fileTransferService;

        // Store catalogs of other online mediatheques
        private readonly Dictionary<string, List<ISong>> onlineCatalogs;

        // Example list of known mediatheques (can be IPs or names)
        private readonly List<string> onlineMediatheques;

        public NetworkProtocol()
        {
            fileTransferService = new FileTransferService();
            onlineCatalogs = new Dictionary<string, List<ISong>>();
            onlineMediatheques = new List<string>();
        }

        /// <summary>
        /// Get the list of all online mediatheques
        /// </summary>
        public string[] GetOnlineMediatheque()
        {
            // Return a copy to prevent modification from outside
            return onlineMediatheques.ToArray();
        }

        /// <summary>
        /// Send an "I'm online" message to notify other mediatheques
        /// </summary>
        public void SayOnline()
        {
            // TODO: send a network broadcast via MQTT/TCP to announce presence
            // Example: publish message {Action = "Online", Sender = thisMediatheque}
            Console.WriteLine("Announcing online status to network...");
        }

        /// <summary>
        /// Request the catalog of a specific mediatheque
        /// </summary>
        public List<ISong> AskCatalog(string name)
        {
            // TODO: send a network request asking for the catalog
            // For now, return stored catalog if available
            if (onlineCatalogs.ContainsKey(name))
                return onlineCatalogs[name];
            else
                return new List<ISong>();
        }

        /// <summary>
        /// Send our catalog to a specific mediatheque
        /// </summary>
        public void SendCatalog(string name)
        {
            // TODO: serialize local catalog and send it over network
            Console.WriteLine($"Sending catalog to {name}...");
        }

        /// <summary>
        /// Download a media file from another mediatheque by byte range
        /// </summary>
        public void AskMedia(string name, int startByte, int endByte)
        {
            // TODO: send request for file chunk(s) from "name"
            // The network response should call AddChunk() in FileTransferService
            Console.WriteLine($"Requesting media from {name}: bytes {startByte}-{endByte}");
        }

        /// <summary>
        /// Send a media file chunk to a requesting mediatheque
        /// </summary>
        public void SendMedia(string name, int startByte, int endByte)
        {
            // TODO: read file bytes from disk using FileTransferService
            // and send them to the requester via network
            Console.WriteLine($"Sending media to {name}: bytes {startByte}-{endByte}");
        }

        /// <summary>
        /// Helper method to send a full file by splitting it into chunks
        /// </summary>
        public void SendFullFile(string filePath, string recipient)
        {
            var chunks = fileTransferService.SplitFile(filePath);
            string fileHash = fileTransferService.ComputeHash(filePath);

            // Example: send chunks one by one
            for (int i = 0; i < chunks.Count; i++)
            {
                // TODO: send each chunk over network (MQTT/TCP)
                Console.WriteLine($"Sending chunk {i + 1}/{chunks.Count} to {recipient}");
            }

            Console.WriteLine($"File {filePath} sent to {recipient} with hash {fileHash}");
        }

        /// <summary>
        /// Helper method to receive a chunk for a file
        /// </summary>
        public void ReceiveChunk(string fileName, int index, byte[] chunk)
        {
            // Add the chunk to the FileTransferService temporary storage
            fileTransferService.AddChunk(fileName, index, chunk);
        }

        /// <summary>
        /// Finalize a received file and verify its hash
        /// </summary>
        public bool FinalizeReceivedFile(string fileName, string expectedHash, string savePath)
        {
            return fileTransferService.TryAssembleFile(fileName, expectedHash, savePath);
        }
    }
}
