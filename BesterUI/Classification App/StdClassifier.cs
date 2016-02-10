using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;

namespace Classification_App
{
    class StdClassifier : Classifier
    {
        private List<double>[] features;
        public override List<SVMParameter> Parameters { get; set; }

        public StdClassifier(string Name, List<SVMParameter> Parameters, List<List<double>> Features) : base(Name, Parameters)
        {
            Features.CopyTo(this.features);
        }

        public StdClassifier(string Name, SVMParameter Parameter, List<List<double>> Features) : base(Name, Parameter)
        {
            Features.CopyTo(this.features);
        }

        public void CrossValidate(bool RunCombinations)
        {
            
        }

        public override void PrintResults()
        {
            throw new NotImplementedException();
        }
        
    }
}
