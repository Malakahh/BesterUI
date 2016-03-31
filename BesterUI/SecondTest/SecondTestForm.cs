using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace SecondTest
{
    public partial class SecondTestForm : Form
    {

        private List<Email> mails = new List<Email>();
        private List<Email> drafts = new List<Email>();
        private List<Email> sentBox = new List<Email>();

        public SecondTestForm(Stopwatch timer, DateTime? startTime, string dateTimeFormat)
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;

            EventLog.Timer = timer;
            EventLog.StartTime = startTime;
            EventLog.DateTimeFormat = dateTimeFormat;

            this.FormClosing += SecondTestForm_FormClosing;

            Contact.GenerateDefaultContacts();
            TaskWizard taskWizard = new TaskWizard();
            SeededProblems.Init(taskWizard);
            taskWizard.Show();

            MakeEmails();
            LoadEmails();
        }


        private void MakeEmails()
        {

            btn_inbox.Text += " (" + mails.Count + ")";
            btn_draft.Text += " (" + drafts.Count + ")";
            btn_sent.Text += " (" + sentBox.Count + ")";
        }

        private void LoadEmails()
        {
            emailList.RowTemplate.Height = 60;
            emailList.DataSource = mails;
            emailList.Invalidate();
            emailList.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            emailList.Columns[0].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }


        private void SecondTestForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            EventLog.CloseWriter();
        }

        private void btn_inbox_Click(object sender, EventArgs e)
        {
            ChangeMailSource("inbox");
        }

        private void btn_draft_Click(object sender, EventArgs e)
        {
            ChangeMailSource("drafts");
        }

        private void ChangeMailSource(string s)
        {
            List<Email> source = new List<Email>();
            if (s == "inbox")
                source = mails;
            else if (s == "drafts")
                source = drafts;
            else if (s == "sentBox")
                source = sentBox;

            emailList.DataSource = source;
            if (emailList.Rows.Count > 0)
            {
                emailList.ClearSelection();
                emailList.Rows[0].Selected = true;
                SetShownMail(source.First());
                emailList.Invalidate();

            }
        }


        private void SetShownMail(Email mail)
        {
            label_header.Text = mail.from + " - " + mail.title;
            label_body.Text = mail.body;
        }

        Email currentMail;
        private void emailList_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (emailList.DataSource == mails)
            {
                currentMail = mails[e.RowIndex];
            }
            else if (emailList.DataSource == drafts)
            {
                currentMail = drafts[e.RowIndex];
            }
            else if (emailList.DataSource == sentBox)
            {
                currentMail = sentBox[e.RowIndex];
            }


            SetShownMail(currentMail);
        }

        private void Contacts_Click(object sender, EventArgs e)
        {
            ContactForm c = new ContactForm();
            c.ShowDialog(this);
        }

        private void btn_reply_Click(object sender, EventArgs e)
        {
            ComposeEmail(currentMail);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ChangeMailSource("sentBox");
        }

        private void btn_new_Click(object sender, EventArgs e)
        {
            ComposeEmail();
        }

        private void ComposeEmail()
        {
            WriteMessageForm wmf = new WriteMessageForm();
            wmf.EmailSent += (Email mail) => { this.sentBox.Add(mail); btn_sent.Text += " (" + this.sentBox.Count + ")"; };
            wmf.EmailSaved += (Email mail) => { this.drafts.Add(mail); btn_draft.Text += " (" + this.drafts.Count + ")"; };

            wmf.ShowDialog(this);
        }

        private void ComposeEmail(Email replyTo)
        {

        }
    }
}
