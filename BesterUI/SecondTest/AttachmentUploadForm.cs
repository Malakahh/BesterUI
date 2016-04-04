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
            button1.Enabled = false;
            DoWait();
        }
        private async void DoWait()
        {
            button1.Enabled = await EnableButton();
        }

        private async Task<bool> EnableButton()
        {
            await Task<bool>.Delay(2000);
            label1.Hide();
            pictureBox1.Hide();
            if (SeededProblems.AttachmentForm.addAttachmentCount < 3)
            {
                label2.Text = "File attached successfully.";
                label2.ForeColor = Color.Green;
            }

            label2.Show();
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
