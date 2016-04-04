using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondTest
{
    static class SeededProblems
    {
        static Task CurrentTask;

        public static void Init(TaskWizard tw)
        {
            tw.TaskStarted += (task) => CurrentTask = task;
        }
        public static class AttachmentForm
        {
            public static int addAttachmentCount = 0;
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
                else
                {
                    return false;
                }
            }
        }
    }
}
