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

namespace ComfyQM_Standalone
{
    public class ComfyQM : MelonMod
    {
        private static bool enabled;
        public static MelonPreferences_Category ComfyQuickMenu;
        public override void OnApplicationStart()
        {
            //ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddToggleButton("ComfyQM", (b) => enabled = b);
            HarmonyInstance.Patch(typeof(QuickMenu).GetMethod(nameof(QuickMenu.Method_Private_Void_Boolean_0)), typeof(ComfyQM).GetMethod(nameof(SetupForDesktopOrHMDPatch),System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).ToNewHarmonyMethod());
            ComfyQuickMenu = MelonPreferences.CreateCategory("ComfyQM");
            ComfyToggle = ComfyQuickMenu.CreateEntry("ComfyQM Toggle", false);
        }
        public static MelonPreferences_Entry<bool> ComfyToggle;
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
            qm.transform.localPosition = new Vector3(0, -0.5f, 1.25f);
        }
    }
}
