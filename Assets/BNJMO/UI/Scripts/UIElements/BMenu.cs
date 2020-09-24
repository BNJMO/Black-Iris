using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace BNJMO
{
    [Serializable]
    public class BBMenuChildBButton
    {
        public BBMenuChildBButton(BButton bButton, BMenu bMenu)
        {
            childBButton = bButton;
            bMenuReference = bMenu;
        }

        [SerializeField] [ReadOnly] BButton childBButton;
        [TableColumnWidth(90, resizable: false)]
        [Button("Set"), VerticalGroup("Start Highlight")]
        public void Button_StartHighlight()
        {
            if (bMenuReference)
            {
                bMenuReference.SetStartHighlightedBButton(childBButton);
            }
        }

        private BMenu bMenuReference;
    }

    public class BMenu : BUIElement
    {
        public bool IsActive { get; private set; }

        private string infoNoStartBButtonHighlight = "You might need to select one of the children BButton as Start Hihghlight.";
        private bool showNoStartButtonHighlight = false;
        [BoxGroup("BMenu", centerLabel: true)]
        [BoxGroup("BMenu")] [Button("Highlight this BMenu")] private void Button_HighlightThis()
        {
            if (parentBFrame)
            {
                parentBFrame.UpdateCurrentBMenu(this);
            }
        }
        [BoxGroup("BMenu")] [InfoBox("$infoNoStartBButtonHighlight", InfoMessageType.Info, "showNoStartButtonHighlight")]
        [BoxGroup("BMenu")] [SerializeField] [ChildGameObjectsOnly] private BButton startHighlightedBButton;
        [Title("Debugging")]
        [BoxGroup("BMenu")] [SerializeField] [ReadOnly] private BButton highlightedBButtonReference;
        [BoxGroup("BMenu")] [TableList(DrawScrollView = true)] public List<BBMenuChildBButton> childrenBButtonsList = new List<BBMenuChildBButton>();

        private BButton[] childrenBButtons = new BButton[0];

        protected override void OnValidate()
        {
            objectNamePrefix = "M_";

            base.OnValidate();

            if (CanValidate() == false)
            {
                return;
            }

            // Get all children BMenu
            childrenBButtons = GetComponentsInChildren<BButton>();
            childrenBButtonsList = new List<BBMenuChildBButton>();
            foreach (BButton bButton in childrenBButtons)
            {
                childrenBButtonsList.Add(new BBMenuChildBButton(bButton, this));
            }


            // Check Start Highlight BMenu
            highlightedBButtonReference = null;
            if (startHighlightedBButton)
            {
                showNoStartButtonHighlight = false;
                highlightedBButtonReference = startHighlightedBButton;
            }
            else
            {
                if ((childrenBButtons.Length > 0)
                    && (childrenBButtons[0]))
                {
                    startHighlightedBButton = childrenBButtons[0];
                    showNoStartButtonHighlight = false;
                    highlightedBButtonReference = startHighlightedBButton;
                }
                else
                {
                    //LogConsole(infoNoStartBButtonHighlight);
                    showNoStartButtonHighlight = true;
                }
            }
        }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            // Check Start Highlight BButton
            highlightedBButtonReference = startHighlightedBButton;
            //if (highlightedBButtonReference == null)
            //{
            //    LogConsoleError("No BButton selected as highlight!");
            //}
        }

        protected override void OnUIElementHidden()
        {
            IsActive = false;
        }

        public void OnBecameActive()
        {
            IsActive = true;

            ShowUIElement();

            if (BEventsCollection.IsInstanceSet)
            {
                BEventsCollection.UI_HighlightedBMenuUpdated.Invoke(new BEHandle<BMenu>(this), MotherOfManagers.Instance.DebugUIButtonsEvents);
            }

            if (startHighlightedBButton)
            {
                startHighlightedBButton.OnHighlighted();
                highlightedBButtonReference = startHighlightedBButton;
            }
        }
                
        public void OnBecameInactive()
        {
            IsActive = false;

            HideUIElement();
        }

        public void OnBButtonPressed(BButton bButton)
        {
            if (IS_NOT_NULL(bButton))
            {
                if (bButton != highlightedBButtonReference)
                {
                    if (highlightedBButtonReference)
                    {
                        highlightedBButtonReference.OnUnhighlighted();
                    }
                    highlightedBButtonReference = bButton;
                }
            }
        }

        public void OnBButtonHighlighted(BButton bButton)
        {
            if (IS_NOT_NULL(bButton))
            {
                if (bButton != highlightedBButtonReference)
                {
                    if (highlightedBButtonReference)
                    {
                        highlightedBButtonReference.OnUnhighlighted();
                    }
                    highlightedBButtonReference = bButton;
                }
            }
        }

        public void SetStartHighlightedBButton(BButton bButton)
        {
            if (IS_NOT_NULL(bButton))
            {
                if (highlightedBButtonReference)
                {
                    highlightedBButtonReference.OnUnhighlighted();
                }

                startHighlightedBButton = bButton;
                highlightedBButtonReference = bButton;

                highlightedBButtonReference.OnHighlighted();
            }
        }

    }
}
