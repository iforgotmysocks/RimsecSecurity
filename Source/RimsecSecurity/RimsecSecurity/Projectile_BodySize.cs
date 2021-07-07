using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimsecSecurity
{
    class Projectile_BodySize : Bullet
    {
        protected override void Impact(Thing hitThing)
        {
            var map = base.Map;
            var position = base.Position;
            GenClamor.DoClamor(this, 2.1f, ClamorDefOf.Impact);
            FleckMaker.Static(position, Map, DefDatabase<FleckDef>.GetNamed("BlastFlame"), 2);
            SoundDef.Named("Explosion_Bomb").PlayOneShot(new TargetInfo(base.Position, base.Map, false));

            this.Destroy(DestroyMode.Vanish);
            var battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget.Thing, this.equipmentDef, this.def, this.targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            this.NotifyImpact(hitThing, map, position);
            if (hitThing != null)
            {
                var dinfo = new DamageInfo(this.def.projectile.damageDef, (float)base.DamageAmount, base.ArmorPenetration, this.ExactRotation.eulerAngles.y, this.launcher, null, this.equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing);
                var pawn = hitThing as Pawn;
                if (pawn != null)
                {
                    var mult = pawn.BodySize <= 1 ? 1 : pawn.BodySize * 2.5f;
                    dinfo.SetAmount(dinfo.Amount * mult);
                }
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                if (pawn != null && pawn.stances != null && pawn.BodySize <= this.def.projectile.StoppingPower + 0.001f)
                {
                    pawn.stances.StaggerFor(95);
                }
                if (this.def.projectile.extraDamages == null)
                {
                    return;
                }
                using (List<ExtraDamage>.Enumerator enumerator = this.def.projectile.extraDamages.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ExtraDamage extraDamage = enumerator.Current;
                        if (Rand.Chance(extraDamage.chance))
                        {
                            var dinfo2 = new DamageInfo(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), this.ExactRotation.eulerAngles.y, this.launcher, null, this.equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing);
                            hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
                        }
                    }
                    return;
                }
            }
            SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(base.Position, map, false));
            if (base.Position.GetTerrain(map).takeSplashes)
            {
                FleckMaker.WaterSplash(this.ExactPosition, map, Mathf.Sqrt((float)base.DamageAmount) * 1f, 4f);
                return;
            }
            FleckMaker.Static(this.ExactPosition, map, FleckDefOf.ShotHit_Dirt, 1f);
        }


        private void NotifyImpact(Thing hitThing, Map map, IntVec3 position)
        {
            var impactData = new BulletImpactData
            {
                bullet = this,
                hitThing = hitThing,
                impactPosition = position
            };
            if (hitThing != null)
            {
                hitThing.Notify_BulletImpactNearby(impactData);
            }
            int num = 9;
            for (int i = 0; i < num; i++)
            {
                IntVec3 c = position + GenRadial.RadialPattern[i];
                if (c.InBounds(map))
                {
                    List<Thing> thingList = c.GetThingList(map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        if (thingList[j] != hitThing)
                        {
                            thingList[j].Notify_BulletImpactNearby(impactData);
                        }
                    }
                }
            }
        }

    }
}
