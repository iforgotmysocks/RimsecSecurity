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
                Job job = RefuelJob(selPawn, Parent);
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
            if (!Parent.Map.itemAvailability.ThingsAvailableAnywhere(new ThingDefCountClass(ThingDefOf.Chemfuel, 1), pawn)) return new AcceptanceReport("No fuel available");
            return AcceptanceReport.WasAccepted;
        }

        public static Job RefuelJob(Pawn pawn, Thing t, bool forced = false, JobDef customRefuelJob = null, JobDef customAtomicRefuelJob = null)
        {
            Thing t2 = FindBestFuel(pawn, t);
            return JobMaker.MakeJob(customRefuelJob ?? RSDefOf.RSFuelRobot, t, t2);
        }

        private static Thing FindBestFuel(Pawn pawn, Thing refuelable)
        {
            var filter = new ThingFilter();
            filter.SetAllow(ThingDefOf.Chemfuel, true);
            Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && filter.Allows(x);
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, filter.BestThingRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        }


    }
}
