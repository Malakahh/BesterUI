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
        public MetaClassifier(string Name, List<SVMParameter> Parameters) : base(Name, Parameters)
        {
        }

        public MetaClassifier(string Name, SVMParameter Parameter) : base(Name, Parameter)
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
