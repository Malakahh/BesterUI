using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SecondTest;
using BesterUI.Data;

namespace BesterUI
{
    public partial class RestingForm : Form
    {
        public RestingForm()
        {
            InitializeComponent();
            this.FormClosing += RestingForm_FormClosing;
            DoWait();
        }

        private async void DoWait()
        {
            await Task<bool>.Delay(180000);
            this.Close();
        }

        private void RestingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SecondTestForm secondTest = new SecondTestForm(DataReading.stopWatch, DataReading.startTime, DataReading.dateFormat);

            secondTest.Show();
        }
    }
}
