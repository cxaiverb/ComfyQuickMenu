using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using HarmonyLib;
using UIExpansionKit;
using UIExpansionKit.API;
using UnityEngine;
using ComfyQM_Standalone;

namespace ComfyQM_Standalone
{
    public class ComfyQM : MelonMod
    {
        public static MelonPreferences_Category ComfyQuickMenu;
        public override void OnApplicationStart()
        {
            HarmonyInstance.Patch(typeof(QuickMenu).GetMethod(nameof(QuickMenu.Method_Private_Void_Boolean_0)), typeof(ComfyQM).GetMethod(nameof(SetupForDesktopOrHMDPatch),System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).ToNewHarmonyMethod());
            ComfyQuickMenu = MelonPreferences.CreateCategory("ComfyQM");
            ComfyToggle = ComfyQuickMenu.CreateEntry("ComfyQM Toggle", false);
            distance = ComfyQuickMenu.CreateEntry("Quick Menu Distance", 1.25f);
        }
        public static MelonPreferences_Entry<bool> ComfyToggle;
        public static MelonPreferences_Entry<float> distance;
        private static void SetupForDesktopOrHMDPatch(ref bool __0)
        {
            if (ComfyToggle.Value)
            {
                __0 = true;
            }
        }
        public override void OnUpdate()
        {
            if (ComfyToggle.Value == false)
                return;
            var qm = QuickMenu.field_Private_Static_QuickMenu_0;
            if (qm == null)
                return;
            qm.transform.localPosition = new Vector3(0, -0.5f, distance.Value);
        }
    }
}
