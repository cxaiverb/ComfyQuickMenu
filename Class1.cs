using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;
using MelonLoader;
using HarmonyLib;
using UIExpansionKit;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.XR;
using ComfyQM_Standalone;
namespace ComfyQM_Standalone
{
    public class ComfyQM : MelonMod
    {
        public static MelonPreferences_Entry<bool> ComfyToggle;
        public static MelonPreferences_Entry<float> distance;
        public static MelonPreferences_Category ComfyQuickMenu;
        public IEnumerator WaitForXRDevice()
        {
            yield return new WaitForEndOfFrame();
            if (XRDevice.isPresent == false)
            {
                yield break;
            }
            HarmonyInstance.Patch(typeof(QuickMenu).GetMethod(nameof(QuickMenu.Method_Private_Void_Boolean_0)), typeof(ComfyQM).GetMethod(nameof(SetupForDesktopOrHMDPatch), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod());
            HarmonyInstance.Patch(typeof(VRCUiCursorManager).GetMethod(nameof(VRCUiCursorManager.Method_Public_Static_Void_Boolean_Boolean_0)), typeof(ComfyQM).GetMethod(nameof(DualLaserPatch), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod());
            ComfyQuickMenu = MelonPreferences.CreateCategory("ComfyQM");
            ComfyToggle = ComfyQuickMenu.CreateEntry("ComfyQM Toggle", false);
            distance = ComfyQuickMenu.CreateEntry("Quick Menu Distance", 1.25f);
        }
        public override void OnApplicationStart()
        {
            MelonCoroutines.Start(WaitForXRDevice());
        }
        private static void DualLaserPatch(ref bool __1)
        {
            if (ComfyToggle.Value)
            {
                __1 = false;
            }
        }
        private static void SetupForDesktopOrHMDPatch(ref bool __0)
        {
            if (ComfyToggle.Value)
            {
                __0 = true;
            }
        }
        public override void OnUpdate()
        {
            if (XRDevice.isPresent == false)
            {
                return;
            }
            if (ComfyToggle.Value == false)
                return;
            VRCUiCursorManager.field_Private_Static_VRCUiCursorManager_0.field_Public_VRCUiCursor_2.gameObject.SetActive(true);
            VRCUiCursorManager.field_Private_Static_VRCUiCursorManager_0.field_Public_VRCUiCursor_3.gameObject.SetActive(true);
            var qm = QuickMenu.field_Private_Static_QuickMenu_0;
            if (qm == null)
                return;
            qm.transform.localPosition = new Vector3(0, -1f, distance.Value);
        }
    }
}