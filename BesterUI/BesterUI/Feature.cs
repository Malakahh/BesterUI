using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BesterUI.Data;
using BesterUI.Helpers;

namespace BesterUI
{
    public class Feature
    {
        public readonly string name;
        Func<List<DataReading>,int,int,double> featureCalculator;
        List<DataReading> dataReadings;

        public Feature(string name, Func<List<DataReading>, int, int, double> featureCalculator)
        {
            this.name = name;
            this.featureCalculator = featureCalculator;
        }

        public override string ToString()
        {
            return name;
        }

        public double GetValue(int startTime, int endTime)
        {
            if (dataReadings == null)
            {
                throw new Exception("ERROR: Call SetData before calling this function");
            }

            return featureCalculator(dataReadings, startTime, endTime);
        }

        public void SetData(List<DataReading> dataReadings)
        {
            this.dataReadings = dataReadings;
        }
    }
}
