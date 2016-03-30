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
    public enum Task { None}

    public partial class TaskWizard : Form
    {
        public event Action<Task> TaskStarted;

        Task[] taskOrder;
        int currentTaskIndex = 0;
        UserControl currentTaskPage;

        public TaskWizard()
        {
            InitializeComponent();
            btnTaskComplete.Click += BtnTaskComplete_Click;
            btnTaskIncomplete.Click += BtnTaskIncomplete_Click;

            taskOrder = Enum.GetValues(typeof(Task)).Cast<Task>().ToArray();
        }

        private void DisableBtns()
        {
            btnTaskComplete.Enabled = false;
            btnTaskIncomplete.Enabled = false;
        }

        private void NextTask()
        {
            currentTaskPage = GetTaskPage(taskOrder[currentTaskIndex]);
            currentTaskPage.Show();

            if (TaskStarted != null)
            {
                TaskStarted(taskOrder[currentTaskIndex]);
            }

            currentTaskIndex++;
            if (currentTaskIndex >= taskOrder.Length)
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
