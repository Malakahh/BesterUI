using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecondTest
{
    static class SeededProblems
    {
        static event Action<Task> CurrentTaskChanged;

        static Task _currentTask;
        static Task CurrentTask
        {
            get
            {
                return _currentTask;
            }

            set
            {
                _currentTask = value;
                if (CurrentTaskChanged != null)
                {
                    CurrentTaskChanged(_currentTask);
                }
            }
        }

        public static void Init(TaskWizard tw)
        {
            tw.TaskStarted += (task) => CurrentTask = task;
        }
        public static class AttachmentForm
        {
            static int addAttachmentCount = 0;
            public static bool AttachFileBtn()
            {
                if (CurrentTask != Task.AddAttachment)
                    return false;

                if (addAttachmentCount < 3)
                {
                    addAttachmentCount++;
                    EventLog.Write("AddAttachmentButtonClick: " + addAttachmentCount);
                    return true;

                }

                EventLog.Write("AddAttachment complete");
                return false;
            }
        }
        public static class ContactForm
        {
            static int addContactCount = 0;
            public static bool AddContactBtn()
            {
                if (CurrentTask != Task.AddContact)
                {
                    return false;
                }

                if (addContactCount < 3)
                {
                    addContactCount++;
                    EventLog.Write("Add Contact Button click: " + addContactCount);
                    return true;
                }

                EventLog.Write("AddContact complete");
                return false;
            }
        }

        public static class SecondTestForm
        {
            public static bool SendDraft()
            {
                if (CurrentTask != Task.SendDraft)
                {
                    return false;
                }

                EventLog.Write("SendDraft error shown");
                MessageBox.Show("An unknown error has occoured.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
        }

        public static class WriteMessageForm
        {
            static InputLanguage origLang;
            public static bool CreateDraft()
            {
                if (CurrentTask != Task.CreateDraft)
                {
                    return false;
                }

                origLang = InputLanguage.CurrentInputLanguage;
                InputLanguage.CurrentInputLanguage = GetInputLanguageEnglish();
                EventLog.Write("CreateDraft, language changed to: " + InputLanguage.CurrentInputLanguage.LayoutName);
                CurrentTaskChanged += WriteMessageForm_CurrentTaskChanged;
                return false;
            }

            private static InputLanguage GetInputLanguageEnglish()
            {
                InputLanguage l = InputLanguage.InstalledInputLanguages[0];

                for (int i = 0; i < InputLanguage.InstalledInputLanguages.Count; i++)
                {
                    if (InputLanguage.InstalledInputLanguages[i].LayoutName == "US")
                    {
                        l = InputLanguage.InstalledInputLanguages[i];
                        break;
                    }
                }

                return l;
            }

            private static void WriteMessageForm_CurrentTaskChanged(Task obj)
            {
                InputLanguage.CurrentInputLanguage = origLang;
            }

            public static bool BogusMessage()
            {
                if (CurrentTask != Task.BogusMessage)
                {
                    return false;
                }

                EventLog.Write("Text Changed");
                return true;
            }
        }
    }
}
