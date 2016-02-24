using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;

namespace Classification_App
{
    class MetaSVMConfiguration
    {
        const char META_SEPARATOR = '&';
        public string Name = "undefined_meta";
        public List<SVMConfiguration> stds = new List<SVMConfiguration>();
        public SVMParameter parameter = new SVMParameter();

        public string Serialize()
        {
            string retVal = Name + META_SEPARATOR + parameter.C + META_SEPARATOR + parameter.Gamma;

            foreach (var item in stds)
            {
                retVal += META_SEPARATOR + item.Serialize();
            }

            return retVal;
        }

        public static MetaSVMConfiguration Deserialize(string input)
        {
            MetaSVMConfiguration msvmc = new MetaSVMConfiguration();
            var bits = input.Split(META_SEPARATOR);

            msvmc.Name = bits[0];
            msvmc.parameter.C = double.Parse(bits[1]);
            msvmc.parameter.Gamma = double.Parse(bits[2]);

            foreach (var item in bits.Skip(3))
            {
                msvmc.stds.Add(SVMConfiguration.Deserialize(item));
            }

            return msvmc;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
