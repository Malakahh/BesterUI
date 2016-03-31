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

            mails.Add(new Email(
                Contact.Contacts.Find(x => x.Email == "julia@jubii.dk"),
                "Dinner on saturday",
                "Hi" + Environment.NewLine + "I would just like to say that we look forward to see you on saturday :)",
                new List<Contact>() { Contact.User }
                ));

            mails.Add(new Email(
                Contact.Contacts.Find(x => x.Email == "ntyles@tyles.com"),
                "Lets go out for a drink!",
                "Hey you! Lets go for a drink soon, it would be totally awesome!",
                new List<Contact>() { Contact.User }
                ));

            mails.Add(new Email(
                Contact.Contacts.Find(x => x.Email == "help@microsoft.com"),
                "The new Windows is out!",
                "Enjoy the new windows which has more features than ever. We have redesigned the start menu, added additional features for gamers and in house media centers. Become faster at your work with the new office 365 apps as well!",
                new List<Contact>() { Contact.User }
                ));

            drafts.Add(new Email(
                Contact.User,
                "Re: Dinner on saturday",
                "Hi Mom! I look forwa",
                new List<Contact>() { Contact.noContactYet }
                ));

            UpdateLabels();
            SetShownMail(mails.First());

        }

        private void UpdateLabels()
        {
            btn_inbox.Text = "Inbox (" + mails.Count + ")";
            btn_draft.Text = "Drafts (" + drafts.Count + ")";
            btn_sent.Text = "Sent (" + sentBox.Count + ")";
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
            label_header.Text = mail.from.FirstName + " " + mail.from.LastName + "(" + mail.from.Email + ")" + " - " + mail.title;
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
            wmf.EmailSent += (Email mail) => { this.sentBox.Add(mail); UpdateLabels(); };
            wmf.EmailSaved += (Email mail) => { this.drafts.Add(mail); UpdateLabels(); };

            wmf.ShowDialog(this);
        }

        private void ComposeEmail(Email replyTo)
        {
            WriteMessageForm wmf = new WriteMessageForm(replyTo);
            wmf.EmailSent += (Email mail) => { this.sentBox.Add(mail); UpdateLabels(); };
            wmf.EmailSaved += (Email mail) => { this.drafts.Add(mail); UpdateLabels(); };


            wmf.ShowDialog(this);
        }
    }
}
