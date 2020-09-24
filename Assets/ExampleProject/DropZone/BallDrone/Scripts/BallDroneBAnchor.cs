using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public enum EBallDroneState
    {
        FREE,
        CAUGHT,
    }

    [RequireComponent(typeof(Rigidbody))]
    public class BallDroneBAnchor : BAnchor
    {
        #region Public Events
        public event Action<EBallDroneState> BallDroneStateUpdated;

        #endregion

        #region Public Methods
        public bool OnCatch(EPlayerID playerID)
        {
            if (ARE_EQUAL(currentState, EBallDroneState.FREE))
            {
                lastCatchingPlayerID = playerID;

                UpdateState(EBallDroneState.CAUGHT);

                gameObject.SetActive(false);

                return true;
            }
            return false;
        }

        public void Release(Vector3 position, Vector3 direction, float intensity)
        {
            if (ARE_EQUAL(currentState, EBallDroneState.CAUGHT))
            {
                transform.position = position;
                gameObject.SetActive(true);

                UpdateState(EBallDroneState.FREE);

                if (intensity > 0.0f)
                {
                    myRigidBody.AddForce(direction * intensity);
                    myRigidBody.AddTorque(direction * intensity);
                }
            }
        }

        #endregion

        #region Inspector Variables
        [SerializeField]
        private float gravityIntensity = -9.81f;
                
        [SerializeField]
        private float forceIntensity = 10.0f;

        [SerializeField]
        private float spawnHeightFromCursor = 0.5f;

        [Button("Random Force")] 
        private void Button_RandomForce() { ApplyRandomFroce(); }


        #endregion

        #region Private Variables
        private Rigidbody myRigidBody;
        private EBallDroneState currentState = EBallDroneState.FREE;
        private EPlayerID lastCatchingPlayerID = EPlayerID.NONE;

        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            myRigidBody = GetComponent<Rigidbody>();

            transform.position += Vector3.up * spawnHeightFromCursor;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (Owner == BEventManager.Instance.LocalNetworkID)
            {
                myRigidBody.AddForce(Vector3.up * gravityIntensity, ForceMode.Force);
            }
        }

        #endregion

        #region Events Callbacks


        #endregion

        #region Others
        private void ApplyRandomFroce()
        {
            gameObject.SetActive(true);

            Vector3 randomForce = BUtils.GetRandomVector(forceIntensity, forceIntensity);
            myRigidBody.AddForce(randomForce, ForceMode.Impulse);

            Vector3 randomRotation = BUtils.GetRandomVector(0.0f, 360.0f);
            myRigidBody.AddTorque(randomRotation, ForceMode.Impulse);

        }

        private void UpdateState(EBallDroneState newState)
        {
            currentState = newState;
            InvokeEventIfBound(BallDroneStateUpdated, newState);
        }

        #endregion
    }
}
