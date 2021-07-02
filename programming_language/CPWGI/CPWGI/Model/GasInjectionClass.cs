using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPWGI.Model
{
    public static class GasInjectionClass
    {
             
        public static double InjctionGasQuality(double pf,double pw)
        {
            double GasQuality;
            if (pf > pw)
                GasQuality = 0.5 * (pf - pw) / 1000000; 
            else
                GasQuality = 0;
            return GasQuality;
        }
    }
}
