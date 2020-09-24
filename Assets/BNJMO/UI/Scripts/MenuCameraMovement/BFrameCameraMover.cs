using System.Collections.Generic;
using UnityEngine;
using System;

namespace BNJMO
{
    public class BFrameCameraMover : BBehaviour
    {
        public event Action<BFrame> CameraMovementEnded;

        [SerializeField] private bool canBeStopped = false;

        private Dictionary<BFrame, BFrameCameraTransform> cameraTransforms = new Dictionary<BFrame, BFrameCameraTransform>();
        private AnimationLerpTransform cameraTransformLerp;
        private BFrameCameraTransform lastCameraTransform;

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            cameraTransformLerp = GetComponentWithCheck<AnimationLerpTransform>();
        }

        protected override void InitializeObjecsInScene()
        {
            base.InitializeObjecsInScene();

            // Find Menu Camera Transform in scene
            foreach (BFrameCameraTransform menuCameraTransform in FindObjectsOfType<BFrameCameraTransform>())
            {
                if (IS_KEY_NOT_CONTAINED(cameraTransforms, menuCameraTransform.BFrame))
                {
                    cameraTransforms.Add(menuCameraTransform.BFrame, menuCameraTransform);

                    //if (menuCameraTransform.BFrame == EUIState.NONE)
                    //{
                    //    lastCameraTransform = menuCameraTransform;
                    //}
                }
            }
        }


        protected override void InitializeEventsCallbacks()
        {
            base.InitializeEventsCallbacks();

            BEventsCollection.UI_FocusedFrameUpdated += On_UI_FocusedFrameUpdated;

            if (IS_NOT_NULL(cameraTransformLerp))
            {
                cameraTransformLerp.AnimationEnded += On_LerpTransform_AnimationEnded;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (BEventsCollection.IsInstanceSet)
            {
                BEventsCollection.UI_FocusedFrameUpdated.Event -= On_UI_FocusedFrameUpdated;
            }

            if (cameraTransformLerp)
            {
                cameraTransformLerp.AnimationEnded -= On_LerpTransform_AnimationEnded;
            }
        }

        private void On_UI_FocusedFrameUpdated(BEHandle<BFrame> bEHandle)
        {
            BFrame bFrame = bEHandle.Arg1;

            if (cameraTransforms.ContainsKey(bFrame))
            {
                BFrameCameraTransform newCameraTransform = cameraTransforms[bFrame];
                cameraTransformLerp.StartValue = lastCameraTransform.transform;
                cameraTransformLerp.EndValue = newCameraTransform.transform;
                cameraTransformLerp.PlayDuration = newCameraTransform.TransitionTime;
                cameraTransformLerp.StartAnimation();

                lastCameraTransform = newCameraTransform;
            }
        }

        private void On_LerpTransform_AnimationEnded(AnimationLerp<Transform> animationLerp)
        {
            if (UIManager.IsInstanceSet)
            {
                InvokeEventIfBound(CameraMovementEnded, UIManager.Instance.CurrentBFraneFocused);
            }
        }
    }
}