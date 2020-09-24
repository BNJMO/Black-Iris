using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace BNJMO
{
    [RequireComponent(typeof(BButton))]
    public class MenuNavigationButton : BBehaviour
    { 
        private string infoToMenuReferenceNotSet = "To Menu reference not set!";
        private bool showToMenuReferenceNotSet = false;
        [BoxGroup("MenuNavigationButton", centerLabel: true)] [InfoBox("$infoToMenuReferenceNotSet", InfoMessageType.Error, "showToMenuReferenceNotSet")] 
        [BoxGroup("MenuNavigationButton")] [SerializeField] [SceneObjectsOnly]  private BMenu toBMenu;
        [BoxGroup("MenuNavigationButton")] [SerializeField] private bool overrideBButtonUIName = true;
        [BoxGroup("MenuNavigationButton")] [ShowInInspector] [ReadOnly] private BButton bButtonReference;

        protected override void OnValidate()
        {
            base.OnValidate();

            if (CanValidate() == false)
            {
                return;
            }

            bButtonReference = GetComponent<BButton>();
            if (bButtonReference)
            {
                if (toBMenu)
                {
                    showToMenuReferenceNotSet = false;

                    if (overrideBButtonUIName == true)
                    {
                        bButtonReference.UIElementName = "B_ToM_" + toBMenu.UIElementName;
                    }
                }
                else
                {
                    showToMenuReferenceNotSet = true;
                    LogConsoleError(infoToMenuReferenceNotSet);
                }
            }
        }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            bButtonReference = GetComponent<BButton>();
        }

        protected override void InitializeEventsCallbacks()
        {
            base.InitializeEventsCallbacks();

            if (IS_NOT_NULL(bButtonReference))
            {
                bButtonReference.ButtonReleased += On_ButtonReleased;
            }
        }

        private void On_ButtonReleased(BButton bButton, bool cursorOutside)
        {
            if ((cursorOutside == true)
                && (IS_NOT_NULL(bButtonReference))
                && (IS_NOT_NULL(bButtonReference.ParentBFrame)))
            {
                bButtonReference.ParentBFrame.UpdateCurrentBMenu(toBMenu);
            }
        }
    }
}
