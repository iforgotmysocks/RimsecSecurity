using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimsecSecurity
{
	class StockGenerator_SRSGuns : StockGenerator_WeaponsRanged
	{

		protected override float SelectionWeight(ThingDef thingDef)
		{
			return SelectionWeightMarketValueCurve.Evaluate(thingDef.BaseMarketValue);
		}

		private static readonly SimpleCurve SelectionWeightMarketValueCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 1f),
				true
			},
			{
				new CurvePoint(1000f, 1f),
				true
			},
			{
				new CurvePoint(2000f, 0.7f),
				true
			},
			{
				new CurvePoint(4000f, 0.5f),
				true
			}
		};

		public override bool HandlesThingDef(ThingDef td)
		{
			return td.IsRangedWeapon && (td.weaponTags != null && td.weaponTags.Contains(weaponTag));
		}

		public new const string weaponTag = "RSPeacekeeperGun";
	}
}
