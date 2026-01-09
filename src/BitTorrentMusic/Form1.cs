using BitTorrentMusic.Models;
using BitTorrentMusic.Protocol;
using BitTorrentMusic.Services;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;  // To work with Directory, FileInfo, Path
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TagLib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BitTorrentMusic
{
    public partial class MainMenuUI : Form
    {
        private IProtocol protocol;
        private readonly Dictionary<string, string> hashToPath = new(StringComparer.OrdinalIgnoreCase);


        public MainMenuUI()
        {
            InitializeComponent();
            //protocol = new NetworkProtocol("YosefLocal"); //MQTT realisation
            protocol = new NetworkProtocol("User_" + new Random().Next(100, 999));

            // wire delegates (protocol will call these when it needs catalog or path)
            ((NetworkProtocol)protocol).LocalCatalogProvider = GetLocalSongs;
            ((NetworkProtocol)protocol).LocalPathProvider = GetLocalSongPath;

            // subscribe to file received to add downloaded file to local UI
            ((NetworkProtocol)protocol).FileReceived += OnFileReceived;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupLocalGrid();
            SetupGlobalGrid();

            StyleDataGrid(dataGridViewLocal);
            StyleDataGrid(dataGridViewGlobal);
        }

        private void LoadLocalSongs(string folderPath)
        {
            dataGridViewLocal.Rows.Clear();

            string[] extensions = { "*.mp3", "*.wav", "*.ogg" };
            List<string> files = new List<string>();

            // Load files for each extension
            foreach (string ext in extensions)
            {
                try
                {
                    files.AddRange(Directory.GetFiles(folderPath, ext, SearchOption.TopDirectoryOnly)); //for only current directory
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error scanning folder with {ext}: {ex.Message}");
                }
            }

            foreach (string file in files)
            {
                try
                {
                    var tag = TagLib.File.Create(file);

                    string title = tag.Tag.Title ?? Path.GetFileNameWithoutExtension(file);  // ?? → If Tag = null → take a file name, if it's not take Tag
                    string artist = tag.Tag.FirstPerformer ?? "Unknown";
                    int year = (int)tag.Tag.Year;
                    TimeSpan duration = tag.Properties.Duration;
                    long size = new FileInfo(file).Length;
                    string featuring = tag.Tag.JoinedPerformers ?? "Unknown";

                    // 20MB limit
                    if (size > 20 * 1024 * 1024)
                        continue;

                    // compute hash once and store mapping
                    string hash = Helper.HashFile(file);
                    hashToPath[hash] = file;

                    dataGridViewLocal.Rows.Add(
                        title,
                        artist,
                        year,
                        duration.ToString(@"mm\:ss"),
                        $"{Math.Round(size / 1024f / 1024f, 2)} MB",
                        featuring,
                        file,       // Hidden file column
                        hash        // Hidden hash column
                    );
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error reading tags from {file}: {ex.Message}");
                }
            }
        }

        private List<Song> GetLocalSongs()
        {
            var list = new List<Song>();
            foreach (DataGridViewRow row in dataGridViewLocal.Rows)
            {
                if (row.IsNewRow) continue;

                var s = new Song
                {
                    Title = row.Cells["Title"].Value?.ToString() ?? "",    // ?. → it's used for : Try calling .ToString(), but if the object on the left is null, don't call anything, just return null.
                    Artist = row.Cells["Artist"].Value?.ToString() ?? "",  // ?? → If the result is null, substitute an empty string "" like this we have just an empty string
                    Year = int.TryParse(row.Cells["Year"].Value?.ToString(), out int y) ? y : 0,
                    Duration = TimeSpan.TryParse(row.Cells["Duration"].Value?.ToString(), out TimeSpan t) ? t : TimeSpan.Zero,
                    Size = ParseSizeToBytes(row.Cells["Size"].Value?.ToString()),
                    Featuring = (row.Cells["Featuring"].Value?.ToString() ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries),
                    Hash = row.Cells["Hash"].Value?.ToString() ?? ""
                };
                list.Add(s);
            }
            return list;
        }

        private string GetLocalSongPath(string hash)
        {
            if (string.IsNullOrEmpty(hash)) return "";
            return hashToPath.TryGetValue(hash, out var path) ? path : "";
        }

        // called when a file is received & verified by NetworkProtocol
        private void OnFileReceived(string hash, string savedPath)
        {
            // add to mapping and UI (if not already)
            if (!hashToPath.ContainsKey(hash))
                hashToPath[hash] = savedPath;

            // add to grid on UI thread
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AddDownloadedFileToGrid(savedPath, hash)));
            }
            else AddDownloadedFileToGrid(savedPath, hash);
        }

        private void AddDownloadedFileToGrid(string path, string hash)
        {
            try
            {
                var tag = TagLib.File.Create(path);
                string title = tag.Tag.Title ?? Path.GetFileNameWithoutExtension(path);
                string artist = tag.Tag.FirstPerformer ?? "Unknown";
                int year = (int)tag.Tag.Year;
                TimeSpan duration = tag.Properties.Duration;
                long size = new FileInfo(path).Length;
                string featuring = tag.Tag.JoinedPerformers ?? "Unknown";

                dataGridViewLocal.Rows.Add(
                    title,
                    artist,
                    year,
                    duration.ToString(@"mm\:ss"),
                    $"{Math.Round(size / 1024f / 1024f, 2)} MB",
                    featuring,
                    path,
                    hash
                );
                MessageBox.Show($"File successfully downloaded!\nSaved in: {path}",
                             "Download Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // If there was an error reading tags, the file still downloaded, but the tags were not read
                MessageBox.Show($"The file was downloaded, but there was an error reading tags: {path}", "Warning");
            }
        }

        private void LoadGlobalSongs(string folderPath)
        {
            dataGridViewGlobal.Rows.Clear();  //Clearing all rows of global dataGrid


        }

        // request global catalog Button
        private async void button1_Click(object sender, EventArgs e)
        {
            // broadcast catalog request
            protocol.AskCatalog("*");

            // wait shortly for responses 
            await Task.Delay(1000);

            // read aggregated catalog if available (NetworkProtocol exposes GetAggregatedCatalog method)
            if (protocol is NetworkProtocol net)
            {
                // Updating the global list
                dataGridViewGlobal.Rows.Clear();
                var rows = net.GetAggregatedCatalog();
                foreach (var (song, peer) in rows)
                {
                    dataGridViewGlobal.Rows.Add(
                        song.Title, song.Artist, song.Year,
                        song.Duration.ToString(@"mm\:ss"),
                        $"{Math.Round(song.Size / 1024.0 / 1024.0, 2)} MB",
                        string.Join(", ", song.Featuring ?? Array.Empty<string>()),
                        song.Hash,
                        peer
                    );
                }
            }
        }

        private int ParseSizeToBytes(string? sizeStr)
        {
            if (string.IsNullOrEmpty(sizeStr)) return 0;
            if (sizeStr!.EndsWith("MB", StringComparison.OrdinalIgnoreCase) &&
                float.TryParse(sizeStr.Replace("MB", "").Trim(), out float mb))
            {
                return (int)(mb * 1024f * 1024f);
            }
            return int.TryParse(sizeStr, out int val) ? val : 0;
        }

        // double-click to download a song
        private void dataGridViewGlobal_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dataGridViewGlobal.Rows[e.RowIndex];
            string hash = row.Cells["Hash"].Value?.ToString() ?? "";
            string source = row.Cells["Source"].Value?.ToString() ?? "";

            // get the song title for beauty
            string title = row.Cells["Title"].Value?.ToString() ?? "Unknown";

            if (!string.IsNullOrEmpty(hash) && !string.IsNullOrEmpty(source))
            {
                // download message
                MessageBox.Show($"The request has been sent to the user. {source}.\nDownload: {title}...",
                                "Download Started", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // start the download process
                ((NetworkProtocol)protocol).AskMedia(source, hash);
            }
        }

        // Browse local files Button
        private void button1_Click_1(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBox_Research.Text = dialog.SelectedPath; // show the path
                    LoadLocalSongs(dialog.SelectedPath);         // charge all (wav, ogg, mp3)
                }
            }
        }

        private void TimeDelayPicker_Scroll(object sender, EventArgs e)
        {
        }

        private void labelTimeout_Click(object sender, EventArgs e)
        {

        }

        private void dataGridViewLocal_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridViewGlobal_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void SetupLocalGrid()
        {
            dataGridViewLocal.Columns.Clear();

            dataGridViewLocal.Columns.Add("Title", "Titre");
            dataGridViewLocal.Columns.Add("Artist", "Artiste");
            dataGridViewLocal.Columns.Add("Year", "Année");
            dataGridViewLocal.Columns.Add("Duration", "Durée");
            dataGridViewLocal.Columns.Add("Size", "Taille");
            dataGridViewLocal.Columns.Add("Featuring", "Featuring");

            var pathCol = new DataGridViewTextBoxColumn { Name = "Path", HeaderText = "Chemin", Visible = false };
            dataGridViewLocal.Columns.Add(pathCol);

            var hashCol = new DataGridViewTextBoxColumn { Name = "Hash", HeaderText = "Hash", Visible = false };
            dataGridViewLocal.Columns.Add(hashCol);
        }
        private void SetupGlobalGrid()
        {
            dataGridViewGlobal.Columns.Clear();

            dataGridViewGlobal.Columns.Add("Title", "Titre");
            dataGridViewGlobal.Columns.Add("Artist", "Artiste");
            dataGridViewGlobal.Columns.Add("Year", "Année");
            dataGridViewGlobal.Columns.Add("Duration", "Durée");
            dataGridViewGlobal.Columns.Add("Size", "Taille");
            dataGridViewGlobal.Columns.Add("Featuring", "Featuring");

            var hashCol = new DataGridViewTextBoxColumn { Name = "Hash", HeaderText = "Hash", Visible = false };
            dataGridViewGlobal.Columns.Add(hashCol);

            var sourceCol = new DataGridViewTextBoxColumn { Name = "Source", HeaderText = "Source", Visible = true };
            dataGridViewGlobal.Columns.Add(sourceCol);
        }


        private void StyleDataGrid(DataGridView grid)
        {
            grid.BackgroundColor = Color.FromArgb(203, 178, 107);
            grid.BorderStyle = BorderStyle.None;

            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(203, 178, 107);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            grid.DefaultCellStyle.BackColor = Color.FromArgb(203, 178, 107);
            grid.DefaultCellStyle.ForeColor = Color.Black;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 220, 255);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10);

            grid.RowTemplate.Height = 28;

            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.AllowUserToResizeRows = false;
            grid.AllowUserToResizeColumns = false;

            grid.GridColor = Color.Black;

            grid.RowHeadersVisible = false;
            grid.ReadOnly = true; // Disables editing (removes blinking)
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Selects the entire row, not just one cell
            grid.MultiSelect = false; // Allows you to select only one song at a time
        }


        private void TimoutDelayPicker_Scroll(object sender, EventArgs e)
        {

        }

        private void buttonRefreshNetwork_Click(object sender, EventArgs e)
        {
            ((NetworkProtocol)protocol).SayOnline();
            ((NetworkProtocol)protocol).AskCatalog("*");
            Task.Delay(1000).ContinueWith(_ => RefreshGlobalGrid());
        }

        private void RefreshGlobalGrid()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefreshGlobalGrid));
                return;
            }

            // USE GetAggregatedCatalog INSTEAD OF GetAllKnownSongs
            // To get not only the song, but also the name of the person who owns it (peer)
            var rows = ((NetworkProtocol)protocol).GetAggregatedCatalog();

            dataGridViewGlobal.Rows.Clear();

            foreach (var (s, peer) in rows)
            {
                dataGridViewGlobal.Rows.Add(
                    s.Title,
                    s.Artist,
                    s.Year,
                    s.Duration.ToString(@"mm\:ss"),
                    $"{s.Size / 1024f / 1024f:F2} MB",
                    string.Join(", ", s.Featuring),
                    s.Hash,
                    peer
                );
            }
        }

        private void btnTestSend_Click(object sender, EventArgs e)
        {
            // "*" broadcast to everyone (MQTTX listener)
            protocol.SendCatalog("*");

            MessageBox.Show("Catalog sent to MQTT! Check MQTTX.");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_2(object sender, EventArgs e)
        {// if song was selected
            if (dataGridViewGlobal.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a song from the list");
                return;
            }

            // taking firs line selected
            var row = dataGridViewGlobal.SelectedRows[0];

            string hash = row.Cells["Hash"].Value?.ToString() ?? "";
            string source = row.Cells["Source"].Value?.ToString() ?? "";
            string title = row.Cells["Title"].Value?.ToString() ?? "Unknown";

            if (!string.IsNullOrEmpty(hash) && !string.IsNullOrEmpty(source))
            {
                MessageBox.Show($"Request sent to user {source}.\nDownload: {title}...",
                                "Download Started", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ((NetworkProtocol)protocol).AskMedia(source, hash);
            }

        }
    }
}
