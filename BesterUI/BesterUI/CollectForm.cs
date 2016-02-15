using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BesterUI
{
    public partial class CollectForm : Form
    {
        FusionData fusionData = new FusionData();

        public CollectForm()
        {
            InitializeComponent();
        }

        private void dummyDataBtn_Click(object sender, EventArgs e)
        {
            fusionData.CreateDummyData();
        }

        private void loadFromFileBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog fb = new OpenFileDialog();
            fb.Multiselect = true;
            DialogResult res = fb.ShowDialog();

            if (res == DialogResult.OK)
            {
                fusionData.LoadFromFile(fb.FileNames);
            }
        }
    }
}
