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
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Drafted) return null;
            var emptyChargeStation = PeacekeeperUtility.GetEmptyChargeStation(pawn);
            if (emptyChargeStation == null || emptyChargeStation.IsForbidden(pawn)) return JobMaker.MakeJob(JobDefOf.Wait, 120, true);
            return JobMaker.MakeJob(RSDefOf.RSRecharge, emptyChargeStation);
        }
    }
}
