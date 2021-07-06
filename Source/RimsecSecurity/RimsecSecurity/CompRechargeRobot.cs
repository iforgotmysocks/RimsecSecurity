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
    class CompRechargeRobot : ThingComp
    {
        private int ticksCharge = -1;
        private int ticksHeal = -1;
        private int ticksHealPermanent = -1;
        private int ticksRestorePart = -1;
        private float componentsForManualRepair = 0f;
        private float availableComponents = 0f;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksCharge, "ticksCharge", -1);
            Scribe_Values.Look(ref ticksHeal, "ticksHeal", -1);
            Scribe_Values.Look(ref ticksHealPermanent, "ticksHealPermanent", -1);
            Scribe_Values.Look(ref ticksRestorePart, "ticksRestorePart", -1);
            Scribe_Values.Look(ref componentsForManualRepair, "componentsForManualRepair", 0f);
            Scribe_Values.Look(ref availableComponents, "availableComponents", 0f);
        }
        public CompProperties_RechargeRobot Props => (CompProperties_RechargeRobot)props;
        public float ComponentsForManualRepair { get => componentsForManualRepair; set => componentsForManualRepair = value; }
        public float AvailableComponents { get => availableComponents; set => availableComponents = value; }

        private Building_ChargeStation cachedParent;
        public Building_ChargeStation Parent => cachedParent ?? (cachedParent = this.parent as Building_ChargeStation);

        public override void CompTick()
        {
            if (Parent == null || Parent.PowerOff()) return;
            if (ticksCharge >= 60)
            {
                if (RobotTreatable())
                {
                    ComponentsForManualRepair = CalculateManualRepairCost();
                    AvailableComponents = Parent.CompFuel.Fuel;

                    Parent.CurrentRobot.needs.rest.CurLevel = (Parent.CurrentRobot.needs.rest.CurLevel + Props.energyPerSecond) > Parent.CurrentRobot.needs.rest.MaxLevel ? Parent.CurrentRobot.needs.rest.MaxLevel : Parent.CurrentRobot.needs.rest.CurLevel + Props.energyPerSecond;
                }
                ticksCharge = 0;
            }

            if (ticksHeal >= 1799 && Parent?.CompFuel?.HasFuel == true)
            {
                if (RobotTreatable())
                {
                    var foundRobotConsciousness = false;
                    var injuriesTreatedCount = 0;
                    foreach (var hediff in Parent.CurrentRobot.health.hediffSet.hediffs)
                    {
                        if (hediff.def == RSDefOf.RSRobotConsciousness) foundRobotConsciousness = true;
                        var injury = hediff as Hediff_Injury;
                        if (injury == null || injury.IsPermanent()) continue;
                        if (injury.IsTended())
                        {
                            injury.Heal(Props.injuryHealAmountPer30s);
                            if (injuriesTreatedCount++ > Props.injuryHealCount) break;
                        }
                        else injury.Tended(1f, 1f);
                    }

                    if (!foundRobotConsciousness)
                    {
                        var hediff = HediffMaker.MakeHediff(RSDefOf.RSRobotConsciousness, Parent.CurrentRobot);
                        Parent.CurrentRobot.health.AddHediff(hediff, Parent.CurrentRobot.health.hediffSet.GetBrain(), null, null);
                    }
                }
                ticksHeal = 0;
            }

            if (ticksHealPermanent >= Props.ticksHealPermanent && Parent?.CompFuel?.HasFuel == true)
            {
                if (RobotTreatable())
                {
                    var permInjury = Parent.CurrentRobot.health.hediffSet.hediffs?.OfType<Hediff_Injury>()?.InRandomOrder()?.FirstOrDefault(hediff => hediff.IsPermanent());
                    if (permInjury != null) HealthUtility.Cure(permInjury);
                }
                ticksHealPermanent = 0;
            }

            if (ticksRestorePart >= Props.ticksRestorePart && Parent?.CompFuel?.HasFuel == true)
            {
                if (RobotTreatable())
                {
                    var missingPart = Parent.CurrentRobot.health.hediffSet.hediffs?.OfType<Hediff_MissingPart>()?.InRandomOrder()?.FirstOrDefault();
                    if (missingPart != null) HealthUtility.Cure(missingPart);
                }
                ticksRestorePart = 0;
            }

            ticksCharge++;
            ticksHeal++;
            ticksHealPermanent++;
            ticksRestorePart++;
        }

        private bool RobotTreatable() => Parent.CurrentRobot != null && !Parent.CurrentRobot.Dead && PeacekeeperUtility.IsPeacekeeper(Parent.CurrentRobot);

        private Pawn GetCurrentPawn() => this.parent.Position.GetFirstPawn(this.parent.Map) ?? (new IntVec3(parent.Position.x, parent.Position.y, parent.Position.z + 1).GetFirstPawn(this.parent.Map));

        private float CalculateManualRepairCost()
        {
            if (Parent?.CurrentRobot?.health?.hediffSet == null) return 0;
            var total = 0f;
            foreach (var hediff in Parent.CurrentRobot.health.hediffSet.hediffs)
            {
                var injury = hediff as Hediff_Injury;
                if (injury != null)
                {
                    if (injury.IsPermanent()) total += Props.repairCostPermanent;
                    else total += injury.Severity * Props.repairCostPerPointOfDamage;
                    continue;
                }
                var missingPart = hediff as Hediff_MissingPart;
                if (missingPart != null) total += Props.repairCostMissing;
            }

            return total > Props.repairCostMax ? Props.repairCostMax : total;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var baseOption in base.CompFloatMenuOptions(selPawn)) yield return baseOption;

            if (!RobotTreatable() || Parent.PowerOff() || ComponentsForManualRepair == 0) yield break;

            AcceptanceReport acceptanceReport = CanRepairRobo(selPawn);
            var text = "RSRepairManually".Translate();
            if (!acceptanceReport.Accepted && !string.IsNullOrWhiteSpace(acceptanceReport.Reason))
            {
                text = text + ": " + acceptanceReport.Reason;
            }

            yield return new FloatMenuOption(text, delegate ()
            {
                RepairManually(selPawn);
            }, MenuOptionPriority.Default, null, null, 0f, null, null)
            {
                Disabled = !acceptanceReport.Accepted,
                revalidateClickTarget = this.parent,
            };
        }

        public AcceptanceReport CanRepairRobo(Pawn pawn)
        {
            if (pawn.Dead || pawn.Faction != Faction.OfPlayer) return false;
            if (!pawn.CanReach(this.parent, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn)) return new AcceptanceReport("can't reach");
            if (!pawn.Map.reservationManager.CanReserve(pawn, Parent.CurrentRobot, 1, -1, null, false))
            {
                Pawn pawn2 = pawn.Map.reservationManager.FirstRespectedReserver(Parent.CurrentRobot, pawn);
                return new AcceptanceReport((pawn2 == null) ? "Reserved".Translate() : "ReservedBy".Translate(pawn.LabelShort, pawn2));
            }
            if (ComponentsForManualRepair == 0 || AvailableComponents < ComponentsForManualRepair) return new AcceptanceReport($"{ComponentsForManualRepair} components are required for manual repairs, refill the station.");
            return AcceptanceReport.WasAccepted;
        }

        public void RepairManually(Pawn pawn)
        {
            var job = JobMaker.MakeJob(RSDefOf.RSManualRepair, this.parent, Parent.CurrentRobot);
            job.count = 1;
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

    }
}
