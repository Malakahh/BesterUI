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
    public enum Task { None, Test, Test2 }

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
                case Task.Test:
                    return new TaskPage.TaskPage_Test();
                    break;
                case Task.Test2:
                    return new TaskPage.TaskPage_Test();
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
