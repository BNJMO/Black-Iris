using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class BAnchorReplication : BBehaviour
    {
        #region Public Events


        #endregion

        #region Public Methods


        #endregion

        #region Inspector Variables
        [SerializeField]
        private BAnchor bAnchor;

        [SerializeField]
        [ReadOnly]
        private string bAnchorID = "None";

        //[SerializeField]
        //private float replicationIntervall = 0.07f;

        [SerializeField]
        private bool interpolateReplication = true;

        [SerializeField]
        [DisableIf("@this.interpolateReplication == false")]
        private float interpolationFactor = 18.0f;


        #endregion

        #region Private Variables
        private Vector3 inverseRepTransformedPosition;
        private Quaternion inverseRepTransformedRotation;
        //private float lastTimeReplicated;

        #endregion

        #region Life Cycle
        protected override void OnValidate()
        {
            base.OnValidate();

            if (CanValidate())
            {
                if (bAnchor == null)
                {
                    bAnchor = GetComponentInHierarchy<BAnchor>();
                }

                if (bAnchor != null)
                {
                    bAnchorID = bAnchor.BAnchorID;
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            BEventsCollection.AR_BAnchorReplicateTransform += On_AR_BAnchorReplicateTransform; ;
        }
               
        protected override void OnDisable()
        {
            base.OnDisable();

            BEventsCollection.AR_BAnchorReplicateTransform -= On_AR_BAnchorReplicateTransform;
        }

        protected override void Awake()
        {
            base.Awake();

            inverseRepTransformedPosition = transform.position;
            inverseRepTransformedRotation = transform.rotation;
        }

        protected override void Update()
        {
            base.Update();

            if (bAnchor
                && BEventManager.Instance.LocalNetworkID != ENetworkID.LOCAL)
            {
                // Is Owner?
                if (bAnchor.Owner == BEventManager.Instance.LocalNetworkID)
                {
                    //if (Time.time - lastTimeReplicated > replicationIntervall)
                    //{
                        ReplicateTransform();
                        //lastTimeReplicated = Time.time;
                    //}
                }
                else if (interpolateReplication == true)
                {
                    transform.position = Vector3.Lerp(transform.position, inverseRepTransformedPosition, Time.deltaTime * interpolationFactor);
                    transform.rotation = Quaternion.Lerp(transform.rotation, inverseRepTransformedRotation, Time.deltaTime * interpolationFactor);
                }
            }
        }
        #endregion

        #region Events Callbacks
        private void On_AR_BAnchorReplicateTransform(BEHandle<BAnchorInformation, string> handle)
        {
            ENetworkID callingNetworkID = handle.InvokingNetworkID;
            if (bAnchorID == handle.Arg2
                && callingNetworkID == bAnchor.Owner
                && ARE_NOT_EQUAL(callingNetworkID, BEventManager.Instance.LocalNetworkID)
                && IS_NOT_NULL(bAnchor))
            {
                BAnchorInformation bAnchorInformation = handle.Arg1;
                if (interpolateReplication == false)
                {
                    bAnchor.SetTransformedPosition(bAnchorInformation.TransformedPosition);
                    bAnchor.SetTransformedRotation(bAnchorInformation.TransformedRotation);
                }
                else
                {
                    inverseRepTransformedPosition = ARManager.Instance.GetInverseTransformedPosition(bAnchorInformation.TransformedPosition);
                    inverseRepTransformedRotation = ARManager.Instance.GetInverseTransformedRotation(bAnchorInformation.TransformedRotation);
                }
            }
        }

        #endregion

        #region Others
        private void ReplicateTransform()
        {
            BAnchorInformation bAnchorInformation = bAnchor.GetBAnchorInformation();
            BEventsCollection.AR_BAnchorReplicateTransform.Invoke(new BEHandle<BAnchorInformation, string>(bAnchorInformation, bAnchorID), BEventReplicationType.TO_ALL_OTHERS, false);
        }

        #endregion
    }
}
