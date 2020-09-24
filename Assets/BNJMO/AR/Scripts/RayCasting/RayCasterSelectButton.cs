using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNJMO;

namespace BNJMO
{
    public class RayCasterSelectButton : BBehaviour
    {
        #region Public Events


        #endregion

        #region Public Methods
        public void OnSelectBAnchorPressed()
        {
            if (raycastedBAnchor)
            {
                raycastedBAnchor.OnRayInteract();
            }
        }

        #endregion

        #region Serialized Fields


        #endregion

        #region Private Variables
        private CameraRayCaster cameraRayCaster;
        private BButton bButton;
        private BAnchor raycastedBAnchor;

        #endregion

        #region Life Cycle
        protected override void OnValidate()
        {
            base.OnValidate();

            if (cameraRayCaster == null)
            {
                cameraRayCaster = FindObjectOfType<CameraRayCaster>();
            }
                        
            if (bButton == null)
            {
                bButton = GetComponent<BButton>();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (cameraRayCaster == null)
            {
                cameraRayCaster = FindObjectOfType<CameraRayCaster>();
            }

            if (IS_NOT_NULL(bButton))
            {
                bButton.DisableButton();
            }

            if (IS_NOT_NULL(cameraRayCaster))
            {
                cameraRayCaster.OnRayCastableEnter += On_CameraRayCaster_OnRayCastableEnter;
                cameraRayCaster.OnRayCastableExit += On_CameraRayCaster_OnRayCastableExit;
            }
        }

        private void On_CameraRayCaster_OnRayCastableEnter(IRayCastable rayCastable)
        {
            BAnchor bAnchor = (BAnchor)rayCastable;
            if (bAnchor)
            {
                raycastedBAnchor = bAnchor;

                if (IS_NOT_NULL(bButton))
                {
                    bButton.EnableButton();
                }
            }
        }

        private void On_CameraRayCaster_OnRayCastableExit(IRayCastable rayCastable)
        {
            BAnchor bAnchor = (BAnchor)rayCastable;
            if (bAnchor
                && bAnchor == raycastedBAnchor)
            {
                raycastedBAnchor = null;
                if (IS_NOT_NULL(bButton))
                {
                    bButton.DisableButton();
                }
            }
        }



        #endregion

        #region Events Callbacks


        #endregion

        #region Others


        #endregion
    }
}
