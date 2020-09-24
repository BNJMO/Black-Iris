using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public enum EARCursorState
{
    NOT_INITIALIZED = 0,
    NOT_TRACKING = 1,
    TRACKING = 2,
}

namespace BNJMO
{
    public class ARCursor : AbstractSingletonManager<ARCursor>
    {
        #region Public Events


        #endregion

        #region Public Methods and Getters
        public EARCursorState State { get; private set; } = EARCursorState.NOT_INITIALIZED;

        public Vector3 GetCursorPosition()
        {
            return transform.position;
        }
              
        public Quaternion GetCursorRotation()
        {
            return transform.rotation;
        }
        #endregion

        #region Serialized Fields

        #endregion

        #region Private Variables
        private ARRaycastManager raycastManager;
        private MeshRenderer myMeshRenderer;

        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            raycastManager = FindObjectOfType<ARRaycastManager>();

            myMeshRenderer = GetComponentInChildren<MeshRenderer>();
            if (IS_NOT_NULL(myMeshRenderer))
            {
                myMeshRenderer.enabled = false;
            }

            if (IS_NOT_NULL(raycastManager)
               && IS_NOT_NULL(myMeshRenderer))
            {
                State = EARCursorState.NOT_TRACKING;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (State != EARCursorState.NOT_INITIALIZED)
            {
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                raycastManager.Raycast(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f), hits, TrackableType.Planes);

                if (hits.Count > 0)
                {
                    transform.position = hits[0].pose.position;
                    transform.rotation = hits[0].pose.rotation;

                    if (State == EARCursorState.NOT_TRACKING)
                    {
                        myMeshRenderer.enabled = true;
                        State = EARCursorState.TRACKING;
                    }
                }
                else
                {
                    if (State == EARCursorState.NOT_TRACKING)
                    {
                        myMeshRenderer.enabled = false;
                        State = EARCursorState.NOT_TRACKING;
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
