using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class FixRootBButton : BBehaviour
    {
        #region Public Events

        #endregion

        #region Public Methods and Getters

        #endregion

        #region Serialized Fields
        [SerializeField]
        private Color freeColor;

        [SerializeField]
        private Color fixedColor;

        #endregion

        #region Private Variables
        private BButton bButton;

        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            bButton = GetComponent<BButton>();
            if (IS_NOT_NULL(bButton))
            {
                bButton.ButtonPressed += On_BButton_ButtonPressed;
            }
        }

        protected override void Start()
        {
            base.Start();

            if (IS_NOT_NULL(bButton)
                && IS_NOT_NULL(ARManager.Instance))
            {
                bool fixWorldRoot = ARManager.Instance.FixWorldRoot;
                if (fixWorldRoot)
                {
                    bButton.SetButtonTextColor(fixedColor);
                }
                else
                {
                    bButton.SetButtonTextColor(freeColor);
                }
            }
        }
        #endregion

        #region Events Callbacks
        private void On_BButton_ButtonPressed(BButton obj)
        {
            if (IS_NOT_NULL(bButton)
                && IS_NOT_NULL(ARManager.Instance))
            {
                bool fixWorldRoot = ARManager.Instance.ToogleFixWorldRoot();
                if (fixWorldRoot)
                {
                    bButton.SetButtonTextColor(fixedColor);
                    bButton.SetButtonText("Root  Fixed");
                }
                else
                {
                    bButton.SetButtonTextColor(freeColor);
                    bButton.SetButtonText("Root  Free");
                }
            }
        }

        #endregion

        #region Others

        #endregion
    }
}
