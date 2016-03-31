using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecondTest
{
    public partial class WriteMessageForm : Form
    {

        Action<Email> sendEmail;

        public WriteMessageForm()
        {
            InitializeComponent();
        }

        private void btn_mail_send_Click(object sender, EventArgs e)
        {
            Email mail = new Email("Me", textbox_mail_title.Text, richtext_mail_body.Text, textbox_mail_to.Text);

            if (sendEmail != null)
                sendEmail(mail);

        }

    }
}
