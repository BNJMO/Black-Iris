using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class CameraRayCaster : BBehaviour
    {
        #region Public Events
        public event Action<IRayCastable> OnRayCastableEnter;
        public event Action<IRayCastable> OnRayCastableStay;
        public event Action<IRayCastable> OnRayCastableExit;

        #endregion

        #region Public Methods
        public void CastRay()
        {
            if (IS_NOT_NULL(mainCamera))
            {
                Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

                if (castMultipleRays == true)
                {
                    RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, layerMask);
                    foreach (RaycastHit hit in hits)
                    {
                        ProcessRaycastHit(hit);
                    }
                }
                else
                {
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
                    {
                        ProcessRaycastHit(hit);
                    }
                }

                if (drawDebugLine == true)
                {
                    Debug.DrawLine(ray.origin, ray.origin + ray.direction * maxDistance, Color.red, debugLineDuration);
                }
            }
        }
        
        #endregion

        #region Serialized Fields
        [SerializeField] private Camera mainCamera;
        [SerializeField] private bool castRayOnUpdate = false;
        [SerializeField] private bool castMultipleRays = false;
        [SerializeField] private bool drawDebugLine = false;
        [SerializeField] [DisableIf("@this.drawDebugLine == false")] private float debugLineDuration = 0.5f;
        [SerializeField] private float maxDistance = float.MaxValue;
        [SerializeField] private LayerMask layerMask;

        #endregion

        #region Private Variables
        /// <summary>
        /// bool is true only when the IRayCastable is added during this frame.
        /// </summary>
        private Dictionary<IRayCastable, bool> rayCastablesMap = new Dictionary<IRayCastable, bool>();

        #endregion

        #region Life Cycle
        protected override void OnValidate()
        {
            base.OnValidate();

            if (mainCamera == null)
            {
                foreach (Camera camera in FindObjectsOfType<Camera>())
                {
                    if (camera
                        && camera.tag == "MainCamera")
                    {
                        mainCamera = camera;
                        break;
                    }
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            if (castRayOnUpdate == true)
            {
                CastRay();
            }

            UpdateRayCastablesMap();
        }         
        #endregion

        #region Events Callbacks


        #endregion

        #region Others
        private void ProcessRaycastHit(RaycastHit hit)
        {
            IRayCastable rayCastable = BUtils.GetComponentInHierarchy<IRayCastable>(hit.transform.gameObject, true);
            if (rayCastable != null)
            {
                // Enter
                if (rayCastablesMap.ContainsKey(rayCastable) == false)
                {
                    rayCastable.OnRayHoverEnter();
                    rayCastablesMap.Add(rayCastable, true);
                    InvokeEventIfBound(OnRayCastableEnter, rayCastable);
                }
                // Stay
                else if (rayCastablesMap.ContainsKey(rayCastable) == true
                    && rayCastablesMap[rayCastable] == false)
                {
                    rayCastablesMap[rayCastable] = true;
                    InvokeEventIfBound(OnRayCastableStay, rayCastable);
                }
            }
        }

        private void UpdateRayCastablesMap()
        {
            // Update rayhovers statuses
            List<IRayCastable> rayCastablesToPend = new List<IRayCastable>();
            List<IRayCastable> rayCastablesToRemove = new List<IRayCastable>();

            foreach (IRayCastable rayCastable in rayCastablesMap.Keys)
            {
                if (rayCastable != null)
                {
                    if (rayCastablesMap[rayCastable] == true)
                    {
                        rayCastablesToPend.Add(rayCastable);
                    }
                    else
                    {
                        rayCastable.OnRayHoverExit();
                        rayCastablesToRemove.Add(rayCastable);
                    }
                }
                else
                {
                    rayCastablesToRemove.Add(rayCastable);
                }
            }

            // Revalidate rayhovers
            foreach (IRayCastable rayCastableToPend in rayCastablesToPend)
            {
                rayCastablesMap[rayCastableToPend] = false;
            }

            // Remove rayhovers (Exit)
            foreach (IRayCastable rayCastableToRemove in rayCastablesToRemove)
            {
                InvokeEventIfBound(OnRayCastableExit, rayCastableToRemove);
                rayCastablesMap.Remove(rayCastableToRemove);
            }
        }
        #endregion
    }
}
