using CPWGI.MathHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPWGI.MultiphaseCalculate
{
    public class CalculateMultiphasePerez : calculateMultiphaseBaseClass
    {
        private double K, Dh, Dep;
        
        /// <summary>
        /// 计算当前条件下环空单位压降（请先调用calculateHoldupGas计算含气率）
        /// </summary>
        public override double calculatedPdZ(double volocitySuperficalGas, double volocitySuperficalLiquid, double densityGas, double densityLiquid, double viscosityGas, double viscosityLiquid)
        {
            dPdZ = 0;
            double dPdZfriction = 0;
            switch (rheology)
            {
                case rheologys.disperedBubble:
                case rheologys.bubble:
                    double volocityMixture = volocitySuperficalGas + volocitySuperficalLiquid;
                    double densityMixture = densityGas * holdupGas + densityLiquid * holdupLiquid;
                    double frictionFanningFactor = CalculateFrictionFactor(volocityMixture, densityMixture, viscosityGas, viscosityLiquid, holdupGas);
                    dPdZfriction = 2 * frictionFanningFactor * densityMixture * volocityMixture * volocityMixture / Dh;
                    dPdZ = dPdZfriction + densityMixture * gravity;
                    break;
                case rheologys.slug:
                    break;
                case rheologys.churn:
                    break;
                case rheologys.annular:
                    break;
                case rheologys.unknown:
                    break;
            }
            dPdZf = dPdZfriction;
            return dPdZ;
        }

        /// <summary>
        /// 判断当前条件下环空流态并返回含气率（需要先设置环空内外径）
        /// </summary>
        public override double calculateHoldupGas(double volocitySuperficalGas, double volocitySuperficalLiquid, double densityGas, double densityLiquid, double viscosityGas, double viscosityLiquid, double tensionInterface)
        {
            if (volocitySuperficalGas == 0)
            { holdupGas = 0; rheology = rheologys.bubble; return holdupGas; }

            double volocityMixture = volocitySuperficalGas + volocitySuperficalLiquid;
            double holdupLiquidNoneSlip = volocitySuperficalLiquid / volocityMixture;
            double densityMixtureNoneSlip = densityGas * (1 - holdupLiquidNoneSlip) + densityLiquid * holdupLiquidNoneSlip;
            double frictioinFNoneslip=calculateFrictionFanningFactor(volocityMixture,densityMixtureNoneSlip,viscosityGas,viscosityLiquid,1-holdupLiquidNoneSlip);
            double volocityBubbuleFinal = 1.53 * Math.Pow((densityLiquid - densityGas) * gravity * tensionInterface / densityLiquid / densityLiquid, 0.25);
            double volocityTaylerFinal = 0.345 * Math.Sqrt(gravity * Dep);
            bool principleMinDiameter = (Dep >= 19.7 * Math.Sqrt((densityLiquid - densityGas) * tensionInterface / gravity / densityLiquid / densityLiquid));
            //perez论文25页公式3.20，取C0=1.2,n=0.5,其中holdupgas=0.20,但之前多数学者建议0.25
            bool principleBubble = volocitySuperficalLiquid > 3.167 * volocitySuperficalGas - 0.745 *volocityBubbuleFinal;
            bool principleDispersedBubble = Math.Sqrt(1.6 * tensionInterface / (densityLiquid - densityGas) / gravity) * Math.Pow(densityLiquid / tensionInterface, 0.6) * Math.Pow(2 * frictioinFNoneslip / Dh, 0.4) * Math.Pow(volocityMixture, 1.2) >
                0.725 + 4.15 * Math.Sqrt(volocitySuperficalGas / volocityMixture);
            bool principleDispersedToSlug = volocitySuperficalLiquid < 0.923 * volocitySuperficalGas;
            bool principleChurn = volocitySuperficalLiquid < 0.0684 * volocitySuperficalGas - 0.292 * Math.Sqrt(gravity * Dep);
            bool principleAnnular = volocitySuperficalGas > 3.1 * Math.Pow((densityLiquid - densityGas) * gravity * tensionInterface / densityGas / densityGas, 0.25);
            if (principleMinDiameter && principleBubble)
            {
                rheology = rheologys.bubble;
                holdupGas = calculateHoldupGasWhenBubble(volocitySuperficalGas, volocitySuperficalLiquid, volocityBubbuleFinal, volocityMixture);
            }
            if(!principleBubble)
            {
                rheology = rheologys.slug;
            }
            if (principleDispersedBubble && !principleDispersedToSlug)
            {
                rheology = rheologys.disperedBubble;
                holdupGas = 1 - holdupLiquidNoneSlip;
            }
            if (principleChurn)
            {
                rheology = rheologys.churn;
            }
            if(principleAnnular)
            {
                rheology = rheologys.annular;
            }
            return holdupGas;
        }

        /// <summary>
        /// 根据不同方法计算摩阻系数
        /// </summary>
        private double CalculateFrictionFactor(double volocityMixture, double densityMixture, double viscosityGas, double viscosityLiquid, double holdupGas)
        {
            double f = 0;
            switch (CalFMethod)
            {
                case CalFrictionMethods.Fanning:
                    f = calculateFrictionFanningFactor(volocityMixture, densityMixture, viscosityGas, viscosityLiquid, holdupGas);
                    break;
                case CalFrictionMethods.Corebook:
                    f = calculateFrcitionFactorColebrook(volocityMixture, densityMixture, viscosityGas, viscosityLiquid, holdupGas);
                    break;
            }
            return f;
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
            frictionFactor = MathSolveEquations.SolveEquationsWithNewtonDownHillMothod(fx, dfx, 0.1, frictionFactorError);
            return frictionFactor;
        }

        /// <summary>
        /// 使用Colebrook公式计算摩阻系数
        /// </summary>
        private double calculateFrcitionFactorColebrook(double volocityMixture, double densityMixture, double viscosityGas, double viscosityLiquid, double holdupGas)
        {
            double frictionFactor = 0;
            double roughness = 0.00033 * 0.001;
            double NRe = densityMixture * volocityMixture * Dh / (viscosityGas * holdupGas + viscosityLiquid * (1 - holdupGas));
            Func<double, double> fx = (f) => Math.Pow(f, -0.5) + 4 * Math.Log10(roughness / 3.7 / Dh + 1.255 / NRe / Math.Pow(f, 0.5));
            Func<double, double> dfx = (f) => -0.5 * Math.Pow(f, -1.5) - 2.51 / NRe * Math.Pow(f, -1.5) / (roughness / 3.7 / Dh + 1.255 / NRe / Math.Pow(f, 0.5)) / Math.Log(10);
            frictionFactor = MathSolveEquations.SolveEquationsWithNewtonDownHillMothod(fx, dfx, 0.001, frictionFactorError);
            return frictionFactor;
        }


        /// <summary>
        /// 使用牛顿下山法计算泡状流含气率
        /// </summary>
        private double calculateHoldupGasWhenBubble(double volocitySuperficalGas,double volocitySuperficalLiquid,double volocityBubbleFinal,double volocityMixture)
        {
            double n = 0.5, C0 = 1.2;
            double holdupLiquidBubble = 0;
            Func<double,double> f=(HL)=> Math.Pow(HL, n) * volocityBubbleFinal - volocitySuperficalGas / (1 - HL) + C0 * volocityMixture;
            Func<double,double> df=(HL)=> n * Math.Pow(HL, n - 1) * volocityBubbleFinal - volocitySuperficalGas / (1 - HL) / (1 - HL);
            holdupLiquidBubble=MathSolveEquations.SolveEquationsWithNewtonDownHillMothod(f, df, 0.9,holdupQasError);
            return 1 - holdupLiquidBubble;
        }

        /// <summary>
        /// 设置环空内外径并更新相关参数
        /// </summary>
        public override void setAnnular(double outterDiameter, double innerDiameter)
        {
            this.outterDiameter = outterDiameter;
            this.innerDiameter = innerDiameter;
            
            K = innerDiameter / outterDiameter;
            Dh = outterDiameter - innerDiameter;
            Dep = outterDiameter + innerDiameter;
        }
    }
}
