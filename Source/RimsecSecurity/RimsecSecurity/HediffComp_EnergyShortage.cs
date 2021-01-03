using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimsecSecurity
{
    class HediffComp_EnergyShortage : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn.needs.rest.CurLevel > 0.01) this.parent.Severity = 0;
            if (Pawn != null && Pawn.Faction != Faction.OfPlayer) Pawn.Kill(null, this.parent); 
        }

    }
}
