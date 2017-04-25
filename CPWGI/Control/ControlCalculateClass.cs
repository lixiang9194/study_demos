using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPWGI.Model;
using CPWGI.MultiphaseCalculate;

namespace CPWGI.Control
{
    public class ControlCalculateClass
    {
        public WellBoreClass WellBore { get; set; }
        public DrillMudClass Mud { get; set; }
        public GasClass Gas { get; set; }
        public CalculateMultiphaseHasanKabir Calculate { get; set; }
        public ResultMatrix Result { get; set; }
        public int WellGridNumber { get; set; }
        public int ttt { get; }
        //todo井口回压在这里添加并不合适
        public double PressureWellHead { get; set; }
        public double ErrorWell = 10000, ErrorSegment = 1000, ErrorHoldup = 0.0001;
        public static double Gravity = 9.81;

        public ControlCalculateClass(WellBoreClass well,DrillMudClass mud,GasClass gas,CalculateMultiphaseHasanKabir cal,ResultMatrix result)
        {
            this.WellBore = well;
            this.Mud = mud;
            this.Gas = gas;
            this.Calculate = cal;
            this.Result = result;
            WellGridNumber = WellBore.wellBoreGrid.Count;
            PressureWellHead = 4000000;
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

            }
        }

        public void CalculateSteadyT(double t,int tIndex)
        {
            Result.addTime(t,WellGridNumber);
            double GridLength = 0;
            //沿井身网格向下计算
            for (int i = 1; i < WellGridNumber; i++)
            {
                Result.wellborePressure[tIndex][0] = PressureWellHead;
                Calculate.setAnnular(WellBore.wellBoreGrid[i].outterDiameter, WellBore.wellBoreGrid[i].innerDiameter);
                Console.WriteLine(i);
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
                    Result.wellborePressure[tIndex][i] = Result.wellborePressure[tIndex][i - 1] + PressuredH;
                    PressureAverage = Result.wellborePressure[tIndex][i] / 2 + Result.wellborePressure[tIndex][i - 1] / 2;
                    TemperatureAverage = Result.wellboreTemperature[tIndex][i] / 2 + Result.wellboreTemperature[tIndex][i - 1] / 2;
                    Mud.setMudPt(PressureAverage, TemperatureAverage);
                    Result.mudDensity[tIndex][i] = Mud.density;
                    Result.mudViscosity[tIndex][i] = Mud.viscosity;
                    Result.mudVolocity[tIndex][i] = Mud.Volocity / WellBore.wellBoreGrid[i].GridArea;
                    Result.holdupGas[tIndex][i] = Calculate.calculateHoldupGas(0, Result.mudVolocity[tIndex][i], 0, Result.mudDensity[tIndex][i], 0, Result.mudViscosity[tIndex][i], 0);
                    Result.wellboredPdZ[tIndex][i] = Calculate.calculatedPdZ(0, Result.mudVolocity[tIndex][i], 0, Result.mudDensity[tIndex][i], 0, Result.mudViscosity[tIndex][i]);
                    ErrorSegmentCal = Result.wellboredPdZ[tIndex][i] * GridLength - PressuredH;
                }
            }
        }
    }
}
