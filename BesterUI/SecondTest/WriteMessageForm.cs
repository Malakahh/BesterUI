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

        public event Action<Email> EmailSent;
        public event Action<Email> EmailSaved;
        private List<Contact> receivers = new List<Contact>();

        public WriteMessageForm()
        {
            InitializeComponent();
        }

        public WriteMessageForm(Email replyToMail)
        {
            receivers.Add(replyToMail.from);
            textbox_mail_to.Text += replyToMail.from.Email + ";";
            InitializeComponent();
        }

        private void btn_mail_send_Click(object sender, EventArgs e)
        {

            Email mail = new Email(Contact.User, textbox_mail_title.Text, richtext_mail_body.Text, this.receivers);
            if (EmailSent != null)
                EmailSent(mail);

            this.Close();

        }

        private void btn_msg_contacts_Click(object sender, EventArgs e)
        {
            ContactForm cf = new ContactForm();
            cf.ContactSelected += (Contact c) =>
            {
                textbox_mail_to.Text += c.Email + ";";
                receivers.Add(c);
            };
            cf.ShowDialog(this);
        }

        private void btn_mail_save_Click(object sender, EventArgs e)
        {
            Email mail = new Email(Contact.User, textbox_mail_title.Text, richtext_mail_body.Text, this.receivers);
            if (EmailSaved != null)
                EmailSaved(mail);

            this.Close();
        }
    }
}
