using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentMusic
{
        /// <summary>
        /// Handles splitting files into chunks, receiving chunks, reassembling files,
        /// and verifying integrity using SHA256.
        /// </summary>
        public class FileTransferService
        {
            // Size of each chunk in bytes (4 KB)
            private const int ChunkSize = 4096;

            // Temporary storage for received file chunks
            // Key = fileName, Value = Dictionary<chunkIndex, chunkData>
            private Dictionary<string, Dictionary<int, byte[]>> receivingFiles = new();

            /// <summary>
            /// Compute SHA256 hash of a file on disk.
            /// </summary>
            public string ComputeHash(string filePath)
            {
                return Helper.HashFile(filePath);
            }

        /// <summary>
        /// Split a file into chunks for sending.
        /// </summary>
        public List<byte[]> SplitFile(string filePath)
        {
            var chunks = new List<byte[]>();

            // Read the whole file into a byte array
            byte[] fileBytes = File.ReadAllBytes(filePath);
            int offset = 0; // beging of the file

            while (offset < fileBytes.Length)
            {
                // Calculate the size of this chunk
                int size = Math.Min(ChunkSize, fileBytes.Length - offset);

                // Copy chunk bytes from "fileBytes" to "chunk".
                byte[] chunk = new byte[size];
                Array.Copy(fileBytes, offset, chunk, 0, size);

                chunks.Add(chunk);
                offset += size; // shift offset by the number of copied bytes.
            }

            return chunks;
        }

            /// <summary>
            /// Initialize receiving storage for a new file.
            /// </summary>
            public void StartReceiving(string fileName, int totalChunks)
            {
                if (!receivingFiles.ContainsKey(fileName))
                {
                    receivingFiles[fileName] = new Dictionary<int, byte[]>(totalChunks);
                }
            }

            /// <summary>
            /// Add a received chunk to the file.
            /// </summary>
            public void AddChunk(string fileName, int index, byte[] chunk)
            {
                if (!receivingFiles.ContainsKey(fileName))
                    throw new Exception("File not initialized for receiving");

                receivingFiles[fileName][index] = chunk;
            }

            /// <summary>
            /// Assemble the received chunks into the final file, verify SHA256 hash, and save it.
            /// </summary>
            /// <param name="fileName">The file identifier</param>
            /// <param name="expectedHash">SHA256 hash expected for the file</param>
            /// <param name="savePath">Path to save the assembled file</param>
            /// <returns>True if the file is assembled and hash verified successfully</returns>
            public bool TryAssembleFile(string fileName, string expectedHash, string savePath)
            {
                if (!receivingFiles.ContainsKey(fileName))
                    return false;

                var chunksDict = receivingFiles[fileName];
                var totalChunks = chunksDict.Count;

                // Check if all chunks are received
                if (chunksDict.Keys.Max() + 1 != totalChunks) // 1 because index starts with 0, if we recived 3 chunks 0,1,2; Max() = 2, Max() + 1 = 3.
                return false;

                // Combine chunks into a single byte array
                var fileBytes = chunksDict.OrderBy(c => c.Key)
                                          .SelectMany(c => c.Value)
                                          .ToArray();

                // Verify SHA256 hash
                var hash = Helper.HashFileBytes(fileBytes);
                if (hash != expectedHash)
                    return false;

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

                // Save the assembled file
                File.WriteAllBytes(savePath, fileBytes);

                // Remove temporary storage
                receivingFiles.Remove(fileName);

                return true;
            }
        }
    }

/*
    *Sending a file in chunks

Load file

Split chunks

Convert chunk → Base64

Add StartByte/EndByte

Return Message

    *Receiving chunks and assembling

Save chunks to a temporary file

Track which chunks have been received

When all are assembled, verify the SHA-256

Deliver the fully assembled MP3*/

