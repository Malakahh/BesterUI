using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BesterUI.Helpers
{
    static class Log
    {
        static RichTextBox logBox;

        public static RichTextBox LogBox
        {
            get
            {
                return logBox;
            }

            set
            {
                logBox = value;
                logBox.Text = "[" + DateTime.Now.ToString("HH:mm:ss") + "] LogBox initialized!";
            }
        }

        public static void LogMessage(object text)
        {
            if (logBox != null)
            {
                LogBox.Invoke((Action<object>)AddLogMessage, text);
            }
            else
            {
                MessageBox.Show("Sorry, I have no where to put the log messages (set Log.LogBox).");
            }
        }

        public static void LogMessageSameLine(object text)
        {
            if (logBox != null)
            {
                LogBox.Invoke((Action<object>)AddLogMessageSameLine, text);
            }
            else
            {
                MessageBox.Show("Sorry, I have no where to put the log messages (set Log.LogBox).");
            }
        }

        static void AddLogMessage(object text)
        {
            logBox.AppendText("\n[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text.ToString());
            logBox.ScrollToCaret();
        }

        static void AddLogMessageSameLine(object text)
        {
            int idx = logBox.Text.LastIndexOf('\n');
            if (idx > -1)
            {
                logBox.Text = logBox.Text.Remove(idx) + "\n";
            }
            logBox.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text.ToString());
        }

    }
}
