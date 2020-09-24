using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class BAnchorHoverVisualizer : BBehaviour
    {
        #region Public Events


        #endregion

        #region Public Methods


        #endregion

        #region Serialized Fields
        [SerializeField] private BAnchor bAnchor;
        [SerializeField] private GameObject boundingBox;

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

        protected override void Awake()
        {
            base.Awake();

            if (boundingBox)
            {
                boundingBox.SetActive(false);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (IS_NOT_NULL(bAnchor))
            {
                bAnchor.BAnchorRayHoverEnter += On_BAnchor_BAnchorRayHoverEnter;
                bAnchor.BAnchorRayHoverExit += On_BAnchor_BAnchorRayHoverExit;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (bAnchor)
            {
                bAnchor.BAnchorRayHoverEnter -= On_BAnchor_BAnchorRayHoverEnter;
                bAnchor.BAnchorRayHoverExit -= On_BAnchor_BAnchorRayHoverExit;
            }
        }
        #endregion

        #region Events Callbacks
        private void On_BAnchor_BAnchorRayHoverEnter(BAnchor bAnchor)
        {
            if (boundingBox)
            {
                boundingBox.SetActive(true);
            }
        }

        private void On_BAnchor_BAnchorRayHoverExit(BAnchor bAnchor)
        {
            if (boundingBox)
            {
                boundingBox.SetActive(false);
            }
        }

        #endregion

        #region Others


        #endregion
    }
}
