using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class WorldRootBAnchor : BAnchor
    {
        #region Public Events

        #endregion

        #region Public Methods and Getters

        #endregion

        #region Serialized Fields

        #endregion

        #region Private Variables

        #endregion

        #region Life Cycle
        protected override void Start()
        {
            base.Start();

            ARManager.Instance.SetWorldRootBAnchor(this);
        }
        #endregion

        #region Events Callbacks
        protected override void On_AR_TrackerUpdated(BEHandle<string, Vector3, Quaternion> eHandle)
        {
            if (ARManager.Instance.FixWorldRoot == false)
            {
                string trackerName = eHandle.Arg1;
                if (eHandle.Arg1 == BindWithTrackerName)
                {
                    transform.position = eHandle.Arg2;

                    // Keep rotation on x axis (pitch) the same 
                    Vector3 newRotation = eHandle.Arg3.eulerAngles;
                    transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, newRotation.y, transform.rotation.eulerAngles.z));

                    InvokeTrackerEvent();
                }
            }
        }
        #endregion

        #region Others

        #endregion
    }
}
