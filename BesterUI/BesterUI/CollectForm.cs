using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BesterUI.Data;

namespace BesterUI
{
    public partial class CollectForm : Form
    {
        public CollectForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            EEGDataReading test = new EEGDataReading();
            test.data.Add(EEGDataReading.ELECTRODE.A1, 1.1);
            test.data.Add(EEGDataReading.ELECTRODE.A2, 2.1);
            test.Write();
            EEGDataReading test2 = new EEGDataReading();
            test2.data.Add(EEGDataReading.ELECTRODE.A1, 1.2);
            test2.data.Add(EEGDataReading.ELECTRODE.A2, 2.2);
            test2.Write();
            test2.EndWrite();

        }
    }
}
