using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimsecSecurity
{
    class PeacekeeperUtility
    {
        public static bool IsPeacekeeper(Pawn pawn) => pawn != null && pawn.def.HasModExtension<RSPeacekeeperModExt>();
        public static Thing GetEmptyChargeStation(Pawn pawn) => pawn.Map?.listerBuildings.allBuildingsColonist.OfType<Building_ChargeStation>().Where(x => x.CurrentRobot == null && x.def == pawn.def?.GetModExtension<RSPeacekeeperModExt>()?.stationDef && pawn.Map.reservationManager.CanReserve(pawn, x)).OrderBy(station => pawn.Position.DistanceTo(station.Position)).FirstOrDefault();
        public static bool IsInChargeStation(Pawn pawn) => pawn == null || pawn.Map == null ? false : pawn.Position.GetThingList(pawn.Map).Any(x => x.def == pawn.def.GetModExtension<RSPeacekeeperModExt>().stationDef && ((Building_ChargeStation)x).CurrentRobot == pawn);
        public static bool IsChargeStationFree(Thing station) => station.Map.reservationManager.IsReservedByAnyoneOf(station, Faction.OfPlayer);
        public static Pawn GetCurrentPawn(Thing pawn) => pawn.Position.GetFirstPawn(pawn.Map) ?? PositionAbove(pawn).GetFirstPawn(pawn.Map);
        public static IntVec3 PositionAbove(Thing thing) => new IntVec3(thing.Position.x, thing.Position.y, thing.Position.z + 1);

        public static Pawn GeneratePeacekeeper(PawnKindDef pawnKind, int tile)
        {
            var robot = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, tile, false, false, false, false, true, false, 20f, true, true, true, true, false, false, false, false, 0f, null, 1f, null, null, null, null, new float?(0.2f), 0, null, Gender.Male, null, null, null, null));
            robot.Name = new NameSingle(robot.Name.ToStringShort + " #" + ModSettings.peacekeeperNumber++);
            var hediff = HediffMaker.MakeHediff(RSDefOf.RSRobotConsciousness, robot);
            if (robot != null) robot.health.AddHediff(hediff, robot.health.hediffSet.GetBrain(), null, null);

            var robotModExt = robot.def.GetModExtension<RSPeacekeeperModExt>();
            var batteryHediff = HediffMaker.MakeHediff(RSDefOf.RSPeacekeeperBattery, robot);
            robot.health.AddHediff(batteryHediff, PeacekeeperUtility.GetTorso(robot), null, null);
            batteryHediff.Severity = robotModExt.batterySeverity;
            var allowedTraits = (robot.def as ThingDef_AlienRace)?.alienRace?.generalSettings?.forcedRaceTraitEntries?.Select(entry => entry.defName)?.ToList();
            if (allowedTraits != null && allowedTraits.Count != 0)
            {
                foreach (var trait in robot.story.traits.allTraits.Reverse<Trait>()) robot.story.traits.allTraits.Remove(trait);
                foreach (var trait in allowedTraits) robot.story.traits.GainTrait(new Trait(trait));
            }

            robot.skills.skills.FirstOrDefault(x => x.def == SkillDefOf.Shooting).Level = robotModExt.shootingSkill;
            robot.skills.skills.FirstOrDefault(x => x.def == SkillDefOf.Melee).Level = robotModExt.meleeSkill;

            return robot;
        }

        public static IntVec3 GetSleepingPosForChargeStation(Pawn takee, Thing station)
        {
            var pos = BedUtility.GetSleepingSlotPos(0, station.Position, station.Rotation, station.def.size);
            return pos;
        }

        internal static IEnumerable<BodyPartRecord> GetLegs(Pawn pawn)
        {
            foreach (BodyPartRecord bodyPartRecord in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null))
            {
                if (bodyPartRecord.def.tags.Contains(BodyPartTagDefOf.MovingLimbCore))
                {
                    yield return bodyPartRecord;
                }
            }
        }

        public static BodyPartRecord GetTorso(Pawn pawn)
        {
            foreach (BodyPartRecord bodyPartRecord in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null))
            {
                if (bodyPartRecord.def == RSDefOf.MechanicalThorax)
                {
                    return bodyPartRecord;
                }
            }
            return null;
        }
    }
}
