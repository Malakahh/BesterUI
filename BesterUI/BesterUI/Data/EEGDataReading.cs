using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Script.Serialization;

namespace BesterUI.Data
{
    public class EEGDataReading : DataReading
    {
        public Dictionary<string, double> data = new Dictionary<string, double>();

        public enum ELECTRODE
        {
            //A1,
            //F9, FT9, T9, TP9, P9,
            /*AF7,*/
            F7, /*FT7,*/ T7, /*TP7,*/ P7, /*PO7,*/
                                          /*F5,*/
            FC5, /*C5, CP5, P5,*/
            AF3, F3, /*FC3, C3, CP3, P3, PO3,*/
                     /*FP1, F1, FC1, C1, CP1, P1,*/
            O1,
            /*NZ, FPZ, AFZ, FZ, FCZ, CZ, CPZ, PZ, POZ, OZ, IZ,*/
            /*FP2, F2, FC2, C2, CP2, P2,*/
            O2,
            AF4, F4, /*FC4, C4, CP4, P4, PO4,*/
                     /*AF6,*/
            FC6, /*C6, CP6, P6,*/
                 /*AF8,*/
            F8, /*FT8,*/ T8, /*TP8,*/ P8,
            /*F10, FT10, T10, TP10, P10,*/
            //A2
        }

        public EEGDataReading(bool startReading) : base(startReading)
        {

        }

        public EEGDataReading() : this(false)
        { }

        public override void Write()
        {
            DataReading.StaticWrite("EEG", this);
        }

        public override string Serialize()
        {
            string retVal = "";
            foreach (var item in data)
            {
                retVal += item.Key + ":" + item.Value + "|";
            }

            return retVal;
        }

        protected override DataReading Deserialize(string line)
        {
            data.Clear();

            var bits = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in bits)
            {
                var dat = item.Split(':');
                 data.Add(dat[0], double.Parse(dat[1].Replace(',','.'), System.Globalization.CultureInfo.InvariantCulture));
            }

            return this;
        }
    }
}
