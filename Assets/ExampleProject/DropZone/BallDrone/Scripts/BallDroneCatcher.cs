using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class BallDroneCatcher : BBehaviour
    {
        #region Public Events
        public bool IsBallDroneCaught { get { return caughtBallDrone != null; } }

        #endregion

        #region Public Methods


        #endregion

        #region Inspector Variables
        [SerializeField]
        private DropZonePlayer dZPlayer;

        [SerializeField]
        [ReadOnly]
        private EPlayerID playerID = EPlayerID.NONE;

        [SerializeField]
        [ReadOnly]
        private ENetworkID owner = ENetworkID.NONE;

        [SerializeField]
        private float releaseForceIntensity = 10.0f;

        [SerializeField]
        private float disableColliderDuration = 1.0f;

        [SerializeField]
        private float releaseVelocityThreshold = 0.2f;

        [SerializeField]
        [Range(-1.0f, 1.0f)]
        private float forwardDotThreshold = 0.7f;

        [Button("Release Ball")]
        private void Button_ReleaseBall() { ReleaseBall(BUtils.GetRandomVector(1.0f, 1.0f)); }

        #endregion

        #region Private Variables
        private Renderer myRenderer;
        private Collider myCollider;
        private BallDroneBAnchor caughtBallDrone = null;
        private Vector3 lastPosition;
        private Vector3 currentVelocity;
        private Vector3 playAreaUpVector = Vector3.up;

        private float maxVelocity;

        #endregion

        #region Life Cycle
        protected override void OnValidate()
        {
            base.OnValidate();

            if (CanValidate())
            {
                if (dZPlayer == null)
                {
                    dZPlayer = GetComponentInHierarchy<DropZonePlayer>();
                    playerID = dZPlayer.PlayerID;
                    owner = dZPlayer.GetOwner();
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            BEventsCollection.AR_PlayAreaStateUpdated += On_AR_PlayAreaStateUpdated;
            BEventsCollection.DZ_BallDroneCaught += On_DZ_BallDroneCaught;
            BEventsCollection.DZ_BallDroneReleased += On_DZ_BallDroneReleased;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            BEventsCollection.AR_PlayAreaStateUpdated -= On_AR_PlayAreaStateUpdated;
            BEventsCollection.DZ_BallDroneCaught -= On_DZ_BallDroneCaught;
            BEventsCollection.DZ_BallDroneReleased -= On_DZ_BallDroneReleased;
        }

        protected override void Awake()
        {
            base.Awake();

            myRenderer = GetComponent<MeshRenderer>();
            myCollider = GetComponent<Collider>();

            lastPosition = transform.position;

        }

        protected override void Start()
        {
            base.Start();

            if (dZPlayer == null)
            {
                dZPlayer = GetComponentInHierarchy<DropZonePlayer>();
            }
            if (IS_NOT_NULL(dZPlayer))
            {
                playerID = dZPlayer.PlayerID;
                owner = dZPlayer.GetOwner();
            }

            // Disable collision if not locally owned
            if (IS_NOT_NULL(myCollider)
                && owner != BEventManager.Instance.LocalNetworkID)
            {
                myCollider.enabled = false;
            }
        }

        protected override void Update()
        {
            base.Update();

            UpdateVelocity();

            LogCanvas("BallDroneVelocity", "Dot : " + Vector3.Dot(currentVelocity.normalized, transform.forward));

            // Release ball ?
            if (IsBallDroneCaught == true
                && owner == BEventManager.Instance.LocalNetworkID
                && currentVelocity.magnitude > releaseVelocityThreshold
                && Vector3.Dot(currentVelocity.normalized, transform.forward) > forwardDotThreshold)
            {
                ReleaseBall(currentVelocity);
            }
        }
        #endregion

        #region Events Callbacks
        private void OnTriggerEnter(Collider other)
        {
            // Should only be called on locally owned objects
            if (dZPlayer
                && owner == BEventManager.Instance.LocalNetworkID)
            {
                BallDroneBAnchor ballDrone = BUtils.GetComponentInHierarchy<BallDroneBAnchor>(other.gameObject);
                if (ballDrone)
                {
                    CatchBallDrone(ballDrone);
                }
            }
        }

        private void On_AR_PlayAreaStateUpdated(BEHandle<EPlayAreaState, AbstractPlayArea> handle)
        {
            if (handle.Arg1 == EPlayAreaState.READY)
            {
                RectanglePlayArea rectanglePlayArea = (RectanglePlayArea)handle.Arg2;
                if (rectanglePlayArea)
                {
                    playAreaUpVector = rectanglePlayArea.UpVector;
                }
            }
        }

        private void On_DZ_BallDroneCaught(BEHandle<EPlayerID> handle)
        {
            // TODO : Feels like a hack
            BallDroneBAnchor ballDrone = FindObjectOfType<BallDroneBAnchor>();

            // On not locally owned instances
            if (handle.Arg1 == playerID
                && ARE_NOT_EQUAL(handle.InvokingNetworkID, BEventManager.Instance.LocalNetworkID)
                && IS_NOT_NULL(ballDrone))
            {
                caughtBallDrone = ballDrone;
                caughtBallDrone.Owner = handle.InvokingNetworkID;

                IS_TRUE(caughtBallDrone.OnCatch(playerID));

                if (myRenderer)
                {
                    myRenderer.material.color = Color.red;
                }
            }
        }


        private void On_DZ_BallDroneReleased(BEHandle<EPlayerID> handle)
        {
            // On not locally owned instances
            if (handle.Arg1 == playerID
                && ARE_NOT_EQUAL(handle.InvokingNetworkID, BEventManager.Instance.LocalNetworkID)
                && IS_NOT_NULL(caughtBallDrone))
            {
                caughtBallDrone.Release(transform.position, Vector3.zero, 0.0f);

                caughtBallDrone = null;

                if (myRenderer)
                {
                    myRenderer.material.color = Color.white;
                }
            }
        }



        #endregion

        #region Others
        private void CatchBallDrone(BallDroneBAnchor ballDrone)
        {
            if (IS_NOT_NULL(ballDrone)
                && ballDrone.OnCatch(playerID))
            {
                caughtBallDrone = ballDrone;
                caughtBallDrone.Owner = owner;

                if (myRenderer)
                {
                    myRenderer.material.color = Color.red;
                }

                BEventsCollection.DZ_BallDroneCaught.Invoke(new BEHandle<EPlayerID>(playerID), BEventReplicationType.TO_ALL_OTHERS);
            }
        }

        private void ReleaseBall(Vector3 velocity)
        {
            if (IS_NOT_NULL(caughtBallDrone)
                && ARE_EQUAL(owner, BEventManager.Instance.LocalNetworkID))
            {
                StartCoroutine(DisableColliderCoroutine());

                caughtBallDrone.Release(transform.position, velocity.normalized, releaseForceIntensity);

                caughtBallDrone = null;

                if (myRenderer)
                {
                    myRenderer.material.color = Color.white;
                }

                BEventsCollection.DZ_BallDroneReleased.Invoke(new BEHandle<EPlayerID>(playerID), BEventReplicationType.TO_ALL_OTHERS);
            }
        }
           
        private IEnumerator DisableColliderCoroutine()
        {
            if (IS_NOT_NULL(myCollider))
            {
                myCollider.enabled = false;

                yield return new WaitForSeconds(disableColliderDuration);

                myCollider.enabled = true;
            }
        }

        private void UpdateVelocity()
        {
            currentVelocity = transform.position - lastPosition;
            lastPosition = transform.position;

            if (currentVelocity.magnitude > maxVelocity)
            {
                maxVelocity = currentVelocity.magnitude;
            }
        }

        #endregion
    }
}
