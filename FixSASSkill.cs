using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FixSASSkill
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class FixSASSkill : MonoBehaviour
    {
        internal static Harmony harmony = new Harmony("org.goufastyle.goufamods.FixSASSkill");

        static bool patched = false;
        private void Awake()
        {
            Harmony.DEBUG = false;
            if (!patched)
            {
                var assembly = Assembly.GetExecutingAssembly();
                harmony.PatchAll(assembly);
                patched = true;
            }
        }
    }

    [HarmonyPatch(typeof(APSkillExtensions), "AvailableAtLevel")]
    [HarmonyPatch(new Type[] { typeof(VesselAutopilot.AutopilotMode), typeof(Vessel) })]
    class PatchSASSkill
    {
        static bool Prefix(VesselAutopilot.AutopilotMode mode, Vessel vessel, ref bool __result)
        {
            //Debug.Log("AutopilotKerbal = " + vessel.VesselValues.AutopilotKerbalSkill.value);
            //Debug.Log("AutopilotSASKerbal = " + vessel.VesselValues.AutopilotSASSkill.value);
            int pilotSkill = vessel.VesselValues.AutopilotKerbalSkill.value;
            int probeSkill = vessel.VesselValues.AutopilotSASSkill.value;
            bool fullSASInMissions = HighLogic.CurrentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().EnableFullSASInMissions;
            bool fullSASInSandbox = HighLogic.CurrentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().EnableFullSASInSandbox;
            bool hasStayPutnik = vessel.Parts.Any(p => p.name.Equals("probeCoreSphere.v2"));
            // full SAS in sandbox
            if (fullSASInSandbox && HighLogic.CurrentGame.Mode != Game.Modes.MISSION && (probeSkill != -1 || probeSkill == -1 && hasStayPutnik))
            {
                probeSkill = 3;
            }
            // full SAS in missions ???
            if (fullSASInMissions && HighLogic.CurrentGame.Mode == Game.Modes.MISSION && (probeSkill != -1 || probeSkill == -1 && hasStayPutnik))
            {
                probeSkill = 3;
            }
            __result = Math.Max(pilotSkill, probeSkill) >= mode.GetRequiredSkill();

            return false;
        }
    }
}
