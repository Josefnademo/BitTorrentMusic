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
using System.IO;          // Для работы с Directory, FileInfo, Path
using System.Diagnostics;
using TagLib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BitTorrentMusic
{
    public partial class MainMenuUI : Form
    {
        public MainMenuUI()
        {
            InitializeComponent();
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

                    // 20MB limit
                    if (size > 20 * 1024 * 1024)
                        continue;

                    dataGridViewLocal.Rows.Add(
                        title,
                        artist,
                        year,
                        duration.ToString(@"mm\:ss"),
                        $"{Math.Round(size / 1024f / 1024f, 2)} MB",
                        file
                    );
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error reading tags from {file}: {ex.Message}");
                }
            }
        }



        private void LoadGlobalSongs(string folderPath) {
        dataGridViewGlobal.Rows.Clear();  //Clearing all rows of global dataGrid


        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

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
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(203,178,107);
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

            // No weird alternating transparency
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;


        }

    }
}
