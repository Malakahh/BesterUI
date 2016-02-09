using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification_App
{
    class MetaClassifier : Classifier
    {
        public MetaClassifier(string Name) : base(Name)
        { }

        protected override void CrossValidate(List<double> CTypes, List<double> GTypes, bool RunCombinations)
        {
            throw new NotImplementedException();
        }


        public override void PrintResults()
        {
            throw new NotImplementedException();
        }
    }
}
