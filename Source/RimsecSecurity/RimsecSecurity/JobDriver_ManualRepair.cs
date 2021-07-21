using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimsecSecurity
{
    class JobDriver_ManualRepair : JobDriver
    {
        protected Building_ChargeStation Station => this.job.GetTarget(TargetIndex.A).Thing as Building_ChargeStation;
        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            this.pawn.Reserve(this.job.targetB, this.job, 1, -1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOn(() => this.pawn.Drafted).FailOnDespawnedNullOrForbidden(TargetIndex.B);
            yield return Toils_General.Wait(Station.CurrentRobot.def.GetModExtension<RSPeacekeeperModExt>().repairTicks, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f).WithEffect(EffecterDefOf.ConstructMetal, TargetIndex.A);
            yield return new Toil
            {
                initAction = () =>
                {
                    FullyRepair(Station.CurrentRobot);
                    Station.CompFuel.ConsumeFuel(Station.CompRecharge.ComponentsForManualRepair);
                }
            }.FailOn(() => Station.CurrentRobot == null || Station.CompRecharge.ComponentsForManualRepair == 0);
        }

        private void FullyRepair(Pawn currentRobo)
        {
            FleckMaker.ThrowDustPuffThick(currentRobo.Position.ToVector3(), currentRobo.Map, Rand.Range(1.5f, 3f), new UnityEngine.Color(1f, 1f, 1f, 2.5f));
            foreach (var hediff in currentRobo.health.hediffSet.hediffs.Reverse<Hediff>())
            {
                if (hediff is Hediff_Injury || hediff is Hediff_MissingPart) HealthUtility.Cure(hediff);
            }
        }

    }
}
