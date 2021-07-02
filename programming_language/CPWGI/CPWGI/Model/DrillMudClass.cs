using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPWGI.Model
{
    /// <summary>
    /// 表示钻井液性质的类
    /// </summary>
    public class DrillMudClass
    {
        public string mudName { get; set; }
        //标况下泥浆密度
        public double densityStand { get; set; }
        public double viscosityStand { get; set; }

        public DrillMudClass( double densityStand,double viscosityStand,double volume,string mudName = "drillMud")
        {
            this.mudName = mudName;
            this.densityStand = densityStand;
            this.viscosityStand = viscosityStand;
            this.VolumeStand = volume;
            this.Quality = VolumeStand * densityStand;
            setMudPt(pressureStand, temperatureStand);
        }

        //标准大气压（Pa）和温度(K)
        public static double pressureStand = 101.325 * 1000;
        public static double temperatureStand = 273.15;
        //钻井液当前压力、温度、密度
        public double pressure { get; set; }
        public double temperature { get; set; }
        public double density { get; set; }
        public double viscosity { get; set; }
        public double VolumeStand { get; set; }
        public double Quality { get; set; }
        public double Volocity { get; set; }

        /// <summary>
        /// 设置钻井液温度和压力，并更新钻井液密度
        /// </summary>
        public void setMudPt(double pressure,double temperature)
        {
            this.pressure = pressure;
            this.temperature = temperature;
            density = calMudDensityAtPT(pressure, temperature);
            viscosity = calMudViscosityAtPt(pressure, temperature);
            Volocity = Quality / density;
            
        }

        private double calMudDensityAtPT(double p,double t)
        {
            return densityStand;
        }

        private double calMudViscosityAtPt(double p,double t)
        {
            return viscosityStand;
        }
    }
}
