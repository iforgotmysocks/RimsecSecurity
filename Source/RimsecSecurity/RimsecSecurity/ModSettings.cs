using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimsecSecurity
{
    class ModSettings : Verse.ModSettings
    {
        public static int peacekeeperNumber = 1;
        public static bool butcheredPeacekeeper;
        public static float energyShortageSeverityMult = 10;
        public static bool hidePeacekeepersFromColonistBar = false;
        public static bool debugActive = false;
        public static bool countPeacekeepersTowardsPopulation = true;
        public static float fuelConsumptionRate;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref peacekeeperNumber, "peacekeeperNumber", 1);
            Scribe_Values.Look(ref energyShortageSeverityMult, "energyShortageSeverityMult", 0);
            Scribe_Values.Look(ref hidePeacekeepersFromColonistBar, "hidePeacekeepersFromColonistBar", false);
            Scribe_Values.Look(ref countPeacekeepersTowardsPopulation, "countPeacekeepersTowardsPopulation", true);
            Scribe_Values.Look(ref fuelConsumptionRate, "fuelConsumptionRate", 0.5f);
            Scribe_Values.Look(ref debugActive, "debugActive", false);
        }
        
        public void DoWindowContents(Rect rect)
        {
            var options = new Listing_Standard();
            options.Begin(rect);
            options.Gap(24f);
            options.CheckboxLabeled("Hide peacekeeper robots from colonist bar  (change requires restart)", ref hidePeacekeepersFromColonistBar);
            options.CheckboxLabeled("Count peacekeeper robots towards colonist population  (change requires restart)", ref countPeacekeepersTowardsPopulation);
            options.Label($"Maintenance station: Component fuel consumption rate per day: {Math.Round(fuelConsumptionRate, 2)}  (change requires restart)");
            fuelConsumptionRate = options.Slider(fuelConsumptionRate, 0.01f, 2f);
            options.Gap(24f);
            if (options.ButtonTextLabeled("Spawn random test robot at random colonist location", "Spawn")) PeacekeeperUtility.SpawnRandomRobot(); 

            options.End();
        }

    }
}
