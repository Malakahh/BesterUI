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
        List<Email> source;

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

            EventLog.Write("Resting period over");

            var myScreen = Screen.FromControl(this);
            //var otherScreen = Screen.AllScreens.FirstOrDefault(s => !s.Equals(myScreen))
            //               ?? myScreen;

            taskWizard.StartPosition = FormStartPosition.Manual;
            //taskWizard.Left = myScreen.WorkingArea.Left + 120;
            //taskWizard.Top = myScreen.WorkingArea.Top + 120;

            taskWizard.Left = myScreen.Bounds.Right - taskWizard.Width;
            taskWizard.Top = myScreen.Bounds.Bottom - taskWizard.Height;

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

            mails.Add(new Email(
                Contact.Contacts.Find(x => x.Email == "support@chrononsystems.com"),
                "Big Dripstat Update",
                "Scala Support! \n We have finally added support for Scala, Play and Spray Frameworks! We specifically designed our agent to capture all the asynchronous activity that using these frameworks entails. \n \n Details for: \n 1. Play framework Support \n 2. Spray Framework Support \n \n Follow us on twitter for the latest news and status updates: twitter.com/dripstat \n\n Keep Dripping,\n Team Dripstat",
                new List<Contact>() { Contact.User }
                ));

            mails.Add(new Email(
                Contact.Contacts.Find(x => x.Email == "news@todoist.com"),
                "Todoist for windows 10",
                "Hi,\n Since launching the Todoist for Windows 10 preview last November, over 12, 000 passionate beta testers have lent a hand in helping us craft the best task manager for modern Windows devices.\n We’ve leveraged the most unique features of Windows 10 for a truly customizable and easy - to - use experience that’s everywhere you need it.\n Now with:\n \n A faster, smarter Quick Add that automatically recognizes Projects and Labels \n Jump List integration for easy task access throughout the day \n Offline support and automatic sync when reconnecting online \n \n And tons of other productivity enhancing goodies! \n To celebrate we’re giving away a brand new Windows device!\n\n Best Regards: \n The Todoist Team",
                new List<Contact>() { Contact.User }
                ));

            mails.Add(new Email(
                Contact.Contacts.Find(x => x.Email == "noreply@coursera.com"),
                "We have recommendations for you",
                "We combed our catalog and found courses and Specializations that we think match your interests. See our recommendations below, and start learning something new today.\n\n Datastructures and Algorithms\n Interaction Design\n Game Design: Arts and Concepts\n Java Programming: An introduction to software\n \n\n Other courses you might be interested in outside your field could be\n\n Internet History, technology and security\n Cybersecurity and its ten domains\n Securing digital democracy\n Model thinking\n \n\n Please do not reply to this mail.",
                new List<Contact>() { Contact.User }
                ));

            mails.Add(new Email(
                Contact.Contacts.Find(x => x.Email == "thomas@usersnap.com"),
                "Thomas from Usersnap",
                "Hi there,\n\n a while ago I asked you to contributed to a survey about the state of client collaboration. Nearly 1,000 agencies, web design & development companies participated in the survey. Thank you!\n\n Today, we published the final survey report. And its findings are surprising on some counts and encouraging on others.\n\n We encourage you to share these results with your colleagues and social networks (tweet now) - and we look forward to next year’s survey and hearing about progress you make and new concepts you implement.\n\n Check out the full survey report here.\n\n Happy reading!\n Thomas from Usersnap",
                new List<Contact>() { Contact.User }
                ));
            mails.Add(new Email(
                Contact.Contacts.Find(x => x.Email == "noreply@coursera.com"),
                "How can you learn more?",
                "Earn your Certificate with these five simple steps\n\n Whether you’re new to online learning or a veteran of hundreds of courses, these motivation and time management strategies can help make your next course your most successful yet.\n\n 1\n Know your goals.\n Do you want to get a promotion, start a business, or write a song? Write down your reasons for learning, and post a copy near your computer.\n\n Master the art of goal-setting:\n • Career Brand Management\n The State University of New York\n\n • Career Success\n University of California, Irvine Extension\n\n • From Idea to Startup\n Technion – Israel Institute of Technology\n\n\n 2\n Establish accountability\n Ask a friend to check in on your learning progress - or even better, recruit a group of friends to take a course with you.\n\n Become your own coach\n • Coaching Skills for Managers\n University of California, Davis\n\n • Leading People and Teams\n University of Michigan\n\n • Human Resource Management: HR for People Managers\n University of Minnesota\n\n\n 3\n Learn on the go\n Download the Coursera App for Android or iOS, save videos for offline viewing, and fit learning into your commute, coffee break, or other quiet moments in your day.\n\n Learn more about mobile\n • Android App Development\n Vanderbilt University\n\n • iOS App Development with Swift\n University of Toronto\n\n • iOS Development for Creative Entrepreneurs\n University of California, Irvine Extension\n",
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
            source = mails;
            currentMail = mails.First();
        }

        private void UpdateLabels()
        {
            RefreshMailSource();
            btn_inbox.Text = "Inbox (" + mails.Count + ")";
            btn_draft.Text = "Drafts (" + drafts.Count + ")";
            if (drafts.Count == 0)
                btn_reply.Hide();
            else
                btn_reply.Show();

            btn_sent.Text = "Sent (" + sentBox.Count + ")";
            if (sentBox.Count == 0)
                btn_reply.Hide();
            else
                btn_reply.Show();
        }

        private void RefreshMailSource()
        {
            var s = emailList.DataSource;
            emailList.DataSource = null;
            emailList.DataSource = s;
        }

        private void LoadEmails()
        {
            emailList.RowTemplate.Height = 60;
            emailList.DataSource = mails;
            emailList.Invalidate();
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
            if (s == "inbox")
            {
                source = mails;
                btn_reply.Text = "Reply";
            }
            else if (s == "drafts")
            {
                source = drafts;
                btn_reply.Text = "Edit";
            }
            else if (s == "sentBox")
            {
                source = sentBox;
                btn_reply.Text = "Reply";
            }
            UpdateLabels();
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
            currentMail = mail;
            btn_reply.Visible = true;
            btn_delete.Visible = true;

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
            if (btn_reply.Text == "Edit" && SeededProblems.SecondTestForm.SendDraft())
            {
                return;
            }

            ComposeEmail(currentMail, btn_reply.Text == "Edit");
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

        private void ComposeEmail(Email replyTo, bool copyBody)
        {
            WriteMessageForm wmf = new WriteMessageForm(replyTo, copyBody);
            wmf.EmailSent += (Email mail) => { this.sentBox.Add(mail); UpdateLabels(); };
            wmf.EmailSaved += (Email mail) => { this.drafts.Add(mail); UpdateLabels(); };


            wmf.ShowDialog(this);
        }

        private void emailList_DataSourceChanged(object sender, EventArgs e)
        {
            emailList.Invalidate();
            currentMail = null;

            if (emailList.DataSource == mails && mails.Count > 0)
            {
                currentMail = mails[0];
            }
            else if (emailList.DataSource == drafts && drafts.Count > 0)
            {
                currentMail = drafts[0];
            }
            else if (emailList.DataSource == sentBox && sentBox.Count > 0)
            {
                currentMail = sentBox[0];
            }

            if (currentMail == null)
            {
                SetShownMail(Email.Empty);
            }
            else
            {
                SetShownMail(currentMail);
            }
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            source.Remove(currentMail);
            UpdateLabels();
        }
    }
}
