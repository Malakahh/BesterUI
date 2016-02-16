﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BesterUI.Data;
using Microsoft.Kinect.Face;
using System.Windows.Forms;
using System.IO;

namespace BesterUI
{
    public class FusionData
    {
        public List<BandDataReading> bandData = new List<BandDataReading>();
        public List<EEGDataReading> eegData = new List<EEGDataReading>();
        public List<GSRDataReading> gsrData = new List<GSRDataReading>();
        public List<KinectDataReading> kinectData = new List<KinectDataReading>();

        public FusionData()
        {

        }

        public void LoadFromFile(string[] fileNames)
        {
            DialogResult res = DialogResult.OK;

            if (bandData.Count != 0 || eegData.Count != 0 || gsrData.Count != 0 || kinectData.Count != 0)
            {
                res = MessageBox.Show("You are about to overwrite unsaved data. Continue?", "WARNING", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            }

            if (res == DialogResult.OK)
            {
                string[] correctNames = new string[4] { "GSR.json", "EEG.json", "Band.json", "Kinect.json" };

                foreach (string file in fileNames)
                {
                    string s = file.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).Last();
                    if (!correctNames.Contains(s))
                    {
                        continue;
                    }

                    string json = File.ReadAllText(file);

                    switch(s)
                    {
                        case "GSR.json":
                            gsrData = GSRDataReading.LoadFromFile(json);
                            break;
                        case "EEG.json":
                            eegData = EEGDataReading.LoadFromFile(json);
                            break;
                        case "Band.json":
                            bandData = BandDataReading.LoadFromFile(json);
                            break;
                        case "Kinect.json":
                            kinectData = KinectDataReading.LoadFromFile(json);
                            break;
                    }
                }
            }
        }

        public void CreateDummyData()
        {
            //EEG
            EEGDataReading test = new EEGDataReading();
            test.data.Add(EEGDataReading.ELECTRODE.AF3.GetName(), 1.1);
            test.data.Add(EEGDataReading.ELECTRODE.AF4.GetName(), 2.1);
            test.Write();
            eegData.Add(test);
            
            EEGDataReading test2 = new EEGDataReading();
            test2.data.Add(EEGDataReading.ELECTRODE.AF3.GetName(), 1.2);
            test2.data.Add(EEGDataReading.ELECTRODE.AF4.GetName(), 2.2);
            test2.Write();
            eegData.Add(test2);
            
            EEGDataReading test3 = new EEGDataReading();
            test3.data.Add(EEGDataReading.ELECTRODE.AF3.GetName(), 13337.0);
            test3.Write();
            eegData.Add(test3);
            test3.EndWrite();

            //GSR
            for (int i = 0; i < 200; i++)
            {
                GSRDataReading gsr = new GSRDataReading();
                gsr.resistance = 4;
                gsr.Write();
                gsrData.Add(gsr);
            }

            GSRDataReading gsr6 = new GSRDataReading();
            gsr6.resistance = 66666;
            gsr6.Write();
            gsrData.Add(gsr6);
            gsr6.EndWrite();


            //Band
            for (int i = 0; i < 200; i++)
            {
                BandDataReading band = new BandDataReading();
                band.heartRate = 5;
                band.quality = BandDataReading.QUALITY.ACQUIRING.GetName();
                band.Write();
                bandData.Add(band);
            }

            BandDataReading band1 = new BandDataReading();
            band1.heartRate = 1337;
            band1.quality = BandDataReading.QUALITY.LOCKED.GetName();
            band1.Write();
            bandData.Add(band1);
            band1.EndWrite();


            //Kinect
            KinectDataReading k = new KinectDataReading();
            k.data.Add(FaceShapeAnimations.JawSlideRight.GetName(), 0.5);
            k.data.Add(FaceShapeAnimations.LeftcheekPuff.GetName(), 0.5);
            k.Write();
            kinectData.Add(k);

            KinectDataReading k2 = new KinectDataReading();
            k2.data.Add(FaceShapeAnimations.JawSlideRight.GetName(), 0.6);
            k2.data.Add(FaceShapeAnimations.LeftcheekPuff.GetName(), 0.7);
            k2.Write();
            kinectData.Add(k2);
            k2.EndWrite();
        }
    }
}
