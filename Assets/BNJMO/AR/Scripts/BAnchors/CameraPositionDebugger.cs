using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class CameraPositionDebugger : BBehaviour
    {
        #region Public Events

        #endregion

        #region Public Methods and Getters

        #endregion

        #region Serialized Fields
        [SerializeField] private CameraBAnchor cameraBAnchor;
        [SerializeField] private BText positionText;
        [SerializeField] private BText transformedPositionText;
        #endregion

        #region Private Variables

        #endregion

        #region Life Cycle
        protected override void Update()
        {
            base.Update();

            if (cameraBAnchor)
            {
                if (positionText)
                {
                    positionText.SetText(cameraBAnchor.transform.position.ToString());
                }
                if (transformedPositionText)
                {
                    transformedPositionText.SetText(cameraBAnchor.GetTransformedPosition().ToString());
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
