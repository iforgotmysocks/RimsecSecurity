using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimsecSecurity
{
    class JobDriver_TakeToRechargeStation : JobDriver
    {
        protected Pawn Takee => this.job.GetTarget(TargetIndex.A).Pawn;
        protected Thing DropBed => this.job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => 
            this.pawn.Reserve(this.Takee, this.job, 1, -1, null, errorOnFailed) 
            && this.pawn.Reserve(this.DropBed, this.job, 1, 0, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnAggroMentalStateAndHostile(TargetIndex.A);
            this.FailOn(delegate ()
            {
                if (this.job.def.makeTargetPrisoner)
                {
                    Log.Message($"tried to make robot prisoner in {this.GetType().Name}");
                    return true;
                }
                return false;
            });
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOn(() => this.job.def == JobDefOf.Arrest && !this.Takee.CanBeArrestedBy(this.pawn))
                .FailOn(() => !this.pawn.CanReach(this.DropBed, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                .FailOn(() => this.job.def == RSDefOf.RSRescueToChargeStation && !this.Takee.Downed)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.A);

            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            yield return Toils_Reserve.Release(TargetIndex.B);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    if (pawn.carryTracker.TryDropCarriedThing(((Building_ChargeStation)DropBed)
                        .GetStandPosition(Takee), ThingPlaceMode.Direct, out var thing, null))
                        ((Building_ChargeStation)DropBed).CurrentRobot = Takee;
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }

    }
}
