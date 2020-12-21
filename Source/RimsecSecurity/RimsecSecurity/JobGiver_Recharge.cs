using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimsecSecurity
{
    class JobGiver_Recharge : ThinkNode_JobGiver
    {
        protected float minEnergyRechargePercentage = 100f;
        protected float minEnergyPowercellPercentage = 40f;
        
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Drafted) return null;
            if (pawn.needs.rest.CurLevel > (minEnergyRechargePercentage / 100f)) return null;
            var emptyChargeStation = PeacekeeperUtility.GetEmptyChargeStation(pawn);
            if (emptyChargeStation == null || emptyChargeStation.IsForbidden(pawn))
            {
                if (pawn.needs.rest.CurLevel < (minEnergyPowercellPercentage / 100f))
                {
                    var fuel = PeacekeeperUtility.FindBestFuel(pawn);
                    if (fuel != null) return JobMaker.MakeJob(RSDefOf.RSFuelRobot, pawn, fuel);
                }
                return JobMaker.MakeJob(JobDefOf.Wait, 120, true);
            }
            return JobMaker.MakeJob(RSDefOf.RSRecharge, emptyChargeStation);
        }
    }
}
