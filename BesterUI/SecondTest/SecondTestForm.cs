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

        public SecondTestForm(Stopwatch timer, DateTime? startTime, string dateTimeFormat)
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;

            EventLog.Timer = timer;
            EventLog.StartTime = startTime;
            EventLog.DateTimeFormat = dateTimeFormat;

            this.FormClosing += SecondTestForm_FormClosing;

            TaskWizard taskWizard = new TaskWizard();
            taskWizard.Show();

            MakeEmails();
            LoadEmails();
        }


        List<Email> mails = new List<Email>();
        private void MakeEmails()
        {
            mails.Add(new Email("EnLargeMe.com", "New and improved penis enlargement pill - BUY NOW FOR CHEAPSIES!", "This body"));
            mails.Add(new Email("AAU", "You have been selected for an extra exam", "This Body"));
            mails.Add(new Email("My Bestie", "Hey are you comming over tonight for dinner?", "This Body"));
            mails.Add(new Email("Microsoft", "New email client for windows users!", "The body"));
            mails.Add(new Email("Tinkov Bank", "We like u join to our bankings operationalities", "The Body"));
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

        private void emailList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            Email mail = mails[e.RowIndex];
            label_header.Text = mail.from + " - " + mail.title;
            label_body.Text = mail.body;
        }

        private void Contacts_Click(object sender, EventArgs e)
        {
            Contacts c = new SecondTest.Contacts();
            c.Show();
        }
    }
}
