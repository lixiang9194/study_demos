using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CPWGI.Model;
using CPWGI.MultiphaseCalculate;

namespace CPWGI.Control
{
    public class ControlCalculateClass
    {
        public WellBoreClass WellBore { get; set; }
        public DrillMudClass Mud { get; set; }
        public GasClass Gas { get; set; }
        public calculateMultiphaseBaseClass Calculate { get; set; }
        public ResultMatrix Result { get; set; }
        public int WellGridNumber { get; set; }
        public double PressureWellHead { get; set; }
        public double ErrorWell = 100000, ErrorSegment = 500, ErrorHoldup = 0.001;
        public const double Gravity = 9.81;
        private double pressureFormation = 0;
        private double timeStep =5;
        private double gasToHeadIndex = 0;
        //设置一个足够大的初始误差值
        private const double errorInitial = 999999999;
        //如果值小于此值，则停止继续迭代
        private readonly double errorMinimalStop = Math.Pow(0.1,10);
        private Random random=new Random();

        public ControlCalculateClass(WellBoreClass well,DrillMudClass mud,GasClass gas,calculateMultiphaseBaseClass cal,ResultMatrix result,double wellHeadPressure)
        {
            this.WellBore = well;
            this.Mud = mud;
            this.Gas = gas;
            this.Calculate = cal;
            this.Result = result;
            this.PressureWellHead = wellHeadPressure;
            WellGridNumber = WellBore.wellBoreGrid.Count;
        }

        public void CalculateSteady(double times,double step)
        {
            CalculateSteadyT(0, 0);
            double t = 0;
            int index = 0;
            while(t<times)
            {
                t = t + step;
                index = index + 1;
                Result.DuplicateTime(index - 1, index);
            }
        }

        /// <summary>
        ///给定时间和时间序号，计算单向流时流动结果
        /// </summary>
        public void CalculateSteadyT(double t,int tIndex)
        {
            Calculate.CalFMethod = CalFrictionMethods.Simple;
            Result.addTime(t,WellGridNumber);
            double GridLength = 0;
            Result.wellborePressure[tIndex][0] = PressureWellHead;
            Result.wellboreTemperature[tIndex][0] = WellBore.wellBoreGrid[0].temperature;
            Result.mudDensity[tIndex][0] = Mud.density;
            Result.mudViscosity[tIndex][0] = Mud.viscosity;
            Result.mudVolocity[tIndex][0] = Mud.Volocity / WellBore.wellBoreGrid[0].GridArea;
            //沿井身网格向下计算
            for (int i = 1; i < WellGridNumber; i++)
            {
                Calculate.setAnnular(WellBore.wellBoreGrid[i].outterDiameter, WellBore.wellBoreGrid[i].innerDiameter);
                GridLength = WellBore.wellBoreGrid[i].wellDepth - WellBore.wellBoreGrid[i - 1].wellDepth;
                double PressureDHSuppose = Mud.density * Gravity * (GridLength);
                double PressuredH = 0, PressureAverage = 0, TemperatureAverage = 0;
                double ErrorSegmentCal = 999000000;
                double kkl = 0, kk = 0, kkr = 0;
                while (Math.Abs(ErrorSegmentCal) > ErrorSegment)
                {
                    if (ErrorSegmentCal == 999000000)
                    { kkl = 0.5; kk = 1; kkr = 1.5; }
                    else if (ErrorSegmentCal > 0)
                    { kkl = kk; kk = (kk + kkr) / 2; }
                    else
                    { kkr = kk; kk = (kk + kkl) / 2; }

                    if (Math.Abs(kkr - kkl) < Math.Pow(0.1, 20))
                    {
                        throw new Exception("稳态计算就不收敛了，喜闻乐见！"); 
                    }

                    PressuredH = PressureDHSuppose * kk;
                    Result.wellboreTemperature[tIndex][i] = WellBore.wellBoreGrid[i].temperature;
                    Result.wellborePressure[tIndex][i] = Result.wellborePressure[tIndex][i - 1] + PressuredH;
                    PressureAverage = Result.wellborePressure[tIndex][i ] / 2 + Result.wellborePressure[tIndex][i - 1] / 2;
                    TemperatureAverage = Result.wellboreTemperature[tIndex][i] / 2 + Result.wellboreTemperature[tIndex][i - 1] / 2;
                    Mud.setMudPt(PressureAverage, TemperatureAverage);
                    Result.mudDensity[tIndex][i] = Mud.density;
                    Result.mudViscosity[tIndex][i] = Mud.viscosity;
                    Result.mudVolocity[tIndex][i] = Mud.Volocity / WellBore.wellBoreGrid[i].GridArea;
                    Result.holdupLiquid[tIndex][i] = 1;
                    Result.holdupGas[tIndex][i] = Calculate.calculateHoldupGas(0, Result.mudVolocity[tIndex][i], 0, Result.mudDensity[tIndex][i], 0, Result.mudViscosity[tIndex][i], 0);
                    Result.wellboredPdZ[tIndex][i] = Calculate.calculatedPdZ(0, Result.mudVolocity[tIndex][i], 0, Result.mudDensity[tIndex][i], 0, Result.mudViscosity[tIndex][i]);
                    ErrorSegmentCal = Result.wellboredPdZ[tIndex][i] * GridLength - PressuredH;
                }
            }
            Console.WriteLine(Result.wellborePressure[0][100]);
        }

        /// <summary>
        /// 给定开始气侵的时间和时间序号，计算气侵后流动情况
        /// </summary>
        public void CalMultiphaseFlow(double t,int tIndex)
        {
            //Calculate.CalFMethod = CalFrictionMethods.Corebook;
            //暂时设置气藏压力小于井底压力1Mpa
            //todo 气藏压力这里需要完善
            pressureFormation = Result.wellborePressure[0][WellGridNumber-1] + 1000000;

            bool stop = true;
            while(stop)//todo 此处暂写为死循环，应该判断气侵稳定后停止运算
            {
                if (tIndex > 99) stop = false;
                Result.addTime(t, WellGridNumber);
                System.Diagnostics.Debug.Print(t-1 + "时刻井底压力为：" + Result.wellborePressure[tIndex-1][WellGridNumber-1]);
                double errorWellCalculate = errorInitial;
                double kkWellL=0, kkWell=1, kkWellR=2;
                //如果井口压力计算误差大于要求，则继续循环
                while (Math.Abs(errorWellCalculate)>ErrorWell)
                {
                    //System.Diagnostics.Debug.Print(t + "时刻井底压力为：" + Result.wellborePressure[tIndex][WellGridNumber - 1]);
                    if (errorWellCalculate==errorInitial)
                    { kkWellL = 0.95;kkWell = 1;kkWellR = 1.05; }
                    else if(Math.Abs(kkWellL-kkWellR)<errorMinimalStop)
                    { kkWellL = 0.99; kkWell = 0.99+0.01*random.NextDouble(); kkWellR = 1.05;
                        System.Diagnostics.Debug.Print(t + "时刻井底压力不收敛!");
                    }
                    else if (errorWellCalculate>0)
                    { kkWellR = kkWell;kkWell = (kkWell + kkWellL) / 2; }
                    else
                    { kkWellL = kkWell;kkWell = (kkWell + kkWellR) / 2; }
                    //存储结果
                    Result.wellborePressure[tIndex][WellGridNumber-1] = Result.wellborePressure[tIndex - 1][WellGridNumber-1] * kkWell;
                    Result.wellboreTemperature[tIndex][WellGridNumber-1] = WellBore.wellBoreGrid[WellGridNumber-1].temperature;
                    Mud.setMudPt(Result.wellborePressure[tIndex][WellGridNumber-1], Result.wellboreTemperature[tIndex][WellGridNumber-1]);
                    Gas.setGasPT(Result.wellborePressure[tIndex][WellGridNumber-1], Result.wellboreTemperature[tIndex][WellGridNumber-1]);
                    Result.mudDensity[tIndex][WellGridNumber-1] = Mud.density;
                    Result.mudViscosity[tIndex][WellGridNumber-1] = Mud.viscosity;
                    Result.holdupLiquid[tIndex][WellGridNumber - 1] = 0;
                    Result.gasDensity[tIndex][WellGridNumber-1] = Gas.densityAtPt;
                    Result.gasViscosity[tIndex][WellGridNumber-1] = Gas.viscosity;
                    //设置标志，井身压力小于井口回压时直接跳出
                    bool breakWhilePUnderHead = false;
                    //沿井身网格从下到上进行计算
                    for (int gridIndex = WellGridNumber-2; gridIndex >=0; gridIndex--)
                    {
                        
                        double gridLength = WellBore.wellBoreGrid[gridIndex + 1].wellDepth - WellBore.wellBoreGrid[gridIndex].wellDepth;
                        if (Result.wellborePressure[tIndex][gridIndex+1] < PressureWellHead)
                        {
                            breakWhilePUnderHead = true; errorWellCalculate=-errorInitial; break;
                        }

                        //如果是最井底，则计算气侵量;
                        //todo 注意这里进气点的位置：倒数第二个网格
                        if (gridIndex == WellGridNumber-2)
                        {
                            Result.gasInjection[tIndex][gridIndex] = GasInjectionClass.InjctionGasQuality(pressureFormation, Result.wellborePressure[tIndex][gridIndex+1]);
                            Result.mudInjection[tIndex][gridIndex] = Mud.Quality;
                        }
                        else
                        {
                            Result.gasInjection[tIndex][gridIndex] = 0;
                            Result.mudInjection[tIndex][gridIndex] = 0;
                        }

                        //如果井段压力计算误差大于设置值，继续迭代
                        double errorSegmentCalculate = errorInitial;
                        double kkSegmentL = 0,kkSegment = 1.0,kkSegmentR = 2.0;
                        while(Math.Abs(errorSegmentCalculate)>ErrorSegment)
                        {
                            
                            if (errorSegmentCalculate==errorInitial)
                            { kkSegmentL = 0.6;kkSegment = 0.8;kkSegmentR = 1.0; }
                            else if (Math.Abs(kkSegmentL-kkSegmentR)<errorMinimalStop)
                            {
                                kkSegmentL = 0;kkSegment = random.NextDouble();kkSegmentR = 1.0;
                                System.Diagnostics.Debug.Print(tIndex + "时刻"+gridIndex+"井段压力不收敛!");
                            }
                            else if(errorSegmentCalculate>0)
                            { kkSegmentR = kkSegment;kkSegment = (kkSegment + kkSegmentL) / 2; }
                            else
                            { kkSegmentL = kkSegment; kkSegment = (kkSegment + kkSegmentR) / 2; }

                            //假设井段压力值进行计算
                            Result.wellborePressure[tIndex][gridIndex] = Result.wellborePressure[tIndex][gridIndex+1] * kkSegment;
                            Result.wellboreTemperature[tIndex][gridIndex] = WellBore.wellBoreGrid[gridIndex].temperature;
                            Mud.setMudPt(Result.wellborePressure[tIndex][gridIndex], Result.wellboreTemperature[tIndex][gridIndex]);
                            Gas.setGasPT(Result.wellborePressure[tIndex][gridIndex], Result.wellboreTemperature[tIndex][gridIndex]);
                            Result.mudDensity[tIndex][gridIndex] = Mud.density;
                            Result.mudViscosity[tIndex][gridIndex] = Mud.viscosity;
                            Result.gasDensity[tIndex][gridIndex] = Gas.densityAtPt;
                            Result.gasViscosity[tIndex][gridIndex] = Gas.viscosity;
                            //System.Diagnostics.Debug.Print(gridIndex + "井段压力为：" + Result.wellborePressure[tIndex][gridIndex]);
                            //计算本段的含气率及液相速度

                            //如果上一个网格为单相流，则此网格也为单相流
                            if (gridIndex < WellGridNumber - 2 && Result.holdupGas[tIndex][gridIndex + 1] <0.0001)
                            {
                                Calculate.rheology = rheologys.singleMud;
                                Calculate.holdupGas = 0;
                                Result.holdupGas[tIndex][gridIndex] = 0;
                                Result.gasVolocity[tIndex][gridIndex] = 0;
                                Result.holdupLiquid[tIndex][gridIndex] = 1;
                                //要注意，使用的是上一个网格的截面积，注意网格划分顺序与计算顺序相反
                                Result.gasVolocity[tIndex][gridIndex] = ((Result.gasInjection[tIndex][gridIndex] / WellBore.wellBoreGrid[gridIndex + 1].GridArea - (Result.gasDensity[tIndex][gridIndex] * Result.holdupGas[tIndex][gridIndex] - Result.gasDensity[tIndex - 1][gridIndex] * Result.holdupGas[tIndex - 1][gridIndex]) * gridLength / timeStep)
                                    + Result.gasDensity[tIndex][gridIndex + 1] * Result.gasVolocity[tIndex][gridIndex + 1]) / Result.gasDensity[tIndex][gridIndex];
                            }
                            else
                            {
                                double errorHoldupCalculate = errorInitial;
                                double kkHoldupL = 0, kkHoldup = 0.2, kkHoldupR = 0.5;
                                double tensionInterFace = FlowHelpClass.CalculateTensionOfWaterAndGas(Result.wellborePressure[tIndex][gridIndex], Result.wellboreTemperature[tIndex][gridIndex]);
                                //如果含气率误差超出设置值，或者气相速度为负，则继续迭代
                                while (Math.Abs(errorHoldupCalculate)>ErrorHoldup || Result.holdupGas[tIndex][gridIndex]<0)
                                {
                                    //System.Diagnostics.Debug.Print(gridIndex + "井段含气率：" + kkHoldup);
                                    if (errorHoldupCalculate == errorInitial)
                                    { kkHoldupL = 0; kkHoldup = 0.25; kkHoldupR = 1; }
                                    else if(Math.Abs(kkHoldupL-kkHoldupR)<errorMinimalStop)
                                    {
                                        kkHoldupL = 0; kkHoldup = random.NextDouble(); kkHoldupR = 1;
                                        System.Diagnostics.Debug.Print(tIndex + "时刻" + gridIndex + "含气率不收敛!");
                                    }
                                    else if(errorHoldupCalculate>0)
                                    { kkHoldupR = kkHoldup;kkHoldup = (kkHoldup + kkHoldupL) / 2; }
                                    else
                                    { kkHoldupL = kkHoldup; kkHoldup = (kkHoldup + kkHoldupR) / 2; }

                                    Result.holdupGas[tIndex][gridIndex] = kkHoldup;
                                    Result.holdupLiquid[tIndex][gridIndex] =1-kkHoldup;
                                    
                                    //采用全隐格式、对流项取一阶迎风格式，求解流体流速
                                    Result.gasVolocity[tIndex][gridIndex] = ((Result.gasInjection[tIndex][gridIndex] / WellBore.wellBoreGrid[gridIndex + 1].GridArea - (Result.gasDensity[tIndex][gridIndex] * Result.holdupGas[tIndex][gridIndex] - Result.gasDensity[tIndex - 1][gridIndex] * Result.holdupGas[tIndex-1][gridIndex]) * gridLength / timeStep)
                                    + Result.gasDensity[tIndex][gridIndex + 1] * Result.gasVolocity[tIndex][gridIndex + 1]) / Result.gasDensity[tIndex][gridIndex];
                                    //如果计算气相速度小于0,说明气体回流，即含气率设置过大

                                    if (Math.Abs(Result.gasVolocity[tIndex][gridIndex]) < 0.001)
                                    { Result.gasVolocity[tIndex][gridIndex] = 0; }
                                    if (Result.gasVolocity[tIndex][gridIndex] < 0)
                                    { errorHoldupCalculate = 1; continue; }
                                    

                                    Result.mudVolocity[tIndex][gridIndex] = ((Result.mudInjection[tIndex][gridIndex] / WellBore.wellBoreGrid[gridIndex + 1].GridArea - (Result.mudDensity[tIndex][gridIndex]*Result.holdupLiquid[tIndex][gridIndex] - Result.mudDensity[tIndex - 1][gridIndex]*Result.holdupLiquid[tIndex-1][gridIndex]) * gridLength / timeStep)
                                    + Result.mudDensity[tIndex][gridIndex + 1] * Result.mudVolocity[tIndex][gridIndex + 1]) / Result.mudDensity[tIndex][gridIndex];

                                    Calculate.calculateHoldupGas(Result.gasVolocity[tIndex][gridIndex], Result.mudVolocity[tIndex][gridIndex], Result.gasDensity[tIndex][gridIndex], Result.mudDensity[tIndex][gridIndex],
                                        Result.gasViscosity[tIndex][gridIndex], Result.mudViscosity[tIndex][gridIndex], tensionInterFace);
                                    Result.rheology[tIndex][gridIndex] = Calculate.rheology;

                                    errorHoldupCalculate = Result.holdupGas[tIndex][gridIndex] - Calculate.holdupGas;

                                }
                            }

                            Result.wellboredPdZ[tIndex][gridIndex] = Calculate.calculatedPdZ(Result.gasVolocity[tIndex][gridIndex], Result.mudVolocity[tIndex][gridIndex], Result.gasDensity[tIndex][gridIndex], Result.mudDensity[tIndex][gridIndex],
                                        Result.gasViscosity[tIndex][gridIndex], Result.mudViscosity[tIndex][gridIndex]);
                            double f = Calculate.dPdZf;

                            //todo 差分动量方程
                            // 求解压力 % 瞬态项 % 对流项 全隐格式 迎风格式

                            double temp = Result.wellborePressure[tIndex][gridIndex + 1] - Gravity * gridLength * (Result.gasDensity[tIndex][gridIndex] * Result.holdupGas[tIndex][gridIndex] + Result.mudDensity[tIndex][gridIndex] * Result.holdupLiquid[tIndex][gridIndex]) - f * gridLength
                                - gridLength / timeStep * (Result.gasDensity[tIndex][gridIndex] * Result.gasVolocity[tIndex][gridIndex] + Result.mudDensity[tIndex][gridIndex] * Result.mudVolocity[tIndex][gridIndex] - Result.gasDensity[tIndex - 1][gridIndex] * Result.gasVolocity[tIndex - 1][gridIndex] - Result.mudDensity[tIndex - 1][gridIndex] * Result.mudVolocity[tIndex - 1][gridIndex]);
                            //    - Math.Pow(Result.gasDensity[tIndex][gridIndex] * Result.gasVolocity[tIndex][gridIndex], 2) / Result.holdupGas[tIndex][gridIndex] + Math.Pow(Result.mudDensity[tIndex][gridIndex] * Result.mudVolocity[tIndex][gridIndex], 2) / Result.holdupLiquid[tIndex][gridIndex]
                            //    - Math.Pow(Result.gasDensity[tIndex][gridIndex + 1] * Result.gasVolocity[tIndex][gridIndex + 1], 2) / Result.holdupGas[tIndex][gridIndex + 1] + Math.Pow(Result.mudDensity[tIndex][gridIndex + 1] * Result.mudVolocity[tIndex][gridIndex + 1], 2) / Result.holdupLiquid[tIndex][gridIndex + 1];  

                            errorSegmentCalculate = Result.wellborePressure[tIndex][gridIndex] - temp;
                        }
                    }
                    if (breakWhilePUnderHead == false)
                        errorWellCalculate = Result.wellborePressure[tIndex][0] - PressureWellHead;
                }
                t = t + timeStep;//todo 加上需要的步长
                tIndex = tIndex + 1;
            }
            MessageBox.Show("瞬态计算完成");
        }
    }
}
