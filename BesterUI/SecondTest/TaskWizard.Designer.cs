namespace SecondTest
{
    partial class TaskWizard
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
            this.btnTaskComplete = new System.Windows.Forms.Button();
            this.btnTaskIncomplete = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnTaskComplete
            // 
            this.btnTaskComplete.Location = new System.Drawing.Point(191, 178);
            this.btnTaskComplete.Name = "btnTaskComplete";
            this.btnTaskComplete.Size = new System.Drawing.Size(172, 71);
            this.btnTaskComplete.TabIndex = 0;
            this.btnTaskComplete.Text = "I completed the task";
            this.btnTaskComplete.UseVisualStyleBackColor = true;
            // 
            // btnTaskIncomplete
            // 
            this.btnTaskIncomplete.Location = new System.Drawing.Point(12, 178);
            this.btnTaskIncomplete.Name = "btnTaskIncomplete";
            this.btnTaskIncomplete.Size = new System.Drawing.Size(173, 71);
            this.btnTaskIncomplete.TabIndex = 1;
            this.btnTaskIncomplete.Text = "I was UNABLE to complete the task";
            this.btnTaskIncomplete.UseVisualStyleBackColor = true;
            // 
            // TaskWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 261);
            this.Controls.Add(this.btnTaskIncomplete);
            this.Controls.Add(this.btnTaskComplete);
            this.Name = "TaskWizard";
            this.Text = "TaskWizard";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTaskComplete;
        private System.Windows.Forms.Button btnTaskIncomplete;
    }
}