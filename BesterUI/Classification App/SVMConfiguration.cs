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
        public List<Feature> features;

        public SVMConfiguration()
        {

        }

        public string Serialize()
        {
            return parameters.C.ToString() + SEPARATOR + parameters.Gamma + SEPARATOR + parameters.Kernel + SEPARATOR + FeatureCreator.GetStringFromFeatures(features);
        }

        public static SVMConfiguration Deserialize(string input)
        {
            SVMConfiguration retVal = new SVMConfiguration();

            string[] bits = input.Split(SEPARATOR);

            retVal.parameters.C = double.Parse(bits[0]);
            retVal.parameters.Gamma = double.Parse(bits[1]);
            retVal.parameters.Kernel = (SVMKernelType)Enum.Parse(typeof(SVMKernelType), bits[2]);

            retVal.features = FeatureCreator.GetFeaturesFromString(bits[3]);

            return retVal;
        }
    }
}
