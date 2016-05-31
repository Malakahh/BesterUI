using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect.Face;

namespace BesterUI.Data
{
    public class FaceDataReading : DataReading
    {
        public Dictionary<FaceShapeAnimations, float> data = new Dictionary<FaceShapeAnimations, float>();


        public FaceDataReading(bool startReading) : base(startReading)
        {

        }

        public FaceDataReading() : this(false) { }

        public void AddData(IReadOnlyDictionary<FaceShapeAnimations, float> f)
        {
            foreach (var kvp in f)
                data.Add(kvp.Key, kvp.Value);
        }

        public override string Serialize()
        {
            string retVal = "";
            foreach (var item in data)
            {
                retVal += (int)item.Key + ":" + item.Value + "|";
            }

            return retVal;
        }

        public override void Write()
        {
            DataReading.StaticWrite("KINECT", this);
        }

        protected override DataReading Deserialize(string line)
        {
            data.Clear();

            var bits = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in bits)
            {
                var dat = item.Split(':');
                data.Add((FaceShapeAnimations)int.Parse(dat[0]), float.Parse(dat[1].Replace(',','.')));
            }

            return this;
        }
    }
}
