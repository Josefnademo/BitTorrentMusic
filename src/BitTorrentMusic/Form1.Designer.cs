namespace BitTorrentMusic
{
    partial class MainMenuUI
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainMenuUI));
            request_global_catalog = new Button();
            SuspendLayout();
            // 
            // request_global_catalog
            // 
            request_global_catalog.BackColor = Color.Gold;
            request_global_catalog.Font = new Font("Modern No. 20", 18F);
            request_global_catalog.Location = new Point(268, 571);
            request_global_catalog.Name = "request_global_catalog";
            request_global_catalog.Size = new Size(281, 63);
            request_global_catalog.TabIndex = 0;
            request_global_catalog.Text = "request global catalog";
            request_global_catalog.UseVisualStyleBackColor = false;
            request_global_catalog.Click += button1_Click;
            // 
            // MainMenuUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(841, 684);
            Controls.Add(request_global_catalog);
            Icon = (Icon)resources.GetObject("$this.Icon");
            IsMdiContainer = true;
            Name = "MainMenuUI";
            Opacity = 0.1D;
            Text = "BitTorrentMusic";
            TransparencyKey = Color.FromArgb(64, 0, 64);
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private Button request_global_catalog;
    }
}
