using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class DropZoneManager : AbstractSingletonManager<DropZoneManager>
    {
        #region Public Events


        #endregion

        #region Public Methods
        public void SpawnDroneBall()
        {
            // Destroy exsiting Ball Drone
            BallDroneBAnchor existingBallDrone = FindObjectOfType<BallDroneBAnchor>();
            if (existingBallDrone)
            {
                Destroy(existingBallDrone.gameObject);
            }

            if (IS_NOT_NULL(ballDronePrefab))
            {
                ballDrone = (BallDroneBAnchor) ARManager.Instance.SpawnBAnchorAtCursorPosition(ballDronePrefab);
            }

            if (ballDrone != null)
            {
                BEventsCollection.DZ_DroneBallSpawned.Invoke(new BEHandle<BAnchorInformation>(ballDrone.GetBAnchorInformation()), BEventReplicationType.TO_ALL);
            }
            else
            {
                LogConsoleWarning("Spawning Ball Drone didn't succeed!");
            }
        }

        #endregion

        #region Inspector Variables

        #endregion

        #region Private Variables
        private BallDroneBAnchor ballDrone;
        private BallDroneBAnchor ballDronePrefab;

        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            BEventsCollection.DZ_DroneBallSpawned.Event += On_DZ_DroneBallSpawned;

            ballDronePrefab = Resources.Load<BallDroneBAnchor>(BConsts.PATH_DZ_DroneBall);
        }

        private void On_DZ_DroneBallSpawned(BEHandle<BAnchorInformation> handle)
        {
            if (BEventManager.Instance.LocalNetworkID != handle.InvokingNetworkID
                && IS_NOT_NULL(ballDronePrefab))
            {
                // Destroy exsiting Ball Drone
                BallDroneBAnchor existingBallDrone = FindObjectOfType<BallDroneBAnchor>();
                if (existingBallDrone)
                {
                    Destroy(existingBallDrone.gameObject);
                }

                // Spawn new
                BAnchorInformation bAnchorInformation = handle.Arg1;
                BallDroneBAnchor ballDrone = Instantiate(ballDronePrefab);
                ballDrone.Owner = handle.InvokingNetworkID;
                ballDrone.SetTransformedPosition(bAnchorInformation.TransformedPosition);
                ballDrone.SetTransformedRotation(bAnchorInformation.TransformedRotation);


            }
        }

        #endregion

        #region Events Callbacks


        #endregion

        #region Others


        #endregion
    }
}
