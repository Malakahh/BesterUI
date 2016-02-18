using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BesterUI.Data;

namespace Classification_App
{
    public class Feature
    {
        public readonly string name;
        Func<List<DataReading>, SAMDataPoint, double> featureCalculator;
        List<DataReading> dataReadings;
        Dictionary<SAMDataPoint, double> cachedResults = new Dictionary<SAMDataPoint, double>();

        public Feature(string name, Func<List<DataReading>, SAMDataPoint, double> featureCalculator)
        {
            this.name = name;
            this.featureCalculator = featureCalculator;
        }

        public override string ToString()
        {
            return name;
        }

        public double GetValue(SAMDataPoint sam)
        {
            if (dataReadings == null)
            {
                throw new Exception("ERROR: Du skal fandme lige bruge Feature.SetData() først!!");
            }

            if (!cachedResults.ContainsKey(sam))
            {
                cachedResults.Add(sam, featureCalculator(dataReadings, sam));
            }

            return cachedResults[sam];
        }

        public List<double> GetAllValues(SAMData samd)
        {
            if (dataReadings == null)
            {
                throw new Exception("ERROR: You need to call SetData before this function is usable");
            }

            List<double> values = new List<double>();
            foreach (SAMDataPoint sd in samd.dataPoints)
            {
                values.Add(GetValue(sd));
            }
            return values;
        }

        public void SetData(List<DataReading> dataReadings)
        {
            this.dataReadings = dataReadings;
            cachedResults.Clear();
        }
    }
}
