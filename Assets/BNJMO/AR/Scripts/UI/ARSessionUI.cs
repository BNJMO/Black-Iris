using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class ARSessionUI : BBehaviour
    {
        #region Public Events


        #endregion

        #region Public Methods
        public void OnPlaceRootPressed()
        {
            ARManager.Instance.SpawnWorldRootBAnchor();
        }

        public void OnSetUpPlayAreaPressed()
        {
            ARManager.Instance.SetupPlayArea();
        }

        public void SpawnBallDrone()
        {
            if (DropZoneManager.IsInstanceSet)
            {
                DropZoneManager.Instance.SpawnDroneBall();
            }
        }

        public void SpawnTestBAnchor()
        {
            BAnchor testBAnchorPrefab = Resources.Load<BAnchor>(BConsts.PATH_AR_TestBAnchor);
            if (IS_NOT_NULL(testBAnchorPrefab))
            {
                ARManager.Instance.SpawnBAnchorAtCursorPosition(testBAnchorPrefab, true);
            }
        }

        #endregion

        #region Serialized Fields


        #endregion

        #region Private Variables

        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            BEventsCollection.AR_ARGameStateUpdated += On_AR_ARGameStateUpdated;
        }

        private void On_AR_ARGameStateUpdated(StateBEHandle<EARGameHostState> stateBEHandle)
        {
            switch (stateBEHandle.NewState)
            {
                case EARGameHostState.READY_TO_SET_WORLD_ROOT:

                    break;
            }    

        }

        #endregion

        #region Events Callbacks


        #endregion

        #region Others


        #endregion
    }
}
