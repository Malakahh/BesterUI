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

        public static class ContactForm
        {
            public static bool AddContactBtn()
            {
                if (CurrentTask != Task.AddContact)
                {
                    return false;
                }

                throw new NotImplementedException();
            }
        }
    }
}
