using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classification_App
{
    static class Extensions
    {
        /// <summary>
        /// Item1 is the training set, Item2 is the prediction set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="nFold"></param>
        /// <returns></returns>
        public static List<Tuple<List<T>, List<T>>> GetCrossValidationSets<T>(this IEnumerable<T> original, int nFold)
        {
            var allSets = new List<Tuple<List<T>, List<T>>>();

            if (original.Count() % nFold != 0)
            {
                return null;
            }

            for (int i = 0; i < original.Count(); i += nFold)
            {
                var trainList = original.Except(original.Skip(i).Take(nFold)).ToList();
                var predictList = original.Except(trainList).ToList();

                allSets.Add(new Tuple<List<T>, List<T>>(trainList, predictList));
            }

            return allSets;
        }
    }
}
