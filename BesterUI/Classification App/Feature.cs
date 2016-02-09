using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BesterUI.Data;

namespace Classification_App
{
    class Feature<T> where T : DataReading
    {
        string name;
        Func<List<T>, SAMDataPoint, double> featureCalculator;
        List<T> dataReadings;
        Dictionary<SAMDataPoint, double> cachedResults = new Dictionary<SAMDataPoint, double>();

        public Feature(string name, Func<List<T>, SAMDataPoint, double> featureCalculator)
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

        public void SetData(List<T> dataReadings)
        {
            this.dataReadings = dataReadings;
            cachedResults.Clear();
        }
    }
}
