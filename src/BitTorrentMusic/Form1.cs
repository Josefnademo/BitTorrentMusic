using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;          // To work with Directory, FileInfo, Path
using System.Diagnostics;
using TagLib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BitTorrentMusic
{
    public partial class MainMenuUI : Form
    {
        private IProtocol protocol;


        public MainMenuUI()
        {
            InitializeComponent();
            protocol = new NetworkProtocol("YosefLocal"); //MQTT realisation
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

                    string title = tag.Tag.Title ?? Path.GetFileNameWithoutExtension(file);
                    string artist = tag.Tag.FirstPerformer ?? "Unknown";
                    int year = (int)tag.Tag.Year;
                    TimeSpan duration = tag.Properties.Duration;
                    long size = new FileInfo(file).Length;
                    string featuring = tag.Tag.JoinedPerformers ?? "Unknown";

                    // 20MB limit
                    if (size > 20 * 1024 * 1024)
                        continue;

                    dataGridViewLocal.Rows.Add(
                        title,
                        artist,
                        year,
                        duration.ToString(@"mm\:ss"),
                        $"{Math.Round(size / 1024f / 1024f, 2)} MB",
                        featuring,
                        file
                    );
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error reading tags from {file}: {ex.Message}");
                }
            }
        }



        private void LoadGlobalSongs(string folderPath)
        {
            dataGridViewGlobal.Rows.Clear();  //Clearing all rows of global dataGrid


        }

        // request global catalog Button
        private void button1_Click(object sender, EventArgs e)
        {
           // protocol.AskCatalog(targetName);

        }
        /*
        private void dataGridViewGlobal_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // 1. Берём заголовок трека (не обязательно)
            string title = dataGridViewGlobal.Rows[e.RowIndex].Cells["Title"].Value.ToString();

            // 2. Берём хэш MP3
            string hash = dataGridViewGlobal.Rows[e.RowIndex].Cells["Hash"].Value.ToString();

            // 3. Берём размер чтобы понять диапазоны
            string sizeStr = dataGridViewGlobal.Rows[e.RowIndex].Cells["Size"].Value.ToString();
            int size = ParseSizeToBytes(sizeStr);

            // 4. Узнаём у кого скачиваем
            string target = "ip_or_name_mediatheque"; // позже подставишь значение

            // 5. Запрашиваем файл
            protocol.AskMedia(target, 0, size - 1, hash);

            MessageBox.Show($"Downloading: {title}");
        }
        */

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

            var pathCol = new DataGridViewTextBoxColumn();
            pathCol.Name = "Path";
            pathCol.HeaderText = "Chemin";
            pathCol.Visible = false;

            dataGridViewLocal.Columns.Add(pathCol);
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

            var hashCol = new DataGridViewTextBoxColumn();
            hashCol.Name = "Hash";
            hashCol.Visible = false;

            dataGridViewGlobal.Columns.Add(hashCol);
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
        }

        private void TimoutDelayPicker_Scroll(object sender, EventArgs e)
        {

        }
    }
}
