namespace SecondTest
{
    partial class Attachments
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
            this.components = new System.ComponentModel.Container();
            this.btn_attach_image = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panelPictures = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // btn_attach_image
            // 
            this.btn_attach_image.Location = new System.Drawing.Point(608, 237);
            this.btn_attach_image.Name = "btn_attach_image";
            this.btn_attach_image.Size = new System.Drawing.Size(75, 23);
            this.btn_attach_image.TabIndex = 1;
            this.btn_attach_image.Text = "Attach";
            this.btn_attach_image.UseVisualStyleBackColor = true;
            this.btn_attach_image.Click += new System.EventHandler(this.btn_attach_image_Click);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // panelPictures
            // 
            this.panelPictures.Location = new System.Drawing.Point(12, 12);
            this.panelPictures.Name = "panelPictures";
            this.panelPictures.Size = new System.Drawing.Size(671, 219);
            this.panelPictures.TabIndex = 2;
            // 
            // Attachments
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(701, 265);
            this.Controls.Add(this.panelPictures);
            this.Controls.Add(this.btn_attach_image);
            this.Name = "Attachments";
            this.Text = "Attachments";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btn_attach_image;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.FlowLayoutPanel panelPictures;
    }
}