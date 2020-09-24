using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace BNJMO
{
    [SelectionBase]
    public class BAnchor : BBehaviour, IRayCastable
    {
        #region Public Events
        public event Action<BAnchor> BAnchorRayHoverEnter;
        public event Action<BAnchor> BAnchorRayHoverExit;
        public event Action<BAnchor> BAnchorRaySelected;
        public event Action<BAnchor, string> BAnchorMovedToTracker; 
        #endregion

        #region Public Methods
        public string BAnchorID { get { return bAnchorID; } set { bAnchorID = value; } }

        public ENetworkID Owner { get { return owner; } set { owner = value; } }

        public string BindWithTrackerName { get { return bindWithTrackerName; } }

        /* IRayCastable */
        public void OnRayHoverEnter()
        {
            InvokeEventIfBound(BAnchorRayHoverEnter, this);
        }

        public void OnRayHoverExit()
        {
            InvokeEventIfBound(BAnchorRayHoverExit, this);
        }

        public void OnRayInteract()
        {
            InvokeEventIfBound(BAnchorRaySelected, this);
        }

        /* Transform getters and setters*/
        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public Quaternion GetRotation()
        {
            return transform.rotation;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public Vector3 GetTransformedPosition() 
        {
            if (ARManager.IsInstanceSet)
            {
                return ARManager.Instance.GetTransformedPosition(transform.position);
            }
            return Vector3.zero;
        }

        public Quaternion GetTransformedRotation()
        {
            if (ARManager.IsInstanceSet)
            {
                return ARManager.Instance.GetTransformedRotation(transform.rotation);
            }
            return Quaternion.identity;
        }

        public void SetTransformedPosition(Vector3 transformedPosition)
        {
            if (ARManager.IsInstanceSet)
            {
                transform.position = ARManager.Instance.WorldRootBAnchor.transform.TransformPoint(transformedPosition);
            }
        }

        public void SetTransformedRotation(Quaternion transformedRotation)
        {
            if (ARManager.IsInstanceSet)
            {
                transform.rotation = ARManager.Instance.GetInverseTransformedRotation(transformedRotation);
            }
        }

        public BAnchorInformation GetBAnchorInformation()
        {
            return new BAnchorInformation(BAnchorID, GetTransformedPosition(), GetTransformedRotation());
        }

        #endregion

        #region Serialized Fields
        //[SerializeField]
        //protected bool replicateMovement = false;

        [SerializeField]
        protected string bAnchorID = BConsts.None;

        [SerializeField]
        protected string bindWithTrackerName = BConsts.None;

        [SerializeField]
        [ReadOnly]
        protected ENetworkID owner = ENetworkID.LOCAL;

        #endregion

        #region Private Variables

        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            owner = ENetworkID.LOCAL; 
        }
        protected override void Start()
        {
            base.Start();

            BEventsCollection.AR_TrackerUpdated += On_AR_TrackerUpdated;
        }

        protected override void Update()
        {
            base.Update();

            //if (replicateMovement == true)
            //{
            //    // TODO: replicate movement
            //}
        }
        #endregion

        #region Events Callbacks
        protected virtual void On_AR_TrackerUpdated(BEHandle<string, Vector3, Quaternion> eHandle)
        {
            string trackerName = eHandle.Arg1;
            if (eHandle.Arg1 == BindWithTrackerName)
            {
                transform.position = eHandle.Arg2;
                transform.rotation = eHandle.Arg3;
                InvokeTrackerEvent();
            }
        }

        protected void InvokeTrackerEvent()
        {
            InvokeEventIfBound(BAnchorMovedToTracker, this, bindWithTrackerName);
        }

        #endregion

        #region Others

        #endregion
    }
}
