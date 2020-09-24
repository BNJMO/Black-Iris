using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public enum EARGameHostState
    {
        NONE = -1,
        READY_TO_SET_WORLD_ROOT = 0,
        READY_TO_SET_PLAY_AREA = 1,
        READY_TO_START_PARTY = 2,
        WAITING_FOR_PLAYERS = 3,
        GAME_STARTED = 4, 

    }

    //public enum EARGameClientState
    //{
    //    WORLD_ROOT_NOT_SET = 0,

    //}



    public class ARGameHostStateManager : AbstractSingletonManager<ARGameHostStateManager>
    {
        #region Public Events


        #endregion

        #region Public Methods
        public EARGameHostState CurrentARGameHostState { get; private set; } = EARGameHostState.NONE;

        /* Shared */
        public void SetWorldRoot()
        {
     
        }


        /* Server */
        public void StartParty()
        {

        }

        public void SetupPlayArea()
        {
        
        }

        public void SpawnBallDrone()
        {
            
        }

        public void ShareParty()
        {

        }

        public void StartGame()
        {

        }

        /* Client */
        //public void JoinParty()
        //{

        //}

        //public void RequestPlayArea()
        //{

        //}

        //public void OnGameStarted()
        //{

        //}
        #endregion

        #region Serialized Fields


        #endregion

        #region Private Variables

        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();


        }

        protected override void LateStart()
        {
            base.LateStart();

            UpdateState(EARGameHostState.READY_TO_SET_WORLD_ROOT);
        }

        protected override void Update()
        {
            base.Update();

            DebugManager.Instance.DebugLogCanvas("ARGameState", CurrentARGameHostState.ToString());
        }

        #endregion

        #region Events Callbacks



        #endregion

        #region Others
        private void UpdateState(EARGameHostState newState)
        {
            if (ARE_NOT_EQUAL(newState, CurrentARGameHostState))
            {
                BEventsCollection.AR_ARGameStateUpdated.Invoke(new StateBEHandle<EARGameHostState>(newState, CurrentARGameHostState));
                CurrentARGameHostState = newState;
            }
        }

        #endregion

    }
}
