using Microsoft.VisualBasic.ApplicationServices;
using System.Windows.Forms;
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

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = "C:\\";
            dialog.Filter = "MP3 files (*.mp3)|*.mp3";
            dialog.FilterIndex = 0;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox_Research.Text = dialog.FileName; // path in TextBox
            }
        }

        private void TimeDelayPicker_Scroll(object sender, EventArgs e)
        {
        }

        private void labelTimeout_Click(object sender, EventArgs e)
        {

        }
    }
}
