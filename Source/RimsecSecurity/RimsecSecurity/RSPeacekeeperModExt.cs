using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimsecSecurity
{
    class RSPeacekeeperModExt : DefModExtension
    {
        public bool isPeacekeeper = true;
        public ThingDef stationDef = null;
        public int stationZOffset = 0;
        public int repairTicks = 400;
        public ThingDef gunDef = null;
        public float batterySeverity = 0f;
    }
}
