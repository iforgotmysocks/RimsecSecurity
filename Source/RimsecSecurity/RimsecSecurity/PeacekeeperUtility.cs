using AlienRace;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimsecSecurity
{
    public class PeacekeeperUtility
    {
        public static bool IsPeacekeeper(Pawn pawn) => pawn != null && pawn.def.HasModExtension<RSPeacekeeperModExt>();
        public static Thing GetEmptyChargeStation(Pawn pawn) => pawn.Faction != Faction.OfPlayerSilentFail ? null : pawn.Map?.listerBuildings.allBuildingsColonist.OfType<Building_ChargeStation>().Where(x => (x.CurrentRobot == null || x.CurrentRobot == pawn) && x.def == pawn.def?.GetModExtension<RSPeacekeeperModExt>()?.stationDef && pawn.Map.reservationManager.CanReserve(pawn, x)).OrderBy(station => pawn.Position.DistanceTo(station.Position)).FirstOrDefault();
        public static bool IsInChargeStation(Pawn pawn) => pawn == null || pawn.Map == null ? false : pawn.Position.GetThingList(pawn.Map).Any(x => x.def == pawn.def.GetModExtension<RSPeacekeeperModExt>().stationDef && ((Building_ChargeStation)x).CurrentRobot == pawn);
        public static bool IsChargeStationFree(Thing station) => station.Map.reservationManager.IsReservedByAnyoneOf(station, Faction.OfPlayer);
        public static Pawn GetCurrentPawn(Thing pawn) => pawn.Position.GetFirstPawn(pawn.Map) ?? PositionAbove(pawn).GetFirstPawn(pawn.Map);
        public static IntVec3 PositionAbove(Thing thing) => new IntVec3(thing.Position.x, thing.Position.y, thing.Position.z + 1);

        public static Pawn GeneratePeacekeeper(PawnKindDef pawnKind, int tile)
        {
            var robot = PawnGenerator.GeneratePawn(new PawnGenerationRequest()
            {
                KindDef = pawnKind,
                Faction = Faction.OfPlayer,
                Context = PawnGenerationContext.NonPlayer,
                Tile = tile,
                FixedBiologicalAge = 0,
                FixedGender = Gender.Male,
                AllowAddictions = false,
                FixedMelanin = 0,
                CanGeneratePawnRelations = false, 
                FixedIdeo = null,
                ForceNoIdeo = true
            });

            robot.Name = new NameSingle(robot.Name.ToStringShort + " #" + ModSettings.peacekeeperNumber++);
            var hediff = HediffMaker.MakeHediff(RSDefOf.RSRobotConsciousness, robot);
            if (robot != null && !robot.health.hediffSet.HasHediff(RSDefOf.RSRobotConsciousness)) robot.health.AddHediff(hediff, robot.health.hediffSet.GetBrain(), null, null);

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

            robot.playerSettings.hostilityResponse = HostilityResponseMode.Attack;

            robot.skills.skills.FirstOrDefault(x => x.def == SkillDefOf.Shooting).Level = robotModExt.shootingSkill;
            robot.skills.skills.FirstOrDefault(x => x.def == SkillDefOf.Melee).Level = robotModExt.meleeSkill;

            robot.guest.joinStatus = JoinStatus.JoinAsColonist;

            return robot;
        }

        internal static void SpawnRandomRobot()
        {
            if (Find.World == null)
            {
                Messages.Message(new Message("No world found", MessageTypeDefOf.NegativeEvent));
                return;
            }
            var currentMap = Find.CurrentMap ?? Find.RandomPlayerHomeMap;
            if (currentMap == null)
            {
                Messages.Message(new Message("No map found", MessageTypeDefOf.NegativeEvent));
                return;
            }
            var peaceKeepers = DefDatabase<PawnKindDef>.AllDefs.Where(def => def.race.HasModExtension<RSPeacekeeperModExt>());
            if (peaceKeepers == null || peaceKeepers.Count() == 0) return;
            var selectedKeeper = peaceKeepers.RandomElement();
            var robot = GeneratePeacekeeper(selectedKeeper, currentMap.Tile);
            if (robot == null) return;
            var cell = currentMap.mapPawns.FreeColonists.FirstOrDefault()?.Position ?? currentMap.AllCells.Where(curCell => curCell.Walkable(currentMap) && !curCell.Fogged(currentMap) && curCell != default).RandomElement();
            if (cell == default) return;  
            
            var spawnedRobot = GenSpawn.Spawn(robot, cell, currentMap) as Pawn;
            if (spawnedRobot == null) return;
            var gun = ThingMaker.MakeThing(selectedKeeper.race.GetModExtension<RSPeacekeeperModExt>().gunDef) as ThingWithComps;
            spawnedRobot.equipment.MakeRoomFor(gun);
            spawnedRobot.equipment.AddEquipment(gun);
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

        internal static void RefuelPawnOnCaravan(Pawn pawn, Caravan caravan)
        {
            if (pawn.needs.rest.CurLevel > 0.4) return;
            var fuel = caravan.AllThings.FirstOrDefault(t => t.def == RSDefOf.RSPowerCell);
            if (fuel == null) return;
            var owner = CaravanInventoryUtility.GetOwnerOf(caravan, fuel);
            var amount = Math.Min(fuel.stackCount, 6);
            if (fuel.stackCount > amount) fuel.stackCount -= amount;
            else owner.inventory.innerContainer.Remove(fuel);
            pawn.needs.rest.CurLevel += amount / 10f;
        }

        public static Job RefuelJob(Pawn pawn, Thing t, bool forced = false, JobDef customRefuelJob = null)
        {
            Thing t2 = PeacekeeperUtility.FindBestFuel(pawn);
            return JobMaker.MakeJob(customRefuelJob ?? RSDefOf.RSFuelRobot, t, t2);
        }

        public static Thing FindBestFuel(Pawn pawn)
        {
            var filter = new ThingFilter();
            filter.SetAllow(RSDefOf.RSPowerCell, true);
            bool validator(Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && filter.Allows(x);
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, filter.BestThingRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        }

        public static void RunSavely(Action action) => RunSavely(() => { action(); return 0; });

        public static T RunSavely<T>(Func<T> action)
        {
            try
            {
               return action();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return default(T);
        }

        public static Assembly GetAssemblyFromString(string assemblyName) => AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName.ToLower().Contains(assemblyName));

    }
}
