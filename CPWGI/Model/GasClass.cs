using CPWGI.MathHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPWGI.Model
{
    /// <summary>
    /// 表示气体性质的类
    /// 使用方法：1.输入气体名称和标况下密度进行初始化
    /// 2.调用setGasPT设置气体温度压力，自动更新气体密度
    /// </summary>
    public class GasClass
    {
        public string gasName { get; set; }

        /// <summary>
        /// 设置气体为干气或者湿气，默认干气，设置该值后需要调用updateGasProperty更新气体属性
        /// </summary>
        public gasTypes gasType { get; set; }

        /// <summary>
        /// 设置气体偏差因子Z计算方法，默认为推荐方法，设置该值后需要调用updateGasProperty更新气体属性
        /// </summary>
        public calZMethods calZMethod { get; set; }

        /// <summary>
        /// 设置计算气体粘度的方法，默认为李氏法，设置该值后需要调用updateGasProperty更新气体粘度
        /// </summary>
        public calViscosityMethods calViscosityMethod { get; set; }

        /// <summary>
        /// 设置标况下气体密度（Kg/m3），设置该值后需要调用updateGasProperty更新气体密度
        /// </summary>
        public double densityStand { get; set; }

        /// <summary>
        /// 气体偏差因子
        /// </summary>
        public double Z { get;private set; }

        //标准大气压（Pa）和温度(K)
        public static double pressureStand = 101.325 * 1000;
        public static double temperatureStand = 273.15;
        public static double densityStandAir = 1.293;

        /// <summary>
        /// 当前环境压力（Pa),请通过setGasPT设置
        /// </summary>
        public double pressure { get;private set; }

        /// <summary>
        /// 当前环境温度（K）,请通过setGasPT设置
        /// </summary>
        public double temperature { get;private set; }

        /// <summary>
        /// 设置当前状态下气体的质量（Kg），将自动更新气体体积
        /// </summary>
        public double quality { get; set; }
        
        /// <summary>
        /// 当前状态下气体的体积（m3），会根据气体质量、密度自动更新
        /// </summary>
        public double volumeAtPT {
            get { return quality / densityAtPt; }
            private set { volumeAtPT = value; }
        }

        /// <summary>
        /// 气体当前温压下的密度（kg/m3）
        /// </summary>
        public double densityAtPt { get;private set; }

        /// <summary>
        /// 气体相对密度
        /// </summary>
        public double densityGasToAir { get;private set; }

        /// <summary>
        /// 气体的粘度，可调用updateGasProperty更新
        /// </summary>
        public double viscosity { get; set; }

        /// <summary>
        /// 输入气体的名称和标况下的密度进行初始化
        /// </summary>
        public GasClass(double densityAtStand, string gasName="NatualGas")
        {
            this.gasName = gasName;
            densityStand = densityAtStand;
            
            gasType = gasTypes.dryGas;
            calZMethod = calZMethods.recommend;
            calViscosityMethod = calViscosityMethods.Lee;
            setGasPT(pressureStand, temperatureStand);
        }
        
        /// <summary>
        /// 显式设置气体当前压力和温度，自动更新气体密度及粘度
        /// </summary>
        public void setGasPT(double pressure,double temperature)
        {
            this.pressure = pressure;
            this.temperature = temperature;
            updateGasProperty();   
        }

        /// <summary>
        /// 更改气体类型、计算方法、气体标况密度后需要调用此函数更新气体密度、粘度等属性
        /// </summary>
        public void updateGasProperty()
        {
            Z = calculateZAtPT();
            densityAtPt = Z * densityStand * pressure / pressureStand / temperature * temperatureStand;
            viscosity = calculateViscosityAtPT();
        }

        /// <summary>
        /// 根据calZMethod不同值选择不同的公式计算气体偏差因子
        /// </summary>
        private double calculateZAtPT()
        {
            //天然气的相对密度一般在0.58-0.62之间，
            //石油伴生产的相对密度在0.7-0.85之间，个别含重烃多的油田也有大于1
            densityGasToAir = densityStand / densityStandAir;

            //计算天然气的视临界温度和视临界压力
            double a0, a1, b0, b1;
            if (gasType == gasTypes.wetGas)//湿气
            {
                if (densityGasToAir < 0.7)
                {
                    a0 = 106; a1 = 152.22; b0 = 4778; b1 = -248.21;
                }
                else
                {
                    a0 = 132; a1 = 116.67; b0 = 5102; b1 = -689.48;
                }
            }
            else//干气
            {
                if (densityGasToAir < 0.7)
                {
                    a0 = 92; a1 = 176.67; b0 = 4778; b1 = -248.21;
                }
                else
                {
                    a0 = 92; a1 = 176.67; b0 = 4881; b1 = -386.11;
                }
            }

            double temperatureCritical, pressureCritical;
            temperatureCritical = a0 + a1 * densityGasToAir;//视临界温度
            pressureCritical = (b0 + b1 * densityGasToAir)*1000;//视临界压力

            double temperatureRelative, pressureRelative;
            temperatureRelative = temperature / temperatureCritical;//拟对比温度
            pressureRelative = pressure / pressureCritical;//拟对比压力
           
            double z = 0;
            switch (calZMethod)
            {
                case calZMethods.recommend:
                    //此方法为书籍石油气液两相管流推荐计算方法（P238）
                    //在中低压条件（35MPa以下）德兰查克-珀维斯-鲁滨逊公式计算比较准确
                    //在高压条件下，霍尔-亚巴勒公式比较准确
                    //然而，计算出来Z值不连续，故我选择了大于35MPa时两个方法都算一下，然后返回较小值
                    if (pressure < 35000000)
                    {
                        z = calculateZuseDPL(pressureRelative, temperatureRelative);
                    }
                    else
                    {
                        double z1 = calculateZuseDPL(pressureRelative, temperatureRelative);
                        double z2 = calculateZuseHY(pressureRelative, temperatureRelative);
                        z = (z1 < z2) ? z1 : z2;
                    }
                    break;
                case calZMethods.DPL:
                    z = calculateZuseDPL(pressureRelative, temperatureRelative);
                    break;
                case calZMethods.HY:
                    z = calculateZuseHY(pressureRelative, temperatureRelative);
                    break;
                case calZMethods.GS:
                    z = calculateZuseGS(pressureRelative, temperatureRelative);
                    break;
            }
            return z;
        }

        /// <summary>
        /// 使用德兰查克-珀维斯-鲁滨逊公式计算天然气压缩因子
        /// </summary>
        private double calculateZuseDPL(double pressureRelative, double temperatureRelative)
        {
            double z = 1.0;
            double densityRelative;
            //迭代5次计算天然气压缩因子
            for (int i = 0; i < 5; i++)
            {
                densityRelative = 0.27 * pressureRelative / z / temperatureRelative;
                z = 1 + (0.3051 - 1.0467 / temperatureRelative - 0.5783 / Math.Pow(temperatureRelative, 3)) * densityRelative
                    + (0.5353 - 0.6123 / temperatureRelative + 0.6816 / Math.Pow(temperatureRelative, 3)) * Math.Pow(densityRelative, 2);
            }
            return z;
        }

        /// <summary>
        /// 使用霍尔-亚巴勒公式计算天然气压缩因子
        /// </summary>
        private double calculateZuseHY(double pressureRelative,double temperatureRelative)
        {
            double z = 1.0,YY=0;
            //因为Z的值约为1，所以这里设置Y初值为zY，加速收敛
            double zY = 0.06125 * pressureRelative / temperatureRelative * Math.Exp(-1.2 * (1 - 1 / temperatureRelative) * (1 - 1 / temperatureRelative));
            double K1 = 14.76 / temperatureRelative - 9.76 / Math.Pow(temperatureRelative, 2) + 4.58 / Math.Pow(temperatureRelative, 3);
            double K2 = 90.7 / temperatureRelative - 242.2 / Math.Pow(temperatureRelative, 2) + 42.4 / Math.Pow(temperatureRelative, 3);
            double K3 = 2.18 + 2.82 / temperatureRelative;
            Func<double,double> fy=(Y)=> -zY + (Y + Y * Y + Math.Pow(Y, 3) + Math.Pow(Y, 4)) / Math.Pow(1 - Y, 3)
                    - K1 * Y * Y + K2 * Math.Pow(Y, K3);
            Func<double,double> fdy=(Y) =>(1 + 2 * Y + 3 * Y * Y + 4 * Math.Pow(Y, 3)) / Math.Pow(1 - Y, 3) + (Y + Y * Y + Math.Pow(Y, 3) + Math.Pow(Y, 4)) * 3 / Math.Pow(1 - Y, 4)
                    - 2 * K1 * Y + K3 * K2 * Math.Pow(Y, K3 - 1);
            YY = MathSolveEquations.SolveEquationsWithNewtonDownHillMothod(fy, fdy, zY);
            z = zY / YY;
            return z;
        }

        private enum prStaus { prS1, prS2, prS3 };
        private enum tpStatus { tpS1,tpS2,tpS3,tpS4};
        /// <summary>
        /// 使用戈帕尔公式计算天然气压缩因子
        /// </summary>
        private double calculateZuseGS(double pressureRelative, double temperatureRelative)
        {
            double z = 1.0;
            //由天然气视对比状态决定的系数
            if (pressureRelative > 5.4 && pressureRelative <= 15.0 && temperatureRelative >= 1.05 && temperatureRelative <= 3.0)
                z = pressureRelative / Math.Pow(3.66 * temperatureRelative + 0.711, 1.4667) - 1.637 / (0.319 * temperatureRelative + 0.522) + 2.071;
            else if (pressureRelative > 0.2 && pressureRelative <= 5.4 && temperatureRelative >= 1.05 && temperatureRelative <= 3.0)
            {
                double A = 0, B = 0, C = 0, D = 0;
                prStaus prS;
                tpStatus trS;
                if (pressureRelative < 1.2) prS = prStaus.prS1;
                else if (pressureRelative < 2.8) prS = prStaus.prS2;
                else prS = prStaus.prS3;

                if (temperatureRelative < 1.2) trS = tpStatus.tpS1;
                else if (temperatureRelative < 1.4) trS = tpStatus.tpS2;
                else if (temperatureRelative < 2.0) trS = tpStatus.tpS3;
                else trS = tpStatus.tpS4;

                switch (prS)
                {
                    case prStaus.prS1:
                        switch (trS)
                        {
                            case tpStatus.tpS1:
                                A = 1.6643; B = -2.2114; C = -0.3647; D = 1.4385; break;
                            case tpStatus.tpS2:
                                A = 0.5222; B = -0.8511; C = -0.0364; D = 1.0490; break;
                            case tpStatus.tpS3:
                                A = 0.1392; B = -0.2988; C = 0.0007; D = 0.9969; break;
                            case tpStatus.tpS4:
                                A = 0.0295; B = -0.0825; C = 0.0009; D = 0.9967; break;
                        }
                        break;
                    case prStaus.prS2:
                        switch (trS)
                        {
                            case tpStatus.tpS1:
                                A = -1.3570; B = 1.4942; C = 4.6315; D = -4.7006; break;
                            case tpStatus.tpS2:
                                A = 0.1717; B = 0.3232; C = 0.5869; D = 0.1229; break;
                            case tpStatus.tpS3:
                                A = 0.0984; B = -0.2053; C = 0.0621; D = 0.8580; break;
                            case tpStatus.tpS4:
                                A = 0.0211; B = -0.0527; C = 0.0127; D = 0.9549; break;
                        }
                        break;
                    case prStaus.prS3:
                        switch (trS)
                        {
                            case tpStatus.tpS1:
                                A = -0.3278; B = 0.4752; C = 1.8223; D = -1.9036; break;
                            case tpStatus.tpS2:
                                A = -0.2521; B = 0.3871; C = 1.6087; D = -1.6635; break;
                            case tpStatus.tpS3:
                                A = -0.0284; B = 0.0625; C = 0.4714; D = -0.0011; break;
                            case tpStatus.tpS4:
                                A = 0.0041; B = 0.0039; C = 0.0607; D = 0.7927; break;
                        }
                        break;
                }
                z = pressureRelative * (A * temperatureRelative + B) + C * temperatureRelative + D;
            }
            return z;
        }
 
        /// <summary>
        /// 根据calViscsityMethod不同值选择不同公式计算天然气粘度
        /// </summary>
        private double calculateViscosityAtPT()
        {
            double viscosityGas = 0;
            switch (calViscosityMethod)
            {
                case calViscosityMethods.Lee:
                    viscosityGas = calculateViscosityUseLee();
                    break;
                case calViscosityMethods.Dempsey:
                    viscosityGas = calculateViscosityUseDempsey();
                    break;
            }
            return viscosityGas;
        }

        /// <summary>
        /// 使用李氏公式计算天然气的粘度
        /// </summary>
        private double calculateViscosityUseLee()
        {
            //石油气液两相管流244页
            double viscosityGas = 0;
            double x = 3.5 + 548 / temperature + 0.29 * densityGasToAir;
            double y = 2.4 - 0.2 * x;
            double C = (1.26 + 0.078 * densityGasToAir) * Math.Pow(temperature, 1.5) / (116 + 306 * densityGasToAir + temperature);
            viscosityGas = C * Math.Exp(x * Math.Pow(densityAtPt / 1000, y)) / 1000;//单位为mPa.s
            return viscosityGas/1000;
        }

        /// <summary>
        /// 登浦西对卡尔等的曲线进行多重回归分析得到的计算公式
        /// </summary>
        private double calculateViscosityUseDempsey()
        {
            double viscosityGas = 0;
            double a0 = -2.46212, a1 = 2.97055, a2 = -0.286264, a3 = -0.00805421, a4 = 2.80861, a5 = -3.49803,
                a6 = 0.360673, a7 = -0.0104432, a8 = -0.793386, a9 = 1.39643, a10 = -0.149145,
                a11 = 0.00441016, a12 = 0.0839387, a13 = -0.186409, a14 = 0.0203368, a15 = -6.09597 / 10000;
            double b0 = 0.0111232, b1 = 1.67727 / 100000, b2 = 2.11360 / Math.Pow(10, 9), b3 = -1.09485 / 10000, b4 = -6.40316 / Math.Pow(10, 8),
                b5 = -8.99375 / Math.Pow(10, 11), b6 = 4.57735 / Math.Pow(10, 7), b7 = 2.12903 / Math.Pow(10, 10), b8 = 3.97732 / Math.Pow(10, 13);
            double Tr = temperature / (97.55 + 171.09 * densityGasToAir);
            double Pr = pressure/1000 / (4830.11 - 330.53 * densityGasToAir);
            double TF = 1.8 * temperature - 459.67;
            double Mg = 28.97 * densityGasToAir;
            //如果气体参数超出该方法适应范围，则返回0
            bool flag = Pr > 1.0 && Pr < 20.0 && Tr > 1.2 && Tr < 3.0 && Mg > 16.0 && Mg < 110 && temperature > 277.6 && temperature < 477.6;
            if (!flag) return viscosityGas;
            double viscosityStand = b0 + b1 * TF + b2 * TF * TF + b3 * Mg + b4 * Mg * TF + b5 * Mg * TF * TF + b6 * Mg * Mg + b7 * Mg * Mg * TF + b8 * Mg * Mg * TF * TF;
            double tempK = a0 + a1 * Pr + a2 * Pr * Pr + a3 * Pr * Pr * Pr + Tr * (a4 + a5 * Pr + a6 * Pr * Pr + a7 * Pr * Pr * Pr)
                + Tr * Tr * (a8 + a9 * Pr + a10 * Pr * Pr + a11 * Pr * Pr * Pr) + Tr * Tr * Tr * (a12 + a13 * Pr + a14 * Pr * Pr + a15 * Pr * Pr * Pr);
            viscosityGas = Math.Exp(tempK) / Tr * viscosityStand;
            return viscosityGas;
        }
    }

    public enum gasTypes
    {
        /// <summary>
        /// 干气
        /// </summary>
        dryGas,
        /// <summary>
        /// 湿气
        /// </summary>
        wetGas
    };
    public enum calZMethods
    {
        /// <summary>
        /// 此方法为书籍石油气液两相管流推荐计算方法（P238）.
        ///在中低压条件（35MPa以下）德兰查克-珀维斯-鲁滨逊公式计算比较准确
        ///在高压条件下，霍尔-亚巴勒公式比较准确
        ///然而，计算出来Z值不连续，故我选择了大于35MPa时两个方法都算一下，然后返回较小值
        /// </summary>
        [Description("石油气液两相管流推荐计算方法")]
        recommend,
        
        /// <summary>
        /// 德兰查克-珀维斯-鲁滨逊方法，低压条件下准确度高，但高压时数值偏大
        /// </summary>
        [Description("德兰查克-珀维斯-鲁滨逊方法")]
        DPL,

        /// <summary>
        /// 霍尔-亚巴勒方法，高压条件下较准确，连续性较好，但采用牛顿下山法计算，运算速度较慢
        /// </summary>
        [Description("霍尔-亚巴勒方法")]
        HY,

        /// <summary>
        /// 戈帕尔方法，不需迭代，计算速度较快
        /// </summary>
        [Description("戈帕尔方法")]
        GS
    }
    public enum calViscosityMethods
    {
        /// <summary>
        /// 1965年李氏计算方法，计算简单，连续性好
        /// </summary>
        Lee,
        /// <summary>
        /// 1965年登浦西对卡尔的曲线图版进行多重回归分析得到的方法，在端点处连续性不好
        /// </summary>
        Dempsey
    }
}

