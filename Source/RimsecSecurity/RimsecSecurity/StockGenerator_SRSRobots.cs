using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimsecSecurity
{
	class StockGenerator_SRSRobots : StockGenerator
	{
		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			var count = this.countRange.RandomInRange;
			var validPawnKinds = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def.race.HasModExtension<RSPeacekeeperModExt>());
			if (validPawnKinds == null || validPawnKinds.Count() == 0) yield break;
			for (int i = 0; i < count; i++)
			{
				validPawnKinds.TryRandomElementByWeight(new Func<PawnKindDef, float>(this.SelectionWeight), out var pawnKind);
				var robot = PeacekeeperUtility.GeneratePeacekeeper(pawnKind, forTile);
				if (robot == null) continue;
				robot.SetFaction(Faction.OfMechanoids);
                var gun = ThingMaker.MakeThing(pawnKind.race.GetModExtension<RSPeacekeeperModExt>().gunDef) as ThingWithComps;
				if (gun != null)
                {
					robot.equipment.MakeRoomFor(gun);
					robot.equipment.AddEquipment(gun);
				}
                yield return robot;
			}
			yield break;
		}

		protected float SelectionWeight(PawnKindDef thingDef)
		{
			return SelectionWeightMarketValueCurve.Evaluate(thingDef.race.BaseMarketValue);
		}

		private static readonly SimpleCurve SelectionWeightMarketValueCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 1f),
				true
			},
			{
				new CurvePoint(500f, 1f),
				true
			},
			{
				new CurvePoint(1500f, 0.7f),
				true
			},
			{
				new CurvePoint(5000f, 0.5f),
				true
			}
		};

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.category == ThingCategory.Pawn && thingDef.race.Humanlike && thingDef.tradeability > Tradeability.None;
		}
	}
}
