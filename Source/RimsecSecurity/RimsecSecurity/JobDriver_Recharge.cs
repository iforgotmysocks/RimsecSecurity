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
    class JobDriver_Recharge : JobDriver
    {
        protected Building_ChargeStation Station => this.job.GetTarget(TargetIndex.A).Thing as Building_ChargeStation;
        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, Station.GetStandPosition(this.pawn)).FailOn(() => this.pawn.Drafted).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            var toil = new Toil();
            toil.defaultDuration = Rand.Range(180, 240);
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.handlingFacing = true;
            toil.initAction = delegate ()
            {
                base.Map.pawnDestinationReservationManager.Reserve(this.pawn, this.job, this.pawn.Position);
                this.pawn.pather.StopDead();
                Station.CurrentRobot = pawn;
            };
            toil.tickAction = delegate ()
            {
                if (ticksLeftThisToil <= 0 && pawn.needs.rest.CurLevel >= 0.99f)
                {
                    base.ReadyForNextToil();
                    return;
                }
                if (this.job.expiryInterval == -1 && this.job.def == JobDefOf.Wait_Combat && !this.pawn.Drafted)
                {
                    Log.Error(this.pawn + " in eternal WaitCombat without being drafted.");
                    base.ReadyForNextToil();
                    return;
                }
                if ((Find.TickManager.TicksGame + this.pawn.thingIDNumber) % 4 == 0)
                {
                    this.CheckForAutoAttack();
                }
                var pos = pawn.Position;
                pos.z -= 1;
                this.pawn.rotationTracker.FaceCell(pos);
            };
            yield return toil;
        }

        public override void Notify_StanceChanged()
        {
            if (this.pawn.stances.curStance is Stance_Mobile)
            {
                this.CheckForAutoAttack();
            }
        }

        private void CheckForAutoAttack()
        {
            if (this.pawn.Downed)
            {
                return;
            }
            if (this.pawn.stances.FullBodyBusy)
            {
                return;
            }
            this.collideWithPawns = false;
            bool flag = !this.pawn.WorkTagIsDisabled(WorkTags.Violent);
            bool flag2 = this.pawn.RaceProps.ToolUser && this.pawn.Faction == Faction.OfPlayer && !this.pawn.WorkTagIsDisabled(WorkTags.Firefighting);
            if (flag || flag2)
            {
                Fire fire = null;
                for (int i = 0; i < 9; i++)
                {
                    IntVec3 c = this.pawn.Position + GenAdj.AdjacentCellsAndInside[i];
                    if (c.InBounds(this.pawn.Map))
                    {
                        List<Thing> thingList = c.GetThingList(base.Map);
                        for (int j = 0; j < thingList.Count; j++)
                        {
                            if (flag)
                            {
                                Pawn pawn = thingList[j] as Pawn;
                                if (pawn != null && !pawn.Downed && this.pawn.HostileTo(pawn) && GenHostility.IsActiveThreatTo(pawn, this.pawn.Faction))
                                {
                                    this.pawn.meleeVerbs.TryMeleeAttack(pawn, null, false);
                                    this.collideWithPawns = true;
                                    return;
                                }
                            }
                            if (flag2)
                            {
                                Fire fire2 = thingList[j] as Fire;
                                if (fire2 != null && (fire == null || fire2.fireSize < fire.fireSize || i == 8) && (fire2.parent == null || fire2.parent != this.pawn))
                                {
                                    fire = fire2;
                                }
                            }
                        }
                    }
                }
                if (fire != null && (!this.pawn.InMentalState || this.pawn.MentalState.def.allowBeatfire))
                {
                    this.pawn.natives.TryBeatFire(fire);
                    return;
                }
                if (flag && this.job.canUseRangedWeapon && this.pawn.Faction != null && this.job.def == JobDefOf.Wait_Combat && (this.pawn.drafter == null || this.pawn.drafter.FireAtWill))
                {
                    Verb currentEffectiveVerb = this.pawn.CurrentEffectiveVerb;
                    if (currentEffectiveVerb != null && !currentEffectiveVerb.verbProps.IsMeleeAttack)
                    {
                        TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
                        if (currentEffectiveVerb.IsIncendiary())
                        {
                            targetScanFlags |= TargetScanFlags.NeedNonBurning;
                        }
                        Thing thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(this.pawn, targetScanFlags, null, 0f, 9999f);
                        if (thing != null)
                        {
                            this.pawn.TryStartAttack(thing);
                            this.collideWithPawns = true;
                            return;
                        }
                    }
                }
            }
        }

        private const int TargetSearchInterval = 4;

    }
}
