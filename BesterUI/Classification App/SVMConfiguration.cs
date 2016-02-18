using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;

namespace Classification_App
{
    class SVMConfiguration
    {
        const char SEPARATOR = ';';
        public string Name = "undefined";
        public SVMParameter parameters;
        public List<Feature> features = new List<Feature>();

        public SVMConfiguration()
        {

        }

        public string Serialize()
        {
            return Name + SEPARATOR + parameters.C + SEPARATOR + parameters.Gamma + SEPARATOR + parameters.Kernel + SEPARATOR + FeatureCreator.GetStringFromFeatures(features);
        }

        public static SVMConfiguration Deserialize(string input)
        {
            SVMConfiguration retVal = new SVMConfiguration();

            string[] bits = input.Split(SEPARATOR);

            retVal.Name = bits[0];
            retVal.parameters.C = double.Parse(bits[1]);
            retVal.parameters.Gamma = double.Parse(bits[2]);
            retVal.parameters.Kernel = (SVMKernelType)Enum.Parse(typeof(SVMKernelType), bits[3]);
            retVal.features = FeatureCreator.GetFeaturesFromString(bits[4]);


            return retVal;
        }
    }
}
