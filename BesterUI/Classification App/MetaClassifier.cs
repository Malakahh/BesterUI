using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;

namespace Classification_App
{
    class MetaClassifier : Classifier
    {
        private List<StdClassifier> standardClassifiers = new List<StdClassifier>();


        public MetaClassifier(string Name, List<SVMParameter> Parameters, SAMData SamData) : base(Name, Parameters, SamData)
        {
        }

        public MetaClassifier(string Name, SVMParameter Parameter, SAMData SamData) : base(Name, Parameter, SamData)
        {
        }


        public void DoStacking()
        {
        }

        public void DoVoting()
        {
        }

        public void DoBoosting()
        {
        }


        public override List<SVMParameter> Parameters
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void PrintResults()
        {
            throw new NotImplementedException();
        }
    }
}
