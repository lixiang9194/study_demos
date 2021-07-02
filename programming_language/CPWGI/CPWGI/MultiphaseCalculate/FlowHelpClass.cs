using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPWGI.MultiphaseCalculate
{
    public static class FlowHelpClass
    {
        /// <summary>
        /// 计算水-天然气界面表面张力，参数压力（Pa）温度（K）
        /// </summary>
        /// <returns>表面张力(N/m)</returns>
        public static double CalculateTensionOfWaterAndGas(double pressure,double temperature)
        {
            double tension = 0;
            pressure = pressure / 1000;
            temperature = temperature - 273.15;
            tension = (248 - 1.8 * temperature) / 206 * (76 * Math.Exp(-0.0003625 * pressure) - 52.5 + 0.00087 * pressure) + 52.5 - 0.00087 * pressure;
            //return tension/1000;
            return 0.05;
            //todo 界面张力修改为了定值
        }
    }
}
