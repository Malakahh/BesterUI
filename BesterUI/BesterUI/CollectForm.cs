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
using Microsoft.Kinect.Face;

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
            test.data.Add(EEGDataReading.ELECTRODE.A1.GetName(), 1.1);
            test.data.Add(EEGDataReading.ELECTRODE.A2.GetName(), 2.1);
            test.Write();

            EEGDataReading test2 = new EEGDataReading();
            test2.data.Add(EEGDataReading.ELECTRODE.A1.GetName(), 1.2);
            test2.data.Add(EEGDataReading.ELECTRODE.A2.GetName(), 2.2);
            test2.Write();
           

            EEGDataReading test3 = new EEGDataReading();
            test3.data.Add(EEGDataReading.ELECTRODE.AF3.GetName(), 13337.0);
            test3.Write();
            test3.EndWrite();


            for (int i = 0; i < 200; i++)
            {
                GSRDataReading gsr = new GSRDataReading();
                gsr.resistance = 4;
                gsr.Write();
            }

            GSRDataReading gsr6 = new GSRDataReading();
            gsr6.resistance = 66666;
            gsr6.Write();
            gsr6.EndWrite();


            for (int i = 0; i < 200; i++)
            {
                BandDataReading band = new BandDataReading();
                band.heartRate = 5;
                band.quality = BandDataReading.QUALITY.ACQUIRING.GetName();
                band.Write();
            }

            BandDataReading band1 = new BandDataReading();
            band1.heartRate = 1337;
            band1.quality = BandDataReading.QUALITY.LOCKED.GetName();
            band1.Write();
            band1.EndWrite();


            KinectDataReading k = new KinectDataReading();
            k.data.Add(FaceShapeAnimations.JawSlideRight.GetName(), 0.5);
            k.data.Add(FaceShapeAnimations.LeftcheekPuff.GetName(), 0.5);
            k.Write();

            KinectDataReading k2 = new KinectDataReading();
            k2.data.Add(FaceShapeAnimations.JawSlideRight.GetName(), 0.6);
            k2.data.Add(FaceShapeAnimations.LeftcheekPuff.GetName(), 0.7);
            k2.Write();
            k2.EndWrite();
        }
    }
}
