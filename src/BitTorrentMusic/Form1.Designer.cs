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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainMenuUI));
            request_global_catalog = new Button();
            Browse_button = new Button();
            textBox_Research = new TextBox();
            bindingSource1 = new BindingSource(components);
            ((System.ComponentModel.ISupportInitialize)bindingSource1).BeginInit();
            SuspendLayout();
            // 
            // request_global_catalog
            // 
            request_global_catalog.BackColor = Color.Gold;
            resources.ApplyResources(request_global_catalog, "request_global_catalog");
            request_global_catalog.Name = "request_global_catalog";
            request_global_catalog.UseVisualStyleBackColor = false;
            request_global_catalog.Click += button1_Click;
            // 
            // Browse_button
            // 
            Browse_button.BackColor = Color.Gold;
            resources.ApplyResources(Browse_button, "Browse_button");
            Browse_button.Name = "Browse_button";
            Browse_button.UseVisualStyleBackColor = false;
            Browse_button.Click += button1_Click_1;
            // 
            // textBox_Research
            // 
            resources.ApplyResources(textBox_Research, "textBox_Research");
            textBox_Research.Name = "textBox_Research";
            // 
            // MainMenuUI
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(textBox_Research);
            Controls.Add(Browse_button);
            Controls.Add(request_global_catalog);
            IsMdiContainer = true;
            Name = "MainMenuUI";
            Opacity = 0.1D;
            TransparencyKey = Color.FromArgb(64, 0, 64);
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)bindingSource1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button request_global_catalog;
        private Button Browse_button;
        private TextBox textBox_Research;
        private BindingSource bindingSource1;
    }
}
