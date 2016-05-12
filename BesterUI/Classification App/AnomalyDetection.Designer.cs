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
            this.btn_getData.Location = new System.Drawing.Point(13, 43);
            this.btn_getData.Name = "btn_getData";
            this.btn_getData.Size = new System.Drawing.Size(75, 23);
            this.btn_getData.TabIndex = 1;
            this.btn_getData.Text = "Get Data Snippets";
            this.btn_getData.UseVisualStyleBackColor = true;
            this.btn_getData.Click += new System.EventHandler(this.btn_getData_Click);
            // 
            // AnomalyDetection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 368);
            this.Controls.Add(this.btn_getData);
            this.Controls.Add(this.btn_loadData);
            this.Name = "AnomalyDetection";
            this.Text = "AnomalyDetection";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_loadData;
        private System.Windows.Forms.Button btn_getData;
    }
}