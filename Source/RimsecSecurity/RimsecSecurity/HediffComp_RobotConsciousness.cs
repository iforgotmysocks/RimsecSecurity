using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimsecSecurity
{
    class HediffComp_RobotConsciousness : HediffComp
    {
        public List<TerrainDef> allowedTerrain = new List<TerrainDef> { TerrainDefOf.Ice };
        public List<TerrainDef> allowedTerrainSand = new List<TerrainDef> { TerrainDefOf.Sand, TerrainDef.Named("SoftSand") };
        public List<TerrainDef> allowedTerrainForest = new List<TerrainDef> { TerrainDefOf.Soil, TerrainDefOf.Gravel };
        public List<BiomeDef> allowedBiomes = new List<BiomeDef> { BiomeDefOf.SeaIce, BiomeDefOf.IceSheet };
        public List<BiomeDef> allowedBiomesSand = new List<BiomeDef> { BiomeDefOf.Desert, BiomeDefOf.Tundra };
        public List<BiomeDef> allowedBiomesForest = new List<BiomeDef> { BiomeDefOf.BorealForest, BiomeDefOf.TemperateForest };

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (this.parent?.pawn == null || parent.pawn.Tile == -1) return;
            if ((Find.TickManager.TicksGame + this.parent.pawn.thingIDNumber) % 120 == 0) CheckTerrain();
        }

        private void CheckTerrain()
        {
            var selTrait = this.parent.pawn.story.traits.allTraits.FirstOrDefault(trait => trait.def == RSDefOf.RSTraitWinter || trait.def == RSDefOf.RSTraitDesert || trait.def == RSDefOf.RSTraitForest);
            if (selTrait == null) return;
            var apply = false;
            var selTerrain = selTrait.def == RSDefOf.RSTraitWinter ? allowedTerrain : allowedTerrainSand;
            var selBiomes = selTrait.def == RSDefOf.RSTraitWinter ? allowedBiomes : allowedBiomesSand;

            if (selTrait.def == RSDefOf.RSTraitForest)
            {
                if (this.parent.pawn.Map == null && allowedBiomesForest.Contains(Find.WorldGrid.tiles[this.parent.pawn.Tile].biome)
                    || (this.parent.pawn.Map != null && allowedBiomesForest.Contains(Find.WorldGrid.tiles[this.parent.pawn.Tile].biome)
                    && (allowedTerrainForest.Contains(this.parent.pawn.Position.GetTerrain(this.parent.pawn.Map)))))
                {
                    apply = true;
                }
            }
            else if (this.parent.pawn.Map == null && selBiomes.Contains(Find.WorldGrid.tiles[this.parent.pawn.Tile].biome)
                || (this.parent.pawn.Map != null && (selTerrain.Contains(this.parent.pawn.Position.GetTerrain(this.parent.pawn.Map))
                    || (selTrait.def == RSDefOf.RSTraitWinter && parent.pawn.Map.snowGrid.TotalDepth > 100f))))
            {
                apply = true;
            }

            if (apply)
            {
                if (!this.parent.pawn.health.hediffSet.HasHediff(RSDefOf.RSTerrainAdvantage))
                {
                    foreach (var leg in PeacekeeperUtility.GetLegs(parent.pawn))
                    {
                        var hediff = HediffMaker.MakeHediff(RSDefOf.RSTerrainAdvantage, parent.pawn);
                        this.parent.pawn.health.AddHediff(hediff, leg);
                    }
                }
            }
            else
            {
                if (this.parent.pawn.health.hediffSet.HasHediff(RSDefOf.RSTerrainAdvantage))
                {
                    this.parent.pawn.health.hediffSet.hediffs.RemoveAll(hediff => hediff.def == RSDefOf.RSTerrainAdvantage);
                }
            }
        }
    }
}
