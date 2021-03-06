﻿using System;
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
        public static Task CurrentTask
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

                EventLog.Write("Task: AddAttachment");

                if (addAttachmentCount < 3)
                {
                    addAttachmentCount++;
                    EventLog.Write("AddAttachmentButtonClick: " + addAttachmentCount);
                    return true;

                }

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

                EventLog.Write("Task: AddContact");

                if (addContactCount < 3)
                {
                    addContactCount++;
                    EventLog.Write("Add Contact Button click: " + addContactCount);
                    return true;
                }

                return false;
            }

            static bool contactRemoved = false;
            public static bool RemoveContactBtn(bool b)
            {
                if (CurrentTask != Task.RemoveContact)
                {
                    return false;
                }

                EventLog.Write("Task: RemoveContact");

                if (!b && !contactRemoved)
                    return false;

                if (!contactRemoved)
                {
                    EventLog.Write("RemoveContact clicked");
                    contactRemoved = true;
                    return true;
                }

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

                EventLog.Write("Task: SendDraft");

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

                EventLog.Write("Task: CreateDraft");

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
                    if (InputLanguage.InstalledInputLanguages[i].LayoutName == "US" || InputLanguage.InstalledInputLanguages[i].LayoutName == "Amerikansk")
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

            private static bool bogusMessagefirst = true;
            public static bool BogusMessage()
            {
                if (CurrentTask != Task.BogusMessage)
                {
                    return false;
                }

                if (bogusMessagefirst)
                {
                    EventLog.Write("Task: BogusMessage");
                    bogusMessagefirst = false;
                }

                EventLog.Write("BogusMessage: Text Changed");
                return true;
            }

            public static bool NotResponding()
            {
                if (CurrentTask != Task.NotResponding)
                {
                    return false;
                }

                EventLog.Write("Task: NotResponding");
                return true;
            }
        }
    }
}
