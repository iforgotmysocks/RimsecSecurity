using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimsecSecurity
{
    class IncidentWorker_OrbitalTraderSRS : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && ((Map)parms.target).passingShipManager.passingShips.Count < MaxShips;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map)parms.target;
            if (map.passingShipManager.passingShips.Count >= MaxShips)
            {
                return false;
            }

            var tradeShip = new TradeShip(RSDefOf.RSSRSTradeShip);
            if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && (b.GetComp<CompPowerTrader>() == null || b.GetComp<CompPowerTrader>().PowerOn)))
            {
                base.SendStandardLetter(tradeShip.def.LabelCap, "TraderArrival".Translate(tradeShip.name, tradeShip.def.label, (tradeShip.Faction == null) ? "TraderArrivalNoFaction".Translate() : "TraderArrivalFromFaction".Translate(tradeShip.Faction.Named("FACTION"))), LetterDefOf.PositiveEvent, parms, LookTargets.Invalid, Array.Empty<NamedArgument>());
            }
            map.passingShipManager.AddShip(tradeShip);
            tradeShip.GenerateThings();
            return true;
        }

        private const int MaxShips = 5;
    }
}
