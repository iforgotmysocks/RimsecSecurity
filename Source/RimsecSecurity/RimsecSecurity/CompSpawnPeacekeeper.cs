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
    class CompSpawnPeacekeeper : ThingComp
    {
        public CompProperties_SpawnPeacekeeper Props => (CompProperties_SpawnPeacekeeper)props;

        public override void CompTick()
        {
            var robot = PeacekeeperUtility.GeneratePeacekeeper(Props.pawnKind, this.parent.Tile);
            var spawnedBot = GenSpawn.Spawn(robot, this.parent.Position, this.parent.Map, Rot4.South) as Pawn;
            var gun = ThingMaker.MakeThing(Props.weaponDef) as ThingWithComps;
            spawnedBot.equipment.MakeRoomFor(gun);
            spawnedBot.equipment.AddEquipment(gun);
            //AddCape(spawnedBot);
            this.parent.Destroy();
        }

        //private static void AddCape(Pawn robot)
        //{
        //    if (robot == null) return;
        //    var cape = ThingMaker.MakeThing(ThingDef.Named("Apparel_Cape"), ThingDef.Named("DevilstrandCloth")) as Apparel;
        //    if (cape == null)
        //    {
        //        Log.Message($"cape is null");
        //        return;
        //    }

        //    robot.apparel.Wear(cape);
        //}
    }
}
