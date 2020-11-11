using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimsecSecurity
{
#pragma warning disable CS0649
    class CompProperties_RechargeRobot : CompProperties
    {
        public CompProperties_RechargeRobot()
        {
            this.compClass = typeof(CompRechargeRobot);
        }

        public float energyPerSecond;
        public int injuryHealCount;
        public float injuryHealAmountPer30s;
        public int ticksHealPermanent;
        public int ticksRestorePart;

        public float repairCostPerPointOfDamage = 0.05f;
        public float repairCostPermanent = 0.5f;
        public float repairCostMissing = 1f;
        public float repairCostMax = 5f;
    }
}
