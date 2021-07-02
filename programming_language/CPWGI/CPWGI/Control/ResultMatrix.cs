using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPWGI.MultiphaseCalculate;
using CPWGI.Model;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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
        public List<List<double>> gasInjection { get; set; }
        public List<List<double>> mudInjection { get; set; }

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
            gasInjection = new List<List<double>>();
            mudInjection = new List<List<double>>();
        }

        /// <summary>
        /// 增加时间，同时将各属性增加一行
        /// </summary>
        /// <param name="t">当前计算时间值</param>
        public void addTime(double t,int length=1)
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
            mudInjection.Add(new List<double>(new double[length]));
            gasInjection.Add(new List<double>(new double[length]));

            //int count = holdupGas.Count;
            //for (int i = 0; i < length; i++)
            //{
            //    holdupGas[count - 1][i] = 1;
            //    holdupLiquid[count - 1][i] = 1;
            //}
        }

        
        /// <summary>
        /// 复制计算结果列
        /// </summary>
        /// <param name="i">要复制的源列序号</param>
        /// <param name="To">复制到目标列序号</param>
        public void DuplicateTime(int i,int To)
        {

            time[To] = Clone<double>(time[i]);
            rheology[To] = Clone<List<rheologys>>(rheology[i]);
            gasDensity[To] = Clone<List<double>>(gasDensity[i]);
            mudDensity[To]= Clone<List<double>>(mudDensity[i]);
            gasVolocity[To] = Clone<List<double>>(gasVolocity[i]);
            mudVolocity[To] = Clone<List<double>>(mudVolocity[i]);
            gasViscosity[To] = Clone<List<double>>(gasViscosity[i]);
            mudViscosity[To] = Clone<List<double>>(mudViscosity[i]);
            tensionInterface[To] = Clone<List<double>>(tensionInterface[i]);
            holdupGas[To] = Clone<List<double>>(holdupGas[i]);
            holdupLiquid[To] = Clone<List<double>>(holdupLiquid[i]);
            wellborePressure[To] = Clone<List<double>>(wellborePressure[i]);
            wellboreTemperature[To] = Clone<List<double>>(wellboreTemperature[i]);
            wellboredPdZ[To] = Clone<List<double>>(wellboredPdZ[i]);
            mudInjection[To] = Clone<List<double>>(mudInjection[i]);
            gasInjection[To] = Clone<List<double>>(gasInjection[i]);
        }

        //采用序列化的方式完成对象的深复制
        public static T Clone<T>(T RealObject)
        {
            using (Stream objectStream=new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, RealObject);
                objectStream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(objectStream);
            }
        }
    }
}
