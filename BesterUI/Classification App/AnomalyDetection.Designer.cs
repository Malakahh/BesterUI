namespace Classification_App
{
    partial class AnomalyDetection
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_loadData = new System.Windows.Forms.Button();
            this.btn_getData = new System.Windows.Forms.Button();
            this.useRestInTraining = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.load_data_from_files = new System.Windows.Forms.Button();
            this.runAllButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.gsrProgress = new System.Windows.Forms.ProgressBar();
            this.eegProgress = new System.Windows.Forms.ProgressBar();
            this.hrProgress = new System.Windows.Forms.ProgressBar();
            this.faceProgress = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // btn_loadData
            // 
            this.btn_loadData.Location = new System.Drawing.Point(13, 13);
            this.btn_loadData.Name = "btn_loadData";
            this.btn_loadData.Size = new System.Drawing.Size(75, 23);
            this.btn_loadData.TabIndex = 0;
            this.btn_loadData.Text = "Load Data";
            this.btn_loadData.UseVisualStyleBackColor = true;
            this.btn_loadData.Click += new System.EventHandler(this.btn_loadData_Click);
            // 
            // btn_getData
            // 
            this.btn_getData.Enabled = false;
            this.btn_getData.Location = new System.Drawing.Point(12, 77);
            this.btn_getData.Name = "btn_getData";
            this.btn_getData.Size = new System.Drawing.Size(75, 23);
            this.btn_getData.TabIndex = 1;
            this.btn_getData.Text = "Get Data Snippets";
            this.btn_getData.UseVisualStyleBackColor = true;
            this.btn_getData.Click += new System.EventHandler(this.btn_getData_Click);
            // 
            // useRestInTraining
            // 
            this.useRestInTraining.AutoSize = true;
            this.useRestInTraining.Location = new System.Drawing.Point(122, 18);
            this.useRestInTraining.Name = "useRestInTraining";
            this.useRestInTraining.Size = new System.Drawing.Size(113, 17);
            this.useRestInTraining.TabIndex = 2;
            this.useRestInTraining.Text = "Use rest in training";
            this.useRestInTraining.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 103);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Status:";
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(57, 103);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(33, 13);
            this.statusLabel.TabIndex = 4;
            this.statusLabel.Text = "None";
            // 
            // load_data_from_files
            // 
            this.load_data_from_files.Location = new System.Drawing.Point(12, 42);
            this.load_data_from_files.Name = "load_data_from_files";
            this.load_data_from_files.Size = new System.Drawing.Size(137, 23);
            this.load_data_from_files.TabIndex = 5;
            this.load_data_from_files.Text = "Load Data From files";
            this.load_data_from_files.UseVisualStyleBackColor = true;
            this.load_data_from_files.Click += new System.EventHandler(this.load_data_from_files_Click);
            // 
            // runAllButton
            // 
            this.runAllButton.Location = new System.Drawing.Point(13, 157);
            this.runAllButton.Name = "runAllButton";
            this.runAllButton.Size = new System.Drawing.Size(137, 23);
            this.runAllButton.TabIndex = 6;
            this.runAllButton.Text = "Calculate All features";
            this.runAllButton.UseVisualStyleBackColor = true;
            this.runAllButton.Click += new System.EventHandler(this.runAllButton_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(14, 186);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(137, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Calculate anomalis";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(252, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "GSR";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(252, 135);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "EEG";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(252, 196);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "HR";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(252, 245);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "FACE";
            // 
            // gsrProgress
            // 
            this.gsrProgress.Location = new System.Drawing.Point(255, 103);
            this.gsrProgress.Maximum = 1098;
            this.gsrProgress.Name = "gsrProgress";
            this.gsrProgress.Size = new System.Drawing.Size(319, 23);
            this.gsrProgress.TabIndex = 12;
            // 
            // eegProgress
            // 
            this.eegProgress.Location = new System.Drawing.Point(255, 157);
            this.eegProgress.Maximum = 1098;
            this.eegProgress.Name = "eegProgress";
            this.eegProgress.Size = new System.Drawing.Size(319, 23);
            this.eegProgress.TabIndex = 13;
            // 
            // hrProgress
            // 
            this.hrProgress.Location = new System.Drawing.Point(255, 212);
            this.hrProgress.Maximum = 1098;
            this.hrProgress.Name = "hrProgress";
            this.hrProgress.Size = new System.Drawing.Size(319, 23);
            this.hrProgress.TabIndex = 14;
            // 
            // faceProgress
            // 
            this.faceProgress.Location = new System.Drawing.Point(255, 261);
            this.faceProgress.Maximum = 1098;
            this.faceProgress.Name = "faceProgress";
            this.faceProgress.Size = new System.Drawing.Size(319, 23);
            this.faceProgress.TabIndex = 15;
            // 
            // AnomalyDetection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 368);
            this.Controls.Add(this.faceProgress);
            this.Controls.Add(this.hrProgress);
            this.Controls.Add(this.eegProgress);
            this.Controls.Add(this.gsrProgress);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.runAllButton);
            this.Controls.Add(this.load_data_from_files);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.useRestInTraining);
            this.Controls.Add(this.btn_getData);
            this.Controls.Add(this.btn_loadData);
            this.Name = "AnomalyDetection";
            this.Text = "AnomalyDetection";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_loadData;
        private System.Windows.Forms.Button btn_getData;
        private System.Windows.Forms.CheckBox useRestInTraining;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button load_data_from_files;
        private System.Windows.Forms.Button runAllButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ProgressBar gsrProgress;
        private System.Windows.Forms.ProgressBar eegProgress;
        private System.Windows.Forms.ProgressBar hrProgress;
        private System.Windows.Forms.ProgressBar faceProgress;
    }
}