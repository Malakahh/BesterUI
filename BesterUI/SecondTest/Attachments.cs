using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecondTest
{
    public partial class Attachments : Form
    {
        public Action<string> ImageAttached;

        public Attachments()
        {
            InitializeComponent();
            PopulateAttachments();

            //Disable resizing
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }


        string selectedPicture;
        private void PopulateAttachments()
        {
            DirectoryInfo directory = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory + "Resources/");
            FileInfo[] files = directory.GetFiles("*.jpg");
            List<PictureBox> images = new List<PictureBox>();
            foreach (FileInfo f in files)
            {
                PictureBox tmpPictureBox = new PictureBox();
                tmpPictureBox.Image = Image.FromFile(f.FullName);
                tmpPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                tmpPictureBox.Size = new Size(100, 100);
                tmpPictureBox.Parent = panelPictures;
                tmpPictureBox.Click += (s, e) =>
                {
                    btn_attach_image.Text = "Attach";
                    foreach (PictureBox p in images)
                        p.BorderStyle = BorderStyle.None;

                    selectedPicture = f.Name;
                    tmpPictureBox.BorderStyle = BorderStyle.Fixed3D;
                };
                images.Add(tmpPictureBox);
            }


            panelPictures.Invalidate();

            //pictureBox1.Image = imageList1.Images[0];
        }

        private void btn_attach_image_Click(object sender, EventArgs e)
        {

            if (selectedPicture != null)
            {
                AttachmentUploadForm auf = new AttachmentUploadForm();
                auf.ShowDialog(this);
                if (SeededProblems.AttachmentForm.AttachFileBtn())
                {
                    return;
                }
                else
                {
                    if (ImageAttached != null)
                        ImageAttached(selectedPicture);

                    this.Close();

                }
            }
            else
            {
                this.Close();
            }
        }
    }
}
