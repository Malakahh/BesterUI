using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BesterUI
{
    static class EnumExtension
    {
        public static string GetName(this System.Enum e)
        {
            return System.Enum.GetName(e.GetType(), e);
        }
    }
}
