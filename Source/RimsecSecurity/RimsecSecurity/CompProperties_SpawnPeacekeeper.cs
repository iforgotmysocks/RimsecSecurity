using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimsecSecurity
{
#pragma warning disable CS0649
    class CompProperties_SpawnPeacekeeper : CompProperties
    {
        public CompProperties_SpawnPeacekeeper()
        {
            this.compClass = typeof(CompSpawnPeacekeeper);
        }

        public PawnKindDef pawnKind;
        public ThingDef weaponDef;
    }
}
