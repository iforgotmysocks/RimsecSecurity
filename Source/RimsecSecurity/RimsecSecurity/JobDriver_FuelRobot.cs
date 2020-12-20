using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimsecSecurity
{
    class JobDriver_FuelRobot : JobDriver
    {
        private const TargetIndex RefuelableInd = TargetIndex.A;
        private const TargetIndex FuelInd = TargetIndex.B;
        private const int RefuelingDuration = 240;
        protected Pawn Refuelable => this.job.GetTarget(TargetIndex.A).Pawn;
        protected Thing Fuel => this.job.GetTarget(TargetIndex.B).Thing;

        private IntVec3 startingPos;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => 
            this.pawn.Reserve(this.Refuelable, this.job, 1, -1, null, errorOnFailed) 
            && this.pawn.Reserve(this.Fuel, this.job, 1, -1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.startingPos = Refuelable.Position;
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            base.AddEndCondition(delegate
            {
                if (Refuelable.needs.rest.CurLevel < 0.9 || (Refuelable == pawn && pawn.Position != startingPos && pawn.CanReach(startingPos, PathEndMode.OnCell, Danger.Deadly)))
                {
                    return JobCondition.Ongoing;
                }
                return JobCondition.Succeeded;
            });
            yield return Toils_General.DoAtomic(delegate
            {
                this.job.count = Convert.ToInt32((Refuelable.needs.rest.MaxLevel - Refuelable.needs.rest.CurLevel) * 10);
            }).FailOn(() => job.count == 0);
            //base.AddFailCondition(() => !this.job.playerForced);
            Toil reserveFuel = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
            yield return reserveFuel;
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, TargetIndex.B, TargetIndex.None, true, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(240, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            Toil toil = new Toil();
            toil.initAction = delegate ()
            {
                //Log.Message($"current rest: {Refuelable.needs.rest.CurLevel} stackcount: {Fuel.stackCount} calced {(Fuel.stackCount / 100f)}");
                Refuelable.needs.rest.CurLevel += (Fuel.stackCount / 10f);
                Fuel.Destroy();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
            yield return Toils_Goto.GotoCell(startingPos, PathEndMode.OnCell).FailOn(() => this.pawn != Refuelable);
            yield break;
        }

    }
}
