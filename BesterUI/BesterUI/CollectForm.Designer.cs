namespace BesterUI
{
    partial class CollectForm
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
            this.logTextBox = new System.Windows.Forms.RichTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.hrPort = new System.Windows.Forms.Label();
            this.gsrPort = new System.Windows.Forms.Label();
            this.eegPort = new System.Windows.Forms.Label();
            this.hrReady = new System.Windows.Forms.Panel();
            this.gsrReady = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.eegReady = new System.Windows.Forms.Panel();
            this.gsrLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.hrdwareStatusLabel = new System.Windows.Forms.Label();
            this.btnVerifySensors = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.collectingDataPanel = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // logTextBox
            // 
            this.logTextBox.Location = new System.Drawing.Point(12, 175);
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.Size = new System.Drawing.Size(797, 124);
            this.logTextBox.TabIndex = 2;
            this.logTextBox.Text = "";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.hrPort);
            this.panel1.Controls.Add(this.gsrPort);
            this.panel1.Controls.Add(this.eegPort);
            this.panel1.Controls.Add(this.hrReady);
            this.panel1.Controls.Add(this.gsrReady);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.eegReady);
            this.panel1.Controls.Add(this.gsrLabel);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(12, 25);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(102, 83);
            this.panel1.TabIndex = 3;
            // 
            // hrPort
            // 
            this.hrPort.AutoSize = true;
            this.hrPort.Location = new System.Drawing.Point(32, 58);
            this.hrPort.Name = "hrPort";
            this.hrPort.Size = new System.Drawing.Size(40, 13);
            this.hrPort.TabIndex = 10;
            this.hrPort.Text = "- - - - - -";
            // 
            // gsrPort
            // 
            this.gsrPort.AutoSize = true;
            this.gsrPort.Location = new System.Drawing.Point(32, 35);
            this.gsrPort.Name = "gsrPort";
            this.gsrPort.Size = new System.Drawing.Size(43, 13);
            this.gsrPort.TabIndex = 9;
            this.gsrPort.Text = "- - - - - - ";
            // 
            // eegPort
            // 
            this.eegPort.AutoSize = true;
            this.eegPort.Location = new System.Drawing.Point(32, 12);
            this.eegPort.Name = "eegPort";
            this.eegPort.Size = new System.Drawing.Size(40, 13);
            this.eegPort.TabIndex = 8;
            this.eegPort.Text = "- - - - - -";
            // 
            // hrReady
            // 
            this.hrReady.BackColor = System.Drawing.Color.Red;
            this.hrReady.Location = new System.Drawing.Point(75, 56);
            this.hrReady.Name = "hrReady";
            this.hrReady.Size = new System.Drawing.Size(16, 16);
            this.hrReady.TabIndex = 7;
            // 
            // gsrReady
            // 
            this.gsrReady.BackColor = System.Drawing.Color.Red;
            this.gsrReady.Location = new System.Drawing.Point(75, 33);
            this.gsrReady.Name = "gsrReady";
            this.gsrReady.Size = new System.Drawing.Size(16, 16);
            this.gsrReady.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "HR";
            // 
            // eegReady
            // 
            this.eegReady.BackColor = System.Drawing.Color.Red;
            this.eegReady.Location = new System.Drawing.Point(75, 10);
            this.eegReady.Name = "eegReady";
            this.eegReady.Size = new System.Drawing.Size(16, 16);
            this.eegReady.TabIndex = 5;
            // 
            // gsrLabel
            // 
            this.gsrLabel.AutoSize = true;
            this.gsrLabel.Location = new System.Drawing.Point(3, 36);
            this.gsrLabel.Name = "gsrLabel";
            this.gsrLabel.Size = new System.Drawing.Size(30, 13);
            this.gsrLabel.TabIndex = 6;
            this.gsrLabel.Text = "GSR";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "EEG";
            // 
            // hrdwareStatusLabel
            // 
            this.hrdwareStatusLabel.AutoSize = true;
            this.hrdwareStatusLabel.Location = new System.Drawing.Point(15, 9);
            this.hrdwareStatusLabel.Name = "hrdwareStatusLabel";
            this.hrdwareStatusLabel.Size = new System.Drawing.Size(86, 13);
            this.hrdwareStatusLabel.TabIndex = 4;
            this.hrdwareStatusLabel.Text = "Hardware Status";
            // 
            // btnVerifySensors
            // 
            this.btnVerifySensors.Location = new System.Drawing.Point(12, 114);
            this.btnVerifySensors.Name = "btnVerifySensors";
            this.btnVerifySensors.Size = new System.Drawing.Size(140, 55);
            this.btnVerifySensors.TabIndex = 5;
            this.btnVerifySensors.Text = "Verify sensors";
            this.btnVerifySensors.UseVisualStyleBackColor = true;
            this.btnVerifySensors.Click += new System.EventHandler(this.btnVerifySensors_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(664, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(140, 157);
            this.button2.TabIndex = 6;
            this.button2.Text = "START COLLECT";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // collectingDataPanel
            // 
            this.collectingDataPanel.BackColor = System.Drawing.Color.Red;
            this.collectingDataPanel.Location = new System.Drawing.Point(498, 12);
            this.collectingDataPanel.Name = "collectingDataPanel";
            this.collectingDataPanel.Size = new System.Drawing.Size(160, 157);
            this.collectingDataPanel.TabIndex = 7;
            // 
            // CollectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(816, 311);
            this.Controls.Add(this.collectingDataPanel);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnVerifySensors);
            this.Controls.Add(this.hrdwareStatusLabel);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.logTextBox);
            this.Name = "CollectForm";
            this.Text = "CollectForm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RichTextBox logTextBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel hrReady;
        private System.Windows.Forms.Panel gsrReady;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel eegReady;
        private System.Windows.Forms.Label gsrLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label hrdwareStatusLabel;
        private System.Windows.Forms.Button btnVerifySensors;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label hrPort;
        private System.Windows.Forms.Label gsrPort;
        private System.Windows.Forms.Label eegPort;
        private System.Windows.Forms.Panel collectingDataPanel;
    }
}