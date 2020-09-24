using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class BallDroneUIManager : BBehaviour
    {
        #region Public Events


        #endregion

        #region Public Methods


        #endregion

        #region Inspector Variables
        [SerializeField]
        private BImage ballDroneCatchIndicator;


        #endregion

        #region Private Variables
        ARObjectSpawner aRObjectSpawner;

        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            aRObjectSpawner = GetComponentInChildren<ARObjectSpawner>();

            BEventsCollection.DZ_BallDroneCaught += On_DZ_BallDroneCaught;
            BEventsCollection.DZ_BallDroneReleased += On_DZ_BallDroneReleased;
        }

        protected override void LateStart()
        {
            base.LateStart();

            if (IS_NOT_NULL(ballDroneCatchIndicator))
            {
                ballDroneCatchIndicator.HideUIElement();
            }
            
        }

        #endregion

        #region Events Callbacks
        private void On_DZ_BallDroneCaught(BEHandle<EPlayerID> handle)
        {
            if (BEventManager.Instance.LocalNetworkID == handle.InvokingNetworkID
                && IS_NOT_NULL(ballDroneCatchIndicator))
            {
                ballDroneCatchIndicator.ShowUIElement();
            }
        }

        private void On_DZ_BallDroneReleased(BEHandle<EPlayerID> handle)
        {
            if (IS_NOT_NULL(ballDroneCatchIndicator))
            {
                ballDroneCatchIndicator.HideUIElement();
            }
        }


        #endregion

        #region Others


        #endregion
    }
}
