using System.Collections;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.XR;
using VRC.UI.Core;

namespace ComfyQM_Standalone
{
    public class ComfyQM : MelonMod
    {
        private GameObject RightHand;
        private GameObject LeftHand;
        private GameObject QuickMenuObject;
        public static MelonPreferences_Entry<bool> ComfyToggle;
        public static MelonPreferences_Category ComfyQuickMenu;

        public IEnumerator WaitForUIMan()
        {
            while (VRCUiManager.field_Private_Static_VRCUiManager_0 == null) yield return null;
            while (UIManager.field_Private_Static_UIManager_0 == null) yield return null;
            while (GameObject.Find("UserInterface").GetComponentInChildren<VRC.UI.Elements.QuickMenu>(true) == null) yield return null;
            QuickMenuObject = GameObject.Find("UserInterface").GetComponentInChildren<VRC.UI.Elements.QuickMenu>(true).gameObject;
            OnUIManInit();
        }


        public override void OnApplicationStart()
        {
            ComfyQuickMenu = MelonPreferences.CreateCategory("ComfyQM");
            ComfyToggle = ComfyQuickMenu.CreateEntry("ComfyQM Toggle", false);

            MelonCoroutines.Start(WaitForUIMan());
            
            HarmonyInstance.Patch(typeof(VRC.UI.Elements.QuickMenu).GetMethod(nameof(VRC.UI.Elements.QuickMenu.Method_Private_Boolean_0)),
                typeof(ComfyQM).GetMethod(nameof(IsAttachedToHandPatch), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod());
            HarmonyInstance.Patch(typeof(VRC.UI.Elements.QuickMenu).GetMethod(nameof(VRC.UI.Elements.QuickMenu.Method_Private_Boolean_1)),
                typeof(ComfyQM).GetMethod(nameof(IsAttachedToHandPatch), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod());

        }

        public void OnUIManInit()
        {
            LeftHand = VRCUiCursorManager.field_Private_Static_VRCUiCursorManager_0.transform.Find("DotLeftHand").gameObject;
            RightHand = VRCUiCursorManager.field_Private_Static_VRCUiCursorManager_0.transform.Find("DotRightHand").gameObject;
        }

        public override void OnUpdate()
        { 
            if (LeftHand == null || RightHand == null)
            {
                return;
            }
            if (!QuickMenuObject.active)
            {
                return;
            }
            if (ComfyToggle.Value == true)
            {
                LeftHand.SetActive(true);
                RightHand.SetActive(true);
                VRCUiCursorManager.field_Private_Static_VRCUiCursorManager_0.field_Private_Boolean_2 = false;
                VRCUiCursorManager.field_Private_Static_VRCUiCursorManager_0.field_Private_Boolean_7 = false;
            }
        }

        private static bool IsAttachedToHandPatch(ref bool __result)
        {
            if (XRDevice.isPresent == false)
            {
                return true;
            }
            if (ComfyToggle.Value)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}