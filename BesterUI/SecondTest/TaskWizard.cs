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
using System.Runtime.InteropServices;

namespace SecondTest
{
    public enum Task { None, Final, AddContact, AddAttachment, SendMailToContact, SendDraft, CreateDraft, BogusMessage, RemoveContact }

    public partial class TaskWizard : Form
    {
        public event Action<Task> TaskStarted;
        static TaskWizard Me;


        List<Task> taskOrder;
        int currentTaskIndex = 0;
        UserControl currentTaskPage;

        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;



        private static LowLevelKeyboardProc _proc = HookCallback;

        private static IntPtr _hookID = IntPtr.Zero;

        public TaskWizard()
        {
            Me = this;
            _hookID = SetHook(_proc);
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
                case Task.RemoveContact:
                    return new TaskPage.TaskPage_RemoveContact();
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


        private static IntPtr SetHook(LowLevelKeyboardProc proc)

        {

            using (Process curProcess = Process.GetCurrentProcess())

            using (ProcessModule curModule = curProcess.MainModule)

            {

                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,

                    GetModuleHandle(curModule.ModuleName), 0);

            }

        }


        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);


        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)

            {

                int vkCode = Marshal.ReadInt32(lParam);
                if ((Keys)vkCode == Keys.F1)
                {
                    Me.BtnTaskComplete_Click(Me, null);
                }
                else if ((Keys)vkCode == Keys.F12)
                {
                    Me.BtnTaskIncomplete_Click(Me, null);
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);

        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr SetWindowsHookEx(int idHook,

            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool UnhookWindowsHookEx(IntPtr hhk);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,

            IntPtr wParam, IntPtr lParam);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr GetModuleHandle(string lpModuleName);

    }
}
