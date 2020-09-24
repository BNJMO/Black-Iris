using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class BAnchorPositionDebugger : BBehaviour
    {
        #region Public Events

        #endregion

        #region Public Methods and Getters

        #endregion

        #region Serialized Fields
        [SerializeField] private BAnchor bAnchor;
        [SerializeField] private BText debugText;
        [SerializeField] private bool debugRotationInstead = false;
        [SerializeField] private bool debugReuntransformed = false;
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
                bAnchor = GetComponent<BAnchor>();
            }
            if (debugText == null)
            {
                debugText = GetComponent<BText>();
            }
        }

        protected override void Update()
        {
            base.Update();

            if (bAnchor
                && debugText)
            {
                if (debugReuntransformed == false)
                {
                    if (debugRotationInstead == false)
                    {
                        debugText.SetText(bAnchor.GetTransformedPosition().ToString());
                    }
                    else
                    {
                        debugText.SetText(bAnchor.GetTransformedRotation().eulerAngles.ToString());
                    }
                }
                else
                {
                    if (debugRotationInstead == false)
                    {
                        Vector3 reuntransformedPosition = ARManager.Instance.WorldRootBAnchor.transform.TransformPoint(bAnchor.GetTransformedPosition());
                        debugText.SetText(reuntransformedPosition.ToString());
                    }
                    else
                    {
                        Quaternion reuntransformedRotation = ARManager.Instance.WorldRootBAnchor.transform.rotation * bAnchor.GetTransformedRotation();
                        debugText.SetText(reuntransformedRotation.eulerAngles.ToString());
                    }
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
