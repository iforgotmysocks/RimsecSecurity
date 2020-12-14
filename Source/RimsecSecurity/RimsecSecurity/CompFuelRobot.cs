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
    class CompFuelRobot : ThingComp
    {
        public Pawn Parent { get => parent as Pawn; }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (Parent == null || Parent.Dead) yield break;
            var acceptanceReport = this.CanRefuel(selPawn, null);

            yield return new FloatMenuOption("RSFuelRobotFloatMenu".Translate(), delegate ()
            {
                var job = PeacekeeperUtility.RefuelJob(selPawn, Parent);
                job.count = 1;
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }, MenuOptionPriority.Default, null, null, 0f, null, null)
            {
                Disabled = !acceptanceReport.Accepted,
                revalidateClickTarget = Parent,
            };
        }

        private AcceptanceReport CanRefuel(Pawn pawn, object p)
        {
            if (!Parent.Map.itemAvailability.ThingsAvailableAnywhere(new ThingDefCountClass(RSDefOf.RSPowerCell, 1), pawn)) return new AcceptanceReport("No fuel available");
            return AcceptanceReport.WasAccepted;
        }
    }
}
