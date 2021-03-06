﻿namespace SecondTest
{
    partial class SecondTestForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SecondTestForm));
            this.emailList = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_delete = new System.Windows.Forms.Button();
            this.label_body = new System.Windows.Forms.Label();
            this.label_header = new System.Windows.Forms.Label();
            this.btn_new = new System.Windows.Forms.Button();
            this.btn_sent = new System.Windows.Forms.Button();
            this.btn_reply = new System.Windows.Forms.Button();
            this.Contacts = new System.Windows.Forms.Button();
            this.btn_draft = new System.Windows.Forms.Button();
            this.btn_inbox = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.emailList)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // emailList
            // 
            this.emailList.AllowUserToAddRows = false;
            this.emailList.AllowUserToDeleteRows = false;
            this.emailList.AllowUserToResizeColumns = false;
            this.emailList.AllowUserToResizeRows = false;
            this.emailList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.emailList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.emailList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.emailList.ColumnHeadersVisible = false;
            this.emailList.Location = new System.Drawing.Point(12, 51);
            this.emailList.MultiSelect = false;
            this.emailList.Name = "emailList";
            this.emailList.ReadOnly = true;
            this.emailList.RowHeadersVisible = false;
            this.emailList.Size = new System.Drawing.Size(180, 266);
            this.emailList.TabIndex = 0;
            this.emailList.DataSourceChanged += new System.EventHandler(this.emailList_DataSourceChanged);
            this.emailList.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.emailList_CellMouseClick);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.AutoSize = true;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.btn_delete);
            this.panel1.Controls.Add(this.btn_reply);
            this.panel1.Controls.Add(this.label_body);
            this.panel1.Controls.Add(this.label_header);
            this.panel1.Location = new System.Drawing.Point(198, 51);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(753, 329);
            this.panel1.TabIndex = 4;
            // 
            // btn_delete
            // 
            this.btn_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_delete.Location = new System.Drawing.Point(372, 224);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(75, 35);
            this.btn_delete.TabIndex = 8;
            this.btn_delete.Text = "Delete";
            this.btn_delete.UseVisualStyleBackColor = true;
            this.btn_delete.Visible = false;
            this.btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
            // 
            // label_body
            // 
            this.label_body.AutoSize = true;
            this.label_body.Location = new System.Drawing.Point(19, 50);
            this.label_body.Name = "label_body";
            this.label_body.Size = new System.Drawing.Size(37, 13);
            this.label_body.TabIndex = 6;
            this.label_body.Text = "BODY";
            // 
            // label_header
            // 
            this.label_header.AutoSize = true;
            this.label_header.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_header.Location = new System.Drawing.Point(12, 9);
            this.label_header.Name = "label_header";
            this.label_header.Size = new System.Drawing.Size(155, 37);
            this.label_header.TabIndex = 5;
            this.label_header.Text = "HEADER";
            // 
            // btn_new
            // 
            this.btn_new.Location = new System.Drawing.Point(309, 13);
            this.btn_new.Name = "btn_new";
            this.btn_new.Size = new System.Drawing.Size(75, 32);
            this.btn_new.TabIndex = 6;
            this.btn_new.Text = "New Mail";
            this.btn_new.UseVisualStyleBackColor = true;
            this.btn_new.Click += new System.EventHandler(this.btn_new_Click);
            // 
            // btn_sent
            // 
            this.btn_sent.Image = global::SecondTest.Properties.Resources.reply;
            this.btn_sent.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_sent.Location = new System.Drawing.Point(198, 13);
            this.btn_sent.Name = "btn_sent";
            this.btn_sent.Size = new System.Drawing.Size(88, 32);
            this.btn_sent.TabIndex = 5;
            this.btn_sent.Text = "Sent";
            this.btn_sent.UseVisualStyleBackColor = true;
            this.btn_sent.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_reply
            // 
            this.btn_reply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_reply.Image = ((System.Drawing.Image)(resources.GetObject("btn_reply.Image")));
            this.btn_reply.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_reply.Location = new System.Drawing.Point(453, 224);
            this.btn_reply.Name = "btn_reply";
            this.btn_reply.Size = new System.Drawing.Size(75, 35);
            this.btn_reply.TabIndex = 7;
            this.btn_reply.Text = "Reply";
            this.btn_reply.UseVisualStyleBackColor = true;
            this.btn_reply.Visible = false;
            this.btn_reply.Click += new System.EventHandler(this.btn_reply_Click);
            // 
            // Contacts
            // 
            this.Contacts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Contacts.Image = ((System.Drawing.Image)(resources.GetObject("Contacts.Image")));
            this.Contacts.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Contacts.Location = new System.Drawing.Point(653, 13);
            this.Contacts.Name = "Contacts";
            this.Contacts.Size = new System.Drawing.Size(88, 32);
            this.Contacts.TabIndex = 3;
            this.Contacts.Text = "Contacts";
            this.Contacts.UseVisualStyleBackColor = true;
            this.Contacts.Click += new System.EventHandler(this.Contacts_Click);
            // 
            // btn_draft
            // 
            this.btn_draft.Image = global::SecondTest.Properties.Resources.saved;
            this.btn_draft.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_draft.Location = new System.Drawing.Point(104, 13);
            this.btn_draft.Name = "btn_draft";
            this.btn_draft.Size = new System.Drawing.Size(88, 32);
            this.btn_draft.TabIndex = 2;
            this.btn_draft.Text = "Drafts";
            this.btn_draft.UseVisualStyleBackColor = true;
            this.btn_draft.Click += new System.EventHandler(this.btn_draft_Click);
            // 
            // btn_inbox
            // 
            this.btn_inbox.Image = ((System.Drawing.Image)(resources.GetObject("btn_inbox.Image")));
            this.btn_inbox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_inbox.Location = new System.Drawing.Point(13, 13);
            this.btn_inbox.Name = "btn_inbox";
            this.btn_inbox.Size = new System.Drawing.Size(88, 32);
            this.btn_inbox.TabIndex = 1;
            this.btn_inbox.Text = "Inbox";
            this.btn_inbox.UseVisualStyleBackColor = true;
            this.btn_inbox.Click += new System.EventHandler(this.btn_inbox_Click);
            // 
            // SecondTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(753, 329);
            this.Controls.Add(this.btn_new);
            this.Controls.Add(this.btn_sent);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.Contacts);
            this.Controls.Add(this.btn_draft);
            this.Controls.Add(this.btn_inbox);
            this.Controls.Add(this.emailList);
            this.Name = "SecondTestForm";
            this.Text = "SecondTestForm";
            ((System.ComponentModel.ISupportInitialize)(this.emailList)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView emailList;
        private System.Windows.Forms.Button btn_inbox;
        private System.Windows.Forms.Button btn_draft;
        private System.Windows.Forms.Button Contacts;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label_body;
        private System.Windows.Forms.Label label_header;
        private System.Windows.Forms.Button btn_reply;
        private System.Windows.Forms.Button btn_sent;
        private System.Windows.Forms.Button btn_new;
        private System.Windows.Forms.Button btn_delete;
    }
}