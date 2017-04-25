using CPWGI.MathHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPWGI.MultiphaseCalculate
{
    /// <summary>
    /// 使用Hasan-Kabir方法计算环空多项流流型及压降
    /// 1.调用setAnnular设置环空内外径 2.调用calculateHoldupGas计算含气率 3.调用calculatedPdZ计算单位压降
    /// </summary>
    public class CalculateMultiphaseHasanKabir:calculateMultiphaseBaseClass
    {     
        private double C0, C1, C2=1.0;//hansan-kabir方法中的系数
        private double K, Dh;

        /// <summary>
        /// 设置环空内外径并更新相关参数
        /// </summary>
        public override void setAnnular(double outterDiameter,double innerDiameter)
        {
            this.outterDiameter = outterDiameter;
            this.innerDiameter = innerDiameter;
            C0 = 1.2 + 0.371 * outterDiameter / innerDiameter;
            C1= 1.2 + 0.9 * outterDiameter / innerDiameter;
            K = innerDiameter / outterDiameter;
            Dh = outterDiameter - innerDiameter;
        }

        /// <summary>
        /// 判断当前条件下环空流态并返回含气率（需要先设置环空内外径）
        /// </summary>
        public override double calculateHoldupGas(double volocitySuperficalGas, double volocitySuperficalLiquid, double densityGas, double densityLiquid, double viscosityGas, double viscosityLiquid, double tensionInterface)
        {
            if (volocitySuperficalGas == 0)
            { holdupGas = 0;rheology = rheologys.bubble; return holdupGas; }

            double volocityMixture = volocitySuperficalGas + volocitySuperficalLiquid;
            double densityMixture;
            
            //单个气泡的极限上升速度
            double volocityBubbleFinal = 1.53 * Math.Pow(gravity * (densityLiquid - densityGas) * tensionInterface / densityLiquid / densityLiquid,0.25);
            //泰勒气泡的极限上升速度
            double volocityTaylerFinal = (0.30 + 0.22 * outterDiameter / innerDiameter) * Math.Sqrt(gravity * (outterDiameter - innerDiameter) * (densityLiquid - densityGas) / densityLiquid);

            //首先假设为泡状流进行计算
            double holdupGasBubble = volocitySuperficalGas / (1.2 * volocityMixture + volocityBubbleFinal);
            densityMixture = densityGas * holdupGasBubble + densityLiquid * holdupLiquid;

            //判断准则
            bool principleDisperse = Math.Pow(volocityMixture, 1.12) > 5.88 * Math.Pow(outterDiameter - innerDiameter, 0.48)
                * Math.Sqrt(gravity * (densityLiquid - densityGas) / tensionInterface) * Math.Pow(tensionInterface / densityLiquid, 0.6) * Math.Pow(densityMixture / viscosityLiquid, 0.08);
            bool principleSlug;
            if (densityLiquid * volocitySuperficalLiquid * volocitySuperficalLiquid - 74.4 > 0)
                principleSlug = densityGas * volocitySuperficalGas * volocitySuperficalGas < 25.4 * Math.Log10(densityLiquid * volocitySuperficalLiquid * volocitySuperficalLiquid) - 38.9;
            else
                principleSlug = densityGas * volocitySuperficalGas * volocitySuperficalGas < 0.0051 * Math.Pow(densityLiquid * volocitySuperficalLiquid * volocitySuperficalLiquid, 1.7);
            bool principleChurn = volocitySuperficalGas < 3.1 * Math.Pow(tensionInterface * gravity * (densityLiquid - densityGas) / densityGas / densityGas, 0.25);

            //判断流态,注意判断顺序
            if (holdupGasBubble <= 0.25 && volocityBubbleFinal < volocityTaylerFinal)
            {
                rheology = rheologys.bubble;
                holdupGas = volocitySuperficalGas / (C0 * volocityMixture + volocityBubbleFinal);
            }
            if ((holdupGasBubble > 0.25 && principleSlug) || volocityBubbleFinal > volocityTaylerFinal)
            {
                rheology = rheologys.slug;
                holdupGas = volocitySuperficalGas / (C1 * volocityMixture + volocityTaylerFinal);
            }
            if (holdupGasBubble < 0.52 && principleDisperse)
            {
                rheology = rheologys.disperedBubble;
                holdupGas = volocitySuperficalGas / (C0 * volocityMixture + volocityBubbleFinal);
            }
            if (principleChurn && !principleSlug)
            {
                rheology = rheologys.churn;
                holdupGas = volocitySuperficalGas / (C2 * volocityMixture + volocityTaylerFinal);
            }
            if (!principleChurn)
            {
                rheology = rheologys.annular;
                holdupGas = 1.0;
            }

            return holdupGas;
        }

        /// <summary>
        /// 计算当前条件下环空单位压降（请先调用calculateHoldupGas计算含气率）
        /// </summary>
        public override double calculatedPdZ(double volocitySuperficalGas, double volocitySuperficalLiquid, double densityGas, double densityLiquid,double viscosityGas,double viscosityLiquid)
        {
            //环状流单独进行处理
            if (rheology == rheologys.annular)
                //待完善
            { dPdZ = 0; }
            else
            {
                double volocityMixture = volocitySuperficalGas + volocitySuperficalLiquid;
                double densityMixure = densityGas * holdupGas + densityLiquid * holdupLiquid;
                double viscosityMixture = viscosityGas * holdupGas + viscosityLiquid * holdupLiquid;
                double frictionFactor = calculateFrcitionFactorColebrook(volocityMixture, densityMixure, viscosityGas, viscosityLiquid, holdupGas);
                //单位摩阻压降
                double dPdZfriction;
                //泡状流
                if (rheology == rheologys.disperedBubble || rheology == rheologys.bubble)
                    dPdZfriction = 2 * frictionFactor * volocityMixture * volocityMixture * densityMixure / (outterDiameter - innerDiameter);
                //段塞流和搅拌流
                else
                    dPdZfriction = 2 * frictionFactor * volocityMixture * volocityMixture * densityMixure / (outterDiameter - innerDiameter)*(1-holdupGas);
                dPdZ = dPdZfriction + densityMixure * gravity;
            }
            return dPdZ;
        }

        /// <summary>
        /// 计算范宁摩阻系数
        /// </summary>
        private double calculateFrictionFanningFactor(double volocityMixture, double densityMixture, double viscosityGas, double viscosityLiquid, double holdupGas)
        {
            double frictionFactor = 0;
            double NRe = densityMixture * volocityMixture * Dh / (viscosityGas * holdupGas + viscosityLiquid * (1 - holdupGas));
            double Fp = 16 / NRe;
            double Fca = 16 * (1 - K) * (1 - K) / ((1 - K * K * K * K) / (1 - K * K) - (1 - K * K) / Math.Log(1 / K));
            double temp = Math.Pow(Fp / Fca, 0.45 * Math.Exp(-(NRe - 3000) / 1000000));
            //使用牛顿下山法解摩阻系数
            Func<double, double> fx = (fF) => Math.Pow(fF * temp, -0.5) - 4 * Math.Log10(NRe * Math.Sqrt(fF * temp)) + 0.4;
            Func<double, double> dfx = (fF) => -0.5 * temp * Math.Pow(fF * temp, -1.5) - 2 / Math.Log(10) / fF;
            frictionFactor = MathSolveEquations.SolveEquationsWithNewtonDownHillMothod(fx, dfx, 0.1,frictionFactorError);
            return frictionFactor;
        }
        
        private double calculateFrcitionFactorColebrook(double volocityMixture, double densityMixture, double viscosityGas, double viscosityLiquid, double holdupGas)
        {
            double frictionFactor = 0;
            double roughness = 0.00033*0.001;
            double NRe = densityMixture * volocityMixture * Dh / (viscosityGas * holdupGas + viscosityLiquid * (1 - holdupGas));
            Func<double, double> fx = (f) => Math.Pow(f, -0.5) + 4 * Math.Log10(roughness / 3.7 / Dh + 1.255 / NRe / Math.Pow(f, 0.5));
            Func<double, double> dfx = (f) => -0.5 * Math.Pow(f, -1.5) - 2.51 / NRe * Math.Pow(f, -1.5) / (roughness / 3.7 / Dh + 1.255 / NRe / Math.Pow(f, 0.5)) / Math.Log(10);
            frictionFactor = MathSolveEquations.SolveEquationsWithNewtonDownHillMothod(fx, dfx, 0.001);
            return frictionFactor;
        }
    }
}
