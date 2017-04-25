using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPWGI.MultiphaseCalculate;

namespace CPWGI.Control
{
    public class ResultMatrix
    {
        public List<double> time { get; set; }
        public List<List<rheologys>> rheology { get; set; }
        public List<List<double>> gasDensity{get;set;}
        public List<List<double>> mudDensity { get; set; }
        public List<List<double>> gasVolocity { get; set; }
        public List<List<double>> mudVolocity { get; set; }
        public List<List<double>> gasViscosity { get; set; }
        public List<List<double>> mudViscosity { get; set; }
        public List<List<double>> tensionInterface { get; set; }
        public List<List<double>> holdupGas { get; set; }
        public List<List<double>> holdupLiquid { get; set; }
        public List<List<double>> wellborePressure { get; set; }
        public List<List<double>> wellboreTemperature { get; set; }
        public List<List<double>> wellboredPdZ { get; set; }

        /// <summary>
        /// 将各属性初始化为矩阵，第一维代表时间，第二维代表井深
        /// </summary>
        public ResultMatrix()
        {
            time = new List<double>();
            rheology = new List<List<rheologys>>();
            gasDensity = new List<List<double>>();
            mudDensity = new List<List<double>>();
            gasVolocity = new List<List<double>>();
            mudVolocity = new List<List<double>>();
            gasViscosity = new List<List<double>>();
            mudViscosity = new List<List<double>>();
            tensionInterface = new List<List<double>>();
            holdupGas = new List<List<double>>();
            holdupLiquid = new List<List<double>>();
            wellborePressure = new List<List<double>>();
            wellboreTemperature = new List<List<double>>();
            wellboredPdZ = new List<List<double>>();
        }

        /// <summary>
        /// 增加时间，同时将各属性增加一行
        /// </summary>
        /// <param name="t">当前计算时间值</param>
        public void addTime(double t,int length=0)
        {
            time.Add(t);
            rheology.Add(new List<rheologys>(new rheologys[length]));
            gasDensity.Add(new List<double>(new double[length]));
            mudDensity.Add(new List<double>(new double[length]));
            gasVolocity.Add(new List<double>(new double[length]));
            mudVolocity.Add(new List<double>(new double[length]));
            gasViscosity.Add(new List<double>(new double[length]));
            mudViscosity.Add(new List<double>(new double[length]));
            tensionInterface.Add(new List<double>(new double[length]));
            holdupGas.Add(new List<double>(new double[length]));
            holdupLiquid.Add(new List<double>(new double[length]));
            wellborePressure.Add(new List<double>(new double[length]));
            wellboreTemperature.Add(new List<double>(new double[length]));
            wellboredPdZ.Add(new List<double>(new double[length]));
        }

        public void DuplicateTime(int i,int To)
        {
            time[To] = time[i];
            rheology[To] = rheology[i];
        }
    }
}
