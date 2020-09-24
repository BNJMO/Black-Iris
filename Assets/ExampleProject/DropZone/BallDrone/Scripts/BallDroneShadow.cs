using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class BallDroneShadow : BBehaviour
    {
        #region Public Events


        #endregion

        #region Public Methods


        #endregion

        #region Inspector Variables
        [SerializeField]
        private BallDroneBAnchor ballDrone;

        #endregion

        #region Private Variables
        private MeshRenderer myMeshRenderer;
        private Vector3 downVector;

        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();
            
            myMeshRenderer = GetComponent<MeshRenderer>();

            // Set down vector
            downVector = Vector3.down;
            RectanglePlayArea rectanglePlayArea = FindObjectOfType<RectanglePlayArea>();
            if (rectanglePlayArea)
            {
                downVector = -rectanglePlayArea.UpVector;
            }

            // Detach transform
            transform.parent = null;
        }

        protected override void Update()
        {
            base.Update();

            if (ballDrone == null
                || myMeshRenderer == null)
            {
                return;
            }
             
            Ray ray = new Ray(ballDrone.transform.position, downVector);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                myMeshRenderer.enabled = true;
                transform.position = hit.point;
            }
        }

        #endregion

        #region Events Callbacks


        #endregion

        #region Others


        #endregion
    }
}
