namespace SecondTest
{
    partial class WriteMessageForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WriteMessageForm));
            this.richtext_mail_body = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textbox_mail_to = new System.Windows.Forms.TextBox();
            this.btn_mail_send = new System.Windows.Forms.Button();
            this.btn_mail_save = new System.Windows.Forms.Button();
            this.btn_msg_contacts = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textbox_mail_title = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // richtext_mail_body
            // 
            this.richtext_mail_body.Location = new System.Drawing.Point(12, 54);
            this.richtext_mail_body.Name = "richtext_mail_body";
            this.richtext_mail_body.Size = new System.Drawing.Size(602, 244);
            this.richtext_mail_body.TabIndex = 0;
            this.richtext_mail_body.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "To:";
            // 
            // textbox_mail_to
            // 
            this.textbox_mail_to.Location = new System.Drawing.Point(43, 28);
            this.textbox_mail_to.Name = "textbox_mail_to";
            this.textbox_mail_to.Size = new System.Drawing.Size(100, 20);
            this.textbox_mail_to.TabIndex = 2;
            // 
            // btn_mail_send
            // 
            this.btn_mail_send.Image = global::SecondTest.Properties.Resources.reply;
            this.btn_mail_send.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_mail_send.Location = new System.Drawing.Point(539, 26);
            this.btn_mail_send.Name = "btn_mail_send";
            this.btn_mail_send.Size = new System.Drawing.Size(75, 23);
            this.btn_mail_send.TabIndex = 3;
            this.btn_mail_send.Text = "Send";
            this.btn_mail_send.UseVisualStyleBackColor = true;
            this.btn_mail_send.Click += new System.EventHandler(this.btn_mail_send_Click);
            // 
            // btn_mail_save
            // 
            this.btn_mail_save.Location = new System.Drawing.Point(449, 26);
            this.btn_mail_save.Name = "btn_mail_save";
            this.btn_mail_save.Size = new System.Drawing.Size(75, 23);
            this.btn_mail_save.TabIndex = 4;
            this.btn_mail_save.Text = "Save";
            this.btn_mail_save.UseVisualStyleBackColor = true;
            this.btn_mail_save.Click += new System.EventHandler(this.btn_mail_save_Click);
            // 
            // btn_msg_contacts
            // 
            this.btn_msg_contacts.Image = ((System.Drawing.Image)(resources.GetObject("btn_msg_contacts.Image")));
            this.btn_msg_contacts.Location = new System.Drawing.Point(149, 26);
            this.btn_msg_contacts.Name = "btn_msg_contacts";
            this.btn_msg_contacts.Size = new System.Drawing.Size(23, 23);
            this.btn_msg_contacts.TabIndex = 5;
            this.btn_msg_contacts.UseVisualStyleBackColor = true;
            this.btn_msg_contacts.Click += new System.EventHandler(this.btn_msg_contacts_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(208, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Title:";
            // 
            // textbox_mail_title
            // 
            this.textbox_mail_title.Location = new System.Drawing.Point(244, 27);
            this.textbox_mail_title.Name = "textbox_mail_title";
            this.textbox_mail_title.Size = new System.Drawing.Size(199, 20);
            this.textbox_mail_title.TabIndex = 7;
            // 
            // WriteMessageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 310);
            this.Controls.Add(this.textbox_mail_title);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btn_msg_contacts);
            this.Controls.Add(this.btn_mail_save);
            this.Controls.Add(this.btn_mail_send);
            this.Controls.Add(this.textbox_mail_to);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richtext_mail_body);
            this.Name = "WriteMessageForm";
            this.Text = "WriteMessageForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richtext_mail_body;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textbox_mail_to;
        private System.Windows.Forms.Button btn_mail_send;
        private System.Windows.Forms.Button btn_mail_save;
        private System.Windows.Forms.Button btn_msg_contacts;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textbox_mail_title;
    }
}