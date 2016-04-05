using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace SecondTest
{
    public partial class AttachmentUploadForm : Form
    {
        public AttachmentUploadForm()
        {
            InitializeComponent();

            //Disable resizing
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.ControlBox = false;

            DoWait();
        }
        private async void DoWait()
        {
            button1.Text = "Waiting...";
            button1.Enabled = false;
            label1.Show();
            pictureBox1.Show();
            button1.Enabled = await EnableButton();
        }

        private async Task<bool> EnableButton()
        {
            await Task<bool>.Delay(2000);

            label1.Hide();
            pictureBox1.Hide();

            if (SeededProblems.AttachmentForm.AttachFileBtn())
            {
                label2.Text = "File could not be uploaded. \nPlease try again.";
                label2.ForeColor = Color.Red;
                label2.Show();
                button1.Text = "Retry";
                return true;
            }
            else
            {
                label2.Text = "File attached successfully.";
                label2.ForeColor = Color.Green;
                label2.Show();
                button1.Text = "Ok";
                return true;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Ok")
                this.Close();
            else
            {
                label2.Hide();
                DoWait();
            }
        }
    }
}
