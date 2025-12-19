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
            TimoutDelayPicker = new TrackBar();
            dataGridViewLocal = new DataGridView();
            dataGridViewGlobal = new DataGridView();
            labelTimeout = new Label();
            label_Local = new Label();
            label_Global = new Label();
            buttonRefreshNetwork = new Button();
            btnTestSend = new Button();
            ((System.ComponentModel.ISupportInitialize)bindingSource1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)TimoutDelayPicker).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewLocal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewGlobal).BeginInit();
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
            textBox_Research.BackColor = Color.Gray;
            resources.ApplyResources(textBox_Research, "textBox_Research");
            textBox_Research.Name = "textBox_Research";
            // 
            // TimoutDelayPicker
            // 
            resources.ApplyResources(TimoutDelayPicker, "TimoutDelayPicker");
            TimoutDelayPicker.BackColor = Color.Gray;
            TimoutDelayPicker.Cursor = Cursors.Hand;
            TimoutDelayPicker.Maximum = 300;
            TimoutDelayPicker.Name = "TimoutDelayPicker";
            TimoutDelayPicker.SmallChange = 2;
            TimoutDelayPicker.Scroll += TimoutDelayPicker_Scroll;
            // 
            // dataGridViewLocal
            // 
            dataGridViewLocal.BackgroundColor = Color.Wheat;
            dataGridViewLocal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(dataGridViewLocal, "dataGridViewLocal");
            dataGridViewLocal.Name = "dataGridViewLocal";
            dataGridViewLocal.CellContentClick += dataGridViewLocal_CellContentClick;
            // 
            // dataGridViewGlobal
            // 
            dataGridViewGlobal.BackgroundColor = Color.Wheat;
            dataGridViewGlobal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(dataGridViewGlobal, "dataGridViewGlobal");
            dataGridViewGlobal.Name = "dataGridViewGlobal";
            dataGridViewGlobal.CellContentClick += dataGridViewGlobal_CellContentClick;
            // 
            // labelTimeout
            // 
            resources.ApplyResources(labelTimeout, "labelTimeout");
            labelTimeout.BackColor = Color.Transparent;
            labelTimeout.ForeColor = Color.AliceBlue;
            labelTimeout.Name = "labelTimeout";
            labelTimeout.Click += labelTimeout_Click;
            // 
            // label_Local
            // 
            resources.ApplyResources(label_Local, "label_Local");
            label_Local.BackColor = Color.NavajoWhite;
            label_Local.Name = "label_Local";
            // 
            // label_Global
            // 
            resources.ApplyResources(label_Global, "label_Global");
            label_Global.BackColor = Color.NavajoWhite;
            label_Global.Name = "label_Global";
            // 
            // buttonRefreshNetwork
            // 
            resources.ApplyResources(buttonRefreshNetwork, "buttonRefreshNetwork");
            buttonRefreshNetwork.Name = "buttonRefreshNetwork";
            buttonRefreshNetwork.UseVisualStyleBackColor = true;
            buttonRefreshNetwork.Click += buttonRefreshNetwork_Click;
            // 
            // btnTestSend
            // 
            btnTestSend.BackColor = Color.Coral;
            resources.ApplyResources(btnTestSend, "btnTestSend");
            btnTestSend.Name = "btnTestSend";
            btnTestSend.UseVisualStyleBackColor = false;
            btnTestSend.Click += btnTestSend_Click;
            // 
            // MainMenuUI
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            Controls.Add(btnTestSend);
            Controls.Add(buttonRefreshNetwork);
            Controls.Add(label_Global);
            Controls.Add(label_Local);
            Controls.Add(labelTimeout);
            Controls.Add(dataGridViewGlobal);
            Controls.Add(dataGridViewLocal);
            Controls.Add(TimoutDelayPicker);
            Controls.Add(textBox_Research);
            Controls.Add(Browse_button);
            Controls.Add(request_global_catalog);
            Name = "MainMenuUI";
            TransparencyKey = Color.White;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)bindingSource1).EndInit();
            ((System.ComponentModel.ISupportInitialize)TimoutDelayPicker).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewLocal).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewGlobal).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button request_global_catalog;
        private Button Browse_button;
        private TextBox textBox_Research;
        private BindingSource bindingSource1;
        private TrackBar TimoutDelayPicker;
        private DataGridView dataGridViewLocal;
        private DataGridView dataGridViewGlobal;
        private Label labelTimeout;
        private Label label_Local;
        private Label label_Global;
        private Button buttonRefreshNetwork;
        private Button btnTestSend;
    }
}
