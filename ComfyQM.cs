using System.Collections;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Animations;
using VRC.UI.Core;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib.XrefScans;
using System;
using Harmony;
using System.Linq;

namespace ComfyQM_Standalone
{
    public class ComfyQM : MelonMod
    {
        private GameObject RightHand;
        private GameObject LeftHand;
        private GameObject QuickMenuObject;
        public static MelonPreferences_Entry<bool> ComfyToggle;
        public static MelonPreferences_Entry<bool> RotationToggle;
        public static MelonPreferences_Category ComfyQuickMenu;

        //public static MelonPreferences_Entry<bool> uncomfyQM; //a person in vrcmg (「Spooks」#4894) asked for this, dont blame me for this

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
            RotationToggle = ComfyQuickMenu.CreateEntry("Menu Rotation", false);

            //uncomfyQM = ComfyQuickMenu.CreateEntry("uncomfies your quickmenu", false);

            MelonCoroutines.Start(WaitForUIMan());

            //dubya is amazing and helped with the xref, ty dubya

            var methodInfo = typeof(VRC.UI.Elements.QuickMenu).GetMethods().Where(m =>
                    m.Name.StartsWith("Method_Private_Boolean") &&
                    //Possible future proof for 'Potentially Dead Method (as pre Requi)
                    !m.Name.Contains("PDM") &&
                    //The two bool methods are IsAttachedToRightHand and IsAttachedToHand
                    //IsAttachedToHand (Method_Private_Boolean_2) has Xrefs, IsAttachedToRightHand does not
                    //This is a rather temporary fix as a fake method of IsAttachedToHand could 'pass' this check
                    XrefScanner.XrefScan(m).Count() > 0
                //Single will throw an error if there are more then 1 items
                ).Single();
            HarmonyInstance.Patch(methodInfo, typeof(ComfyQM).GetMethod(nameof(IsAttachedToHandPatch), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod());

        }

        public void OnUIManInit()
        {
            LeftHand = VRCUiCursorManager.field_Private_Static_VRCUiCursorManager_0.transform.Find("DotLeftHand").gameObject;
            RightHand = VRCUiCursorManager.field_Private_Static_VRCUiCursorManager_0.transform.Find("DotRightHand").gameObject;
            var EDL = QuickMenuObject.AddComponent<EnableDisableListener>();
            EDL.OnEnableEvent += QMCheck;
        }

        [RegisterTypeInIl2Cpp]
        public class EnableDisableListener : MonoBehaviour
        {
            [method: HideFromIl2Cpp]
            public event Action OnEnableEvent;
            [method: HideFromIl2Cpp]
            public event Action OnDisableEvent;
            public EnableDisableListener(IntPtr obj) : base(obj) { }
            public void OnEnable() => OnEnableEvent?.Invoke();
            public void OnDisable() => OnDisableEvent?.Invoke();
        }

        public void QMCheck()
        {
            if (!XRDevice.isPresent)
            {
                return;
            }
            if (RotationToggle.Value == true)
            {
                //takes rotation of head/Camera (eye)
                var NeckRotate = VRCPlayer.field_Internal_Static_VRCPlayer_0.gameObject.GetComponent<GamelikeInputController>().field_Protected_NeckMouseRotator_0.transform.Find("Camera (eye)").transform;
                //gets QM rotation
                var QMAngle = QuickMenuObject.transform.rotation.eulerAngles;
                //reserves x and y rotations, swaps z
                var NewAngle = new Vector3(QMAngle.x, QMAngle.y, NeckRotate.rotation.eulerAngles.z);
                //applies the z rotation
                QuickMenuObject.transform.rotation = Quaternion.Euler(NewAngle);
            }
        }
        //work in progress thing
        /*public void UnComfy()
        {
            var WristSource = GameObject.Find("_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Controller (left)/WristOrigin");

            if (WristSource == null)
            {
                return;
            }

            var parentConstraint = QuickMenuObject.GetComponent<ParentConstraint>();
            if (parentConstraint == null)
            {
                parentConstraint = QuickMenuObject.AddComponent<ParentConstraint>();
                ConstraintSource Wrist = new ConstraintSource();
                Wrist.sourceTransform = WristSource.transform;
                Wrist.weight = 1;
                parentConstraint.AddSource(Wrist);
                parentConstraint.weight = 1;
                //parentConstraint.SetRotationOffset(0, new Vector3());
                parentConstraint.constraintActive = true;
            }
            else
            {
                parentConstraint.constraintActive = true;
            }

        }*/

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
            /*if (uncomfyQM.Value == true)
            {
                ComfyToggle.Value = false;
                UnComfy();
                return;
            }
            else
            {
                var parentConstraint = QuickMenuObject.GetComponent<ParentConstraint>();
                if (parentConstraint != null)
                {
                    parentConstraint.constraintActive = false;
                }

            }*/

        }
        
        private static bool IsAttachedToHandPatch(ref bool __result)
        {
            if (!XRDevice.isPresent)
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