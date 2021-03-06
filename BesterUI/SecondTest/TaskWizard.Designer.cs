﻿namespace SecondTest
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
            this.btnTaskComplete.BackColor = System.Drawing.Color.ForestGreen;
            this.btnTaskComplete.Enabled = false;
            this.btnTaskComplete.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTaskComplete.Location = new System.Drawing.Point(191, 178);
            this.btnTaskComplete.Name = "btnTaskComplete";
            this.btnTaskComplete.Size = new System.Drawing.Size(172, 82);
            this.btnTaskComplete.TabIndex = 0;
            this.btnTaskComplete.Text = "On KEYBOARD, press green for:\r\nI completed the task";
            this.btnTaskComplete.UseVisualStyleBackColor = false;
            // 
            // btnTaskIncomplete
            // 
            this.btnTaskIncomplete.BackColor = System.Drawing.Color.Red;
            this.btnTaskIncomplete.Enabled = false;
            this.btnTaskIncomplete.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTaskIncomplete.Location = new System.Drawing.Point(12, 178);
            this.btnTaskIncomplete.Name = "btnTaskIncomplete";
            this.btnTaskIncomplete.Size = new System.Drawing.Size(173, 82);
            this.btnTaskIncomplete.TabIndex = 1;
            this.btnTaskIncomplete.Text = "On KEYBOARD, press red for:\r\nI was UNABLE to complete the task";
            this.btnTaskIncomplete.UseVisualStyleBackColor = false;
            // 
            // TaskWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 270);
            this.Controls.Add(this.btnTaskIncomplete);
            this.Controls.Add(this.btnTaskComplete);
            this.Name = "TaskWizard";
            this.Text = "TaskWizard";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTaskComplete;
        private System.Windows.Forms.Button btnTaskIncomplete;
    }
}