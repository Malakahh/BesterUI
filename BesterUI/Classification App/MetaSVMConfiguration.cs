using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification_App
{
    class MetaSVMConfiguration
    {
        const char META_SEPARATOR = '&';
        public string Name = "undefined_meta";
        public List<SVMConfiguration> stds = new List<SVMConfiguration>();

        public string Serialize()
        {
            string retVal = Name;

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

            foreach (var item in bits.Skip(1))
            {
                msvmc.stds.Add(SVMConfiguration.Deserialize(item));
            }

            return msvmc;
        }
    }
}
