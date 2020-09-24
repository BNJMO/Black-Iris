using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    [RequireComponent(typeof(BAnchorHoverVisualizer))]
    [RequireComponent(typeof(BAnchorCursorPlacer))]
    public class PlayAreaBAnchor : BAnchor
    {
        #region Public Events
        public event Action<PlayAreaBAnchor> StartedPlacing;
        public event Action<PlayAreaBAnchor> EndedPlacing;

        #endregion

        #region Public Methods
        public void OnStartPlacing()
        {
            OnRayInteract();
        }

        public void EnableReplacement()
        {
            if (IS_NOT_NULL(bAnchorCursorPlacer))
            {
                bAnchorCursorPlacer.CanPlaceBAnchor = true;
            }
        }
                
        public void DisableReplacement()
        {
            if (IS_NOT_NULL(bAnchorCursorPlacer))
            {
                bAnchorCursorPlacer.CanPlaceBAnchor = false;
            }
        }

        public void EnablePlacementRotation()
        {
            if (IS_NOT_NULL(bAnchorCursorPlacer))
            {
                bAnchorCursorPlacer.RotateBAnchor = true;
            }
        }
                
        public void DisablePlacementRotation()
        {
            if (IS_NOT_NULL(bAnchorCursorPlacer))
            {
                bAnchorCursorPlacer.RotateBAnchor = false;
            }
        }

        public void EndPlacing()
        {
            On_BAnchorCursorPlacer_EndedPlacing(this);
        }

        #endregion

        #region Serialized Fields
        [SerializeField] [ReadOnly] private BAnchorCursorPlacer bAnchorCursorPlacer;

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

            if (bAnchorCursorPlacer == null)
            {
                bAnchorCursorPlacer = GetComponent<BAnchorCursorPlacer>();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (bAnchorCursorPlacer == null)
            {
                bAnchorCursorPlacer = GetComponent<BAnchorCursorPlacer>();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (IS_NOT_NULL(bAnchorCursorPlacer))
            {
                bAnchorCursorPlacer.StartedPlacing += On_BAnchorCursorPlacer_StartedPlacing;
                bAnchorCursorPlacer.EndedPlacing += On_BAnchorCursorPlacer_EndedPlacing;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (IS_NOT_NULL(bAnchorCursorPlacer))
            {
                bAnchorCursorPlacer.StartedPlacing -= On_BAnchorCursorPlacer_StartedPlacing;
                bAnchorCursorPlacer.EndedPlacing -= On_BAnchorCursorPlacer_EndedPlacing;
            }
        }

        #endregion

        #region Events Callbacks
        private void On_BAnchorCursorPlacer_StartedPlacing(BAnchor bAnchor)
        {
            PlayAreaBAnchor playAreaBAnchor = (PlayAreaBAnchor)bAnchor;
            if (IS_NOT_NULL(playAreaBAnchor)
                && (ARE_EQUAL(playAreaBAnchor, this)))
            {
                InvokeEventIfBound(StartedPlacing, this);
            }    
        }

        private void On_BAnchorCursorPlacer_EndedPlacing(BAnchor bAnchor)
        {
            PlayAreaBAnchor playAreaBAnchor = (PlayAreaBAnchor)bAnchor;
            if (IS_NOT_NULL(playAreaBAnchor)
                && (ARE_EQUAL(playAreaBAnchor, this)))
            {
                InvokeEventIfBound(EndedPlacing, this);
            }    
        }
        #endregion

        #region Others


        #endregion
    }
}
