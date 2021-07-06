using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimsecSecurity
{
    class Verb_LaunchProjectileWithOffset : Verb_Shoot
    {

        protected override bool TryCastShot()
        {
            if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
            {
                return false;
            }
            ThingDef projectile = this.Projectile;
            if (projectile == null)
            {
                return false;
            }
            ShootLine shootLine;
            bool flag = base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out shootLine);
            if (this.verbProps.stopBurstWithoutLos && !flag)
            {
                return false;
            }
            if (base.EquipmentSource != null)
            {
                CompChangeableProjectile comp = base.EquipmentSource.GetComp<CompChangeableProjectile>();
                if (comp != null)
                {
                    comp.Notify_ProjectileLaunched();
                }
                CompReloadable comp2 = base.EquipmentSource.GetComp<CompReloadable>();
                if (comp2 != null)
                {
                    comp2.UsedOnce();
                }
            }
            Thing launcher = this.caster;
            Thing equipment = base.EquipmentSource;
            CompMannable compMannable = this.caster.TryGetComp<CompMannable>();
            if (compMannable != null && compMannable.ManningPawn != null)
            {
                launcher = compMannable.ManningPawn;
                equipment = this.caster;
            }
            Vector3 drawPos = this.caster.DrawPos;
            var casterTurret = caster as Building_TurretGun;
            Thing gun = null; 
            if (casterTurret != null) gun = casterTurret.gun;
            var newSource = ApplyOffset(shootLine.Source, shootLine.Dest);
            var newSourceVec = newSource.ToVector3();
            Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, this.caster.Position, this.caster.Map, WipeMode.Vanish);
            
            
            if (this.verbProps.ForcedMissRadius > 0.5f)
            {
                float num = VerbUtility.CalculateAdjustedForcedMiss(this.verbProps.ForcedMissRadius, this.currentTarget.Cell - this.caster.Position);
                if (num > 0.5f)
                {
                    int max = GenRadial.NumCellsInRadius(num);
                    int num2 = Rand.Range(0, max);
                    if (num2 > 0)
                    {
                        IntVec3 c = this.currentTarget.Cell + GenRadial.RadialPattern[num2];
                        this.ThrowDebugText("ToRadius");
                        this.ThrowDebugText("Rad\nDest", c);
                        ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
                        if (Rand.Chance(0.5f))
                        {
                            projectileHitFlags = ProjectileHitFlags.All;
                        }
                        if (!this.canHitNonTargetPawnsNow)
                        {
                            projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
                        }
                        projectile2.Launch(launcher, newSourceVec, c, this.currentTarget, projectileHitFlags, false, equipment, null);
                        return true;
                    }
                }
            }
            ShotReport shotReport = ShotReport.HitReportFor(this.caster, this, this.currentTarget);
            Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
            ThingDef targetCoverDef = (randomCoverToMissInto != null) ? randomCoverToMissInto.def : null;
            if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
            {
                shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget);
                this.ThrowDebugText("ToWild" + (this.canHitNonTargetPawnsNow ? "\nchntp" : ""));
                this.ThrowDebugText("Wild\nDest", shootLine.Dest);
                ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
                if (Rand.Chance(0.5f) && this.canHitNonTargetPawnsNow)
                {
                    projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
                }
                projectile2.Launch(launcher, newSourceVec, shootLine.Dest, this.currentTarget, projectileHitFlags2, false, equipment, targetCoverDef);
                return true;
            }
            if (this.currentTarget.Thing != null && this.currentTarget.Thing.def.category == ThingCategory.Pawn && !Rand.Chance(shotReport.PassCoverChance))
            {
                this.ThrowDebugText("ToCover" + (this.canHitNonTargetPawnsNow ? "\nchntp" : ""));
                this.ThrowDebugText("Cover\nDest", randomCoverToMissInto.Position);
                ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
                if (this.canHitNonTargetPawnsNow)
                {
                    projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
                }
                projectile2.Launch(launcher, newSourceVec, randomCoverToMissInto, this.currentTarget, projectileHitFlags3, false, equipment, targetCoverDef);
                return true;
            }
            ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
            if (this.canHitNonTargetPawnsNow)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
            }
            if (!this.currentTarget.HasThing || this.currentTarget.Thing.def.Fillage == FillCategory.Full)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
            }
            this.ThrowDebugText("ToHit" + (this.canHitNonTargetPawnsNow ? "\nchntp" : ""));
            if (this.currentTarget.Thing != null)
            {
                projectile2.Launch(launcher, newSourceVec, this.currentTarget, this.currentTarget, projectileHitFlags4, false, equipment, targetCoverDef);
                this.ThrowDebugText("Hit\nDest", this.currentTarget.Cell);
            }
            else
            {
                projectile2.Launch(launcher, newSourceVec, shootLine.Dest, this.currentTarget, projectileHitFlags4, false, equipment, targetCoverDef);
                this.ThrowDebugText("Hit\nDest", shootLine.Dest);
            }
            return true;
        }

        private IntVec3 ApplyOffset(IntVec3 source, IntVec3 dest)
        {
            var rot = GetRotation(source, dest);
            var sourceOffset = burstShotsLeft % 2 != 0 ? RightHandCellOffset(rot) : LeftHandCellOffset(rot);
            var newPos = new IntVec3(source.x + sourceOffset.x, source.y, source.z + sourceOffset.z);
            return newPos;
        }

        private Rot4 GetRotation(IntVec3 source, IntVec3 dest)
        {
            Quaternion.LookRotation((dest.ToVector3() - source.ToVector3()).Yto0()).ToAngleAxis(out var angle, out var axis);
            if (angle >= 45 && angle < 135) return Rot4.East;
            else if (angle >= 135 && angle < 225) return Rot4.South;
            else if (angle >= 225 && angle < 315) return Rot4.West;
            else return Rot4.North;
        }

        public IntVec3 LeftHandCellOffset(Rot4 rot)
        {
            switch (rot.AsInt)
            {
                case 0:
                    return new IntVec3(0, 0, 1);
                case 1:
                    return new IntVec3(1, 0, 3);
                case 2:
                    return new IntVec3(3, 0, 1);
                case 3:
                    return new IntVec3(1, 0, 0);
                default:
                    return default(IntVec3);
            }
        }

        public IntVec3 RightHandCellOffset(Rot4 rot)
        {
            switch (rot.AsInt)
            {
                case 0:
                    return new IntVec3(3, 0, 1);
                case 1:
                    return new IntVec3(1, 0, 0);
                case 2:
                    return new IntVec3(0, 0, 1);
                case 3:
                    return new IntVec3(1, 0, 3);
                default:
                    return default(IntVec3);
            }
        }

        private void ThrowDebugText(string text, bool overwrite = false)
        {
            if (DebugViewSettings.drawShooting || overwrite)
            {
                MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, text, -1f);
            }
        }

        private void ThrowDebugText(string text, IntVec3 c, bool overwrite = false)
        {
            if (DebugViewSettings.drawShooting || overwrite)
            {
                MoteMaker.ThrowText(c.ToVector3Shifted(), this.caster.Map, text, -1f);
            }
        }
    }
}
