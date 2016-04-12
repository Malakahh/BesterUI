using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BesterUI.Data;
using BesterUI.Helpers;

namespace Classification_App
{
    public class Feature
    {
        public readonly string name;
        Func<List<DataReading>, double> featureCalculator;
        List<DataReading> dataReadings;
        Dictionary<string, double> cachedResults = new Dictionary<string, double>();

        public Feature(string name, Func<List<DataReading>, double> featureCalculator)
        {
            this.name = name;
            this.featureCalculator = featureCalculator;
        }

        public override string ToString()
        {
            return name;
        }

        public double GetValue(string Name)
        {
            if (dataReadings == null)
            {
                throw new Exception("ERROR: Du skal fandme lige bruge Feature.SetData() først!!");
            }

            if (!cachedResults.ContainsKey(Name))
            {
                    cachedResults.Add(Name, featureCalculator(dataReadings));

            }
                return cachedResults[Name];
        }

        public void SetData(List<DataReading> dataReadings)
        {
            this.dataReadings = dataReadings;
            cachedResults.Clear();
        }
    }
}
