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
            this.dummyDataBtn = new System.Windows.Forms.Button();
            this.loadFromFileBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // dummyDataBtn
            // 
            this.dummyDataBtn.Location = new System.Drawing.Point(12, 12);
            this.dummyDataBtn.Name = "dummyDataBtn";
            this.dummyDataBtn.Size = new System.Drawing.Size(105, 23);
            this.dummyDataBtn.TabIndex = 0;
            this.dummyDataBtn.Text = "CreateDummyData";
            this.dummyDataBtn.UseVisualStyleBackColor = true;
            this.dummyDataBtn.Click += new System.EventHandler(this.dummyDataBtn_Click);
            // 
            // loadFromFileBtn
            // 
            this.loadFromFileBtn.Location = new System.Drawing.Point(13, 42);
            this.loadFromFileBtn.Name = "loadFromFileBtn";
            this.loadFromFileBtn.Size = new System.Drawing.Size(104, 23);
            this.loadFromFileBtn.TabIndex = 1;
            this.loadFromFileBtn.Text = "Load From File";
            this.loadFromFileBtn.UseVisualStyleBackColor = true;
            this.loadFromFileBtn.Click += new System.EventHandler(this.loadFromFileBtn_Click);
            // 
            // CollectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.loadFromFileBtn);
            this.Controls.Add(this.dummyDataBtn);
            this.Name = "CollectForm";
            this.Text = "CollectForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button dummyDataBtn;
        private System.Windows.Forms.Button loadFromFileBtn;
    }
}