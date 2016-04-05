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
    public enum Task { None, Final, AddContact, AddAttachment, SendMailToContact, SendDraft, CreateDraft, BogusMessage }

    public partial class TaskWizard : Form
    {
        public event Action<Task> TaskStarted;

        List<Task> taskOrder;
        int currentTaskIndex = 0;
        UserControl currentTaskPage;

        public TaskWizard()
        {
            InitializeComponent();
            btnTaskComplete.Click += BtnTaskComplete_Click;
            btnTaskIncomplete.Click += BtnTaskIncomplete_Click;

            this.ControlBox = false;

            //Disable resizing
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            taskOrder = GetScrambledTaskOrder();

            currentTaskPage = GetTaskPage(Task.None);
            this.Controls.Add(currentTaskPage);
            currentTaskPage.Show();
        }

        private List<Task> GetScrambledTaskOrder()
        {
            List<Task> list = new List<Task>();
            list = Enum.GetValues(typeof(Task)).Cast<Task>().ToList();

            list.Remove(Task.None);
            list.Remove(Task.Final);

            //Scramble
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Task val = list[k];
                list[k] = list[n];
                list[n] = val;
            }

            list.Add(Task.Final);

            return list;
        }

        private void DisableBtns()
        {
            btnTaskComplete.Enabled = false;
            btnTaskIncomplete.Enabled = false;
        }

        private void NextTask()
        {
            this.Controls.Remove(currentTaskPage);
            currentTaskPage = GetTaskPage(taskOrder[currentTaskIndex]);
            this.Controls.Add(currentTaskPage);
            currentTaskPage.Show();

            if (TaskStarted != null)
            {
                TaskStarted(taskOrder[currentTaskIndex]);
            }

            currentTaskIndex++;
            if (currentTaskIndex >= taskOrder.Count)
            {
                DisableBtns();
            }
        }

        private UserControl GetTaskPage(Task t)
        {
            switch (t)
            {
                default:
                case Task.None:
                    return new TaskPage.TaskPage_None();
                    break;
                case Task.Final:
                    return new TaskPage.TaskPage_Final();
                    break;
                case Task.AddContact:
                    return new TaskPage.TaskPage_AddContact();
                    break;
                case Task.SendMailToContact:
                    return new TaskPage.TaskPage_SendMailToContact();
                    break;
                case Task.AddAttachment:
                    return new TaskPage.TaskPage_AddAttachment();
                    break;
                case Task.SendDraft:
                    return new TaskPage.TaskPage_SendDraft();
                    break;
                case Task.CreateDraft:
                    return new TaskPage.TaskPage_CreateDraft();
                    break;
                case Task.BogusMessage:
                    return new TaskPage.TaskPage_BogusMessage();
                    break;
            }
        }

        private void BtnTaskComplete_Click(object sender, EventArgs e)
        {
            EventLog.Write("TaskWizard - BtnCompleteClicked");
            NextTask();
        }

        private void BtnTaskIncomplete_Click(object sender, EventArgs e)
        {
            EventLog.Write("TaskWizard - BtnIncompleteClicked");
            NextTask();
        }

    }
}
