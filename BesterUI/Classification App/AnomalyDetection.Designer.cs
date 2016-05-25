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
            this.btn_getData.Location = new System.Drawing.Point(13, 43);
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
            this.label1.Location = new System.Drawing.Point(12, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Status:";
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(58, 69);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(33, 13);
            this.statusLabel.TabIndex = 4;
            this.statusLabel.Text = "None";
            // 
            // AnomalyDetection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 368);
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
    }
}