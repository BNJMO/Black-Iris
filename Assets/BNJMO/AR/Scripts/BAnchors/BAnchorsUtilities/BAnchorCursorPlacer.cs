using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class BAnchorCursorPlacer : BBehaviour
    {
        #region Public Events
        public event Action<BAnchor> StartedPlacing;
        public event Action<BAnchor> EndedPlacing;

        #endregion

        #region Public Methods
        public bool IsPlacing { get; private set; } = false;
        public bool CanPlaceBAnchor { get; set; } = true;
        public bool RotateBAnchor { get; set; } = true;

        #endregion

        #region Serialized Fields
        [SerializeField] private BAnchor bAnchor;
        [SerializeField] private float interpolationFactor = 10.0f;

        #endregion

        #region Private Variables


        #endregion

        #region Life Cycle
        protected override void OnValidate()
        {
            base.OnValidate();

            if (CanValidate() == false)
            {
                return;
            }

            if (bAnchor == null)
            {
                bAnchor = GetComponentInHierarchy<BAnchor>();
            }
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();

            if (IS_NOT_NULL(bAnchor))
            {
                bAnchor.BAnchorRaySelected += On_BAnchor_BAnchorRaySelected;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (bAnchor)
            {
                bAnchor.BAnchorRaySelected -= On_BAnchor_BAnchorRaySelected;
                if (ARManager.Instance)
                {
                    ARManager.Instance.OnStopPlacingBAnchor(this, bAnchor);
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            if (IsPlacing == true
                && ARCursor.Instance)
            {
                Vector3 lerpedPosition = Vector3.Lerp(bAnchor.GetPosition(), ARCursor.Instance.GetCursorPosition(), Time.deltaTime * interpolationFactor);
                bAnchor.SetPosition(lerpedPosition);

                if (RotateBAnchor == true)
                {
                    Quaternion lerpedRotation = Quaternion.Lerp(bAnchor.GetRotation(), ARCursor.Instance.GetCursorRotation(), Time.deltaTime * interpolationFactor);
                    bAnchor.SetRotation(lerpedRotation);
                }
            }
        }

        #endregion

        #region Events Callbacks
        private void On_BAnchor_BAnchorRaySelected(BAnchor bAnchor)
        {
            if (CanPlaceBAnchor == true
                && IS_NOT_NULL(ARCursor.Instance))
            {
                bool succeeded = false;
                if (IsPlacing == false)
                {
                    succeeded = ARManager.Instance.OnStartPlacingBAnchor(this, bAnchor);
                }
                else
                {
                    succeeded = ARManager.Instance.OnStopPlacingBAnchor(this, bAnchor);
                }

                if (succeeded == true)
                {
                    if (IsPlacing == false)
                    {
                        IsPlacing = true;
                        InvokeEventIfBound(StartedPlacing, bAnchor);
                    }
                    else
                    {
                        IsPlacing = false;
                        InvokeEventIfBound(EndedPlacing, bAnchor);
                    }
                }
            }
        }


        #endregion

        #region Others


        #endregion
    }
}
