using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace RimsecSecurity
{
    class Building_ChargeStation : Building
    {
        private Graphic graphicStationOn;

        private CompFlickable compFlick;
        public CompFlickable CompFlick
        {
            get => compFlick ?? (compFlick = this.TryGetComp<CompFlickable>());
            set => compFlick = value;
        }
        private CompPowerTrader compPower;
        public CompPowerTrader CompPower
        {
            get => compPower ?? (compPower = this.TryGetComp<CompPowerTrader>());
            set => compPower = value;
        }
        private CompRefuelable compFuel;
        public CompRefuelable CompFuel
        {
            get => compFuel ?? (compFuel = this.TryGetComp<CompRefuelable>());
            set => compFuel = value;
        }
        private CompRechargeRobot compRecharge;
        public CompRechargeRobot CompRecharge
        {
            get => compRecharge ?? (compRecharge = this.TryGetComp<CompRechargeRobot>());
            set => compRecharge = value;
        }

        private Pawn currentRobot;
        public Pawn CurrentRobot { get => currentRobot; set => currentRobot = value; }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref currentRobot, "currentRobot");
        }

        public override void Tick()
        {
            base.Tick();
            if ((Find.TickManager.TicksGame + this.thingIDNumber) % 60 == 0)
            {
                if (CurrentRobot == null) return;
                if (CurrentRobot?.Map != this.Map || CurrentRobot.Position != this.Position
                    && CurrentRobot.Position != PeacekeeperUtility.PositionAbove(this))
                {
                    CurrentRobot = null;
                }
            }
        }

        public bool PowerOff() => CompPower == null || !CompPower.PowerOn || CompPower?.PowerNet?.HasActivePowerSource != true;

        public override Graphic Graphic
        {
            get
            {
                if (this == null) { Log.Message($"this is null"); return null; }

                if (PowerOff() || CompFlick == null || !CompFlick.SwitchIsOn) return this.DefaultGraphic;

                if (graphicStationOn == null)
                {
                    graphicStationOn = GraphicDatabase.Get(this.def.graphicData.graphicClass, this.def.graphicData.texPath + "_On", this.def.graphicData.shaderType.Shader, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo);
                    var graphicData = this.def.graphic.data;
                    graphicStationOn.data = graphicData;
                }
                return graphicStationOn;
            }
        }

        public IntVec3 GetStandPosition(Pawn pawn)
        {
            var modExt = pawn.def.GetModExtension<RSPeacekeeperModExt>();
            if (modExt == null)
            {
                Log.Error($"pawn is not a peacekeeper");
                return default;
            }

            return new IntVec3(this.Position.x, this.Position.y, this.Position.z + modExt.stationZOffset);
        }

    }
}
