using System;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class BEventsCollection : AbstractSingletonManager<BEventsCollection>
    {
        public Dictionary<string, AbstractBEvent> AllReplicatedBEvent = new Dictionary<string, AbstractBEvent>()
        {
            /* Test */ 
            { "TEST_CounterIncrement", TEST_CounterIncrement },
            { "TEST_FloatTest", TEST_FloatTest },
            { "TEST_Vector3Test", TEST_Vector3Test },
            { "TEST_ObjectTest", TEST_ObjectTest },
            { "TEST_ImagePosition", TEST_ImagePosition },
            { "TEST_TrackerMoved", TEST_TrackerMoved },

            /* AR */
            { "AR_TrackerFound", AR_TrackerFound },
            { "AR_TrackerUpdated", AR_TrackerUpdated },
            { "AR_TrackerLost", AR_TrackerLost },
            { "AR_PlayAreadStateUpdated", AR_PlayAreaStateUpdated },
            { "AR_NewPlayAreaSet", AR_NewPlayAreaSet },
            { "AR_BAnchorReplicateTransform", AR_BAnchorReplicateTransform },
            { "AR_BAnchorSpawned", AR_BAnchorSpawned },

            /* Network */
            { "NETWORK_PlayerJoined", NETWORK_PlayerJoined },
            { "NETWORK_PlayerLeft", NETWORK_PlayerLeft },
            { "NETWORK_CalculateRTT", NETWORK_CalculateRTT },
            { "NETWORK_RequestPing", NETWORK_RequestPing },
            { "NETWORK_SharePing", NETWORK_SharePing },

            /* Drop Zone */
            { "DZ_DroneBallSpawned", DZ_DroneBallSpawned },
            { "DZ_BallDroneCaught", DZ_BallDroneCaught },
            { "DZ_BallDroneReleased", DZ_BallDroneReleased },

            /* Black Iris */
            { "BI_PlayVideo", BI_PlayVideo },
            { "BI_SynchFrame", BI_SynchFrame },



        };

        #region Test
        public static BEvent<BEHandle<int>> TEST_CounterIncrement
                = new BEvent<BEHandle<int>>("TEST_CounterIncrement");

        public static BEvent<BEHandle<float>> TEST_FloatTest
                = new BEvent<BEHandle<float>>("TEST_FloatTest");

        public static BEvent<BEHandle<Vector3>> TEST_Vector3Test
                = new BEvent<BEHandle<Vector3>>("TEST_Vector3Test");

        public static BEvent<BEHandle<BallDroneBAnchor>> TEST_ObjectTest
                = new BEvent<BEHandle<BallDroneBAnchor>>("TEST_ObjectTest");

        public static BEvent<BEHandle<Vector3>> TEST_ImagePosition
                = new BEvent<BEHandle<Vector3>>("TEST_ImagePosition");

        public static BEvent<BEHandle<Vector3>> TEST_TrackerMoved
                = new BEvent<BEHandle<Vector3>>("TEST_TrackerMoved");


        #endregion

        #region AR
        /// <summary>
        /// Arg1 : Tracker Name
        /// Arg2 : Position of the tracked object
        /// Arg3 : Rotation of the tracked object
        /// </summary>
        public static BEvent<BEHandle<string, Vector3, Quaternion>> AR_TrackerFound
         = new BEvent<BEHandle<string, Vector3, Quaternion>>("AR_TrackerFound");

        /// <summary>
        /// Arg1 : Tracker Name
        /// Arg2 : Position of the tracked object
        /// Arg3 : Rotation of the tracked object
        /// </summary>
        public static BEvent<BEHandle<string, Vector3, Quaternion>> AR_TrackerUpdated
         = new BEvent<BEHandle<string, Vector3, Quaternion>>("AR_TrackerFound");

        /// <summary>
        /// Arg1 : Tracker Name
        /// </summary>
        public static BEvent<BEHandle<string>> AR_TrackerLost
         = new BEvent<BEHandle<string>>("AR_TrackerLost");

        //public static BEvent<>

        public static BEvent<BEHandle<EPlayAreaState, AbstractPlayArea>> AR_PlayAreaStateUpdated
         = new BEvent<BEHandle<EPlayAreaState, AbstractPlayArea>>("AR_PlayAreadStateUpdated");

        public static BEvent<BEHandle<BAnchorInformation[]>> AR_NewPlayAreaSet
         = new BEvent<BEHandle<BAnchorInformation[]>> ("AR_NewPlayAreaSet");

        public static BEvent<StateBEHandle<EARGameHostState>> AR_ARGameStateUpdated
         = new BEvent<StateBEHandle<EARGameHostState>>("AR_ARGameStateUpdated");

        public static BEvent<BEHandle<BAnchorInformation>> AR_WorldBAnchorSet
         = new BEvent<BEHandle<BAnchorInformation>>("AR_WorldBAnchorSet");

        public static BEvent<BEHandle<BAnchorInformation, string>> AR_BAnchorSpawned
         = new BEvent<BEHandle<BAnchorInformation, string>>("AR_BAnchorSpawned");

        /// <summary>
        /// BAnchorInformation : transformed position and rotation of the BAnchor
        /// string : BAnchorID of the replicated BAnchor
        /// </summary>
        public static BEvent<BEHandle<BAnchorInformation, string>> AR_BAnchorReplicateTransform
                = new BEvent<BEHandle<BAnchorInformation, string>>("AR_BAnchorReplicateTransform");




        #endregion

        #region App State
        public static BEvent<StateBEHandle<EAppState>> APP_AppStateUpdated
            = new BEvent<StateBEHandle<EAppState>>("APP_AppStateUpdated");

        public static BEvent<StateBEHandle<EAppScene>> APP_SceneWillChange
            = new BEvent<StateBEHandle<EAppScene>>("APP_SceneWillChange");

        public static BEvent<StateBEHandle<EAppScene>> APP_AppSceneUpdated
            = new BEvent<StateBEHandle<EAppScene>>("APP_AppSceneUpdated");

        #endregion
     
        #region Game
        public event Action<AbstractGameMode> GAME_GameStarted;
        public void Invoke_GAME_GameStarted(AbstractGameMode gameMode)
        {
            if (GAME_GameStarted != null)
            {
                GAME_GameStarted.Invoke(gameMode);
            }
            DebugLog("Game started : " + gameMode.GameModeType);
        }


        public event Action<AbstractGameMode> GAME_GamePaused;
        public void Invoke_GAME_GamePaused(AbstractGameMode gameMode)
        {
            if (GAME_GamePaused != null)
            {
                GAME_GamePaused.Invoke(gameMode);
            }
            DebugLog("Game paused : " + gameMode.GameModeType);
        }

        public event Action<AbstractGameMode> GAME_GameUnPaused;
        public void Invoke_GAME_GameUnPaused(AbstractGameMode gameMode)
        {
            if (GAME_GameUnPaused != null)
            {
                GAME_GameUnPaused.Invoke(gameMode);
            }
            DebugLog("Game unpaused : " + gameMode.GameModeType);
        }

        public event Action<AbstractGameMode, bool> GAME_GameEnded;
        public void Invoke_GAME_GameEnded(AbstractGameMode gameMode, bool wasAborted = false)
        {
            if (GAME_GameEnded != null)
            {
                GAME_GameEnded.Invoke(gameMode, wasAborted);
            }
            DebugLog("Game ended : " + gameMode.GameModeType + ". Aborted : " + wasAborted.ToString());
        }

        public event Action<AbstractPlayerStats, EGameMode> GAME_PlayerStatsUpdated;
        public void Invoke_GAME_PlayerStatsUpdated(AbstractPlayerStats updatedPlayerStats, EGameMode fromGameMode)
        {
            if (GAME_PlayerStatsUpdated != null)
            {
                GAME_PlayerStatsUpdated.Invoke(updatedPlayerStats, fromGameMode);
            }
            // DebugLog("Player stats " + updatedPlayerStats.ToString() + " updated for " + fromGameMode);
        }

        public event Action<AbstractGameMode> GAME_SetPlayersScores;
        public void Invoke_GAME_SetPlayersScores(AbstractGameMode gameMode)
        {
            if (GAME_SetPlayersScores != null)
            {
                GAME_SetPlayersScores.Invoke(gameMode);
            }
            DebugLog("Set Players Scores");
        }

        public event Action<int> GAME_GameTimeUpdated;
        public void Invoke_GAME_GameTimeUpdated(int time)
        {
            if (GAME_GameTimeUpdated != null)
            {
                GAME_GameTimeUpdated.Invoke(time);
            }
            // DebugLog("Time has been Updated to : " + time);
        }

        #endregion

        #region Players
        /* Lobby Join */
        public static BEvent<BEHandle<EPlayerID, EControllerID>> PLAYERS_PlayerJoined
                = new BEvent<BEHandle<EPlayerID, EControllerID>>("PLAYERS_PlayerJoined");

        public static BEvent<BEHandle<EPlayerID, EControllerID>> PLAYERS_PlayerLeft
                = new BEvent<BEHandle<EPlayerID, EControllerID>>("PLAYERS_PlayerLeft");

        public static BEvent<BEHandle> PLAYERS_AllPlayersReady
                = new  BEvent<BEHandle>("PLAYERS_AllPlayersReady");

        public static BEvent<BEHandle<EPlayerID>> PLAYERS_PlayerReady
                = new BEvent<BEHandle<EPlayerID>>("PLAYERS_PlayerReady");

        public static BEvent<BEHandle<EPlayerID>> PLAYERS_PlayerCanceledReady
                = new BEvent<BEHandle<EPlayerID>>("PLAYERS_PlayerCanceledReady");

        /* In Game */
        public static BEvent<BEHandle<EPlayerID, IPlayer>> PLAYERS_PlayerSpawned 
                = new BEvent<BEHandle<EPlayerID, IPlayer>>("PLAYERS_PlayerSpawned");

        public static BEvent<BEHandle<EPlayerID, IPlayer>> PLAYERS_PlayerDied
                = new BEvent<BEHandle<EPlayerID, IPlayer>>("PLAYERS_PlayerDied");
        #endregion

        #region UI
        //public BEvent<StateBEHandle<EUIState>> UI_UIStateUpdated 
        // = new BEvent<StateBEHandle<EUIState>>("UI_UIStateUpdated");

        public static BEvent<BEHandle<BFrame>> UI_FocusedFrameUpdated
         = new BEvent<BEHandle<BFrame>>("UI_FocusedFrameUpdated");

        public static BEvent<BEHandle<BMenu>> UI_HighlightedBMenuUpdated
         = new BEvent<BEHandle<BMenu>>("UI_HighlightedBMenuUpdated");

        /* Buttons */
        public static BEvent<BEHandle<BButton>> UI_ButtonHighlighted
         = new BEvent<BEHandle<BButton>>("UI_ButtonHighlighted");
                
        public static BEvent<BEHandle<BButton>> UI_ButtonPressed
         = new BEvent<BEHandle<BButton>>("UI_ButtonPressed");

        public static BEvent<BEHandle<BButton>> UI_ButtonUnhighlighted
         = new BEvent<BEHandle<BButton>>("UI_ButtonUnhighlighted");

        public static BEvent<BEHandle<BButton, bool>> UI_ButtonReleased
         = new BEvent<BEHandle<BButton, bool>>("UI_ButtonReleased");


        #endregion

        #region Input
        public static BEvent<BEHandle<EControllerID>> INPUT_ControllerConnected
            = new BEvent<BEHandle<EControllerID>>("INPUT_ControllerConnected");

        public static BEvent<BEHandle<EControllerID>> INPUT_ControllerDisconnected
            = new BEvent<BEHandle<EControllerID>>("INPUT_ControllerDisconnected");

        public static BEvent<BEHandle<EControllerID, EInputButton>> INPUT_ButtonPressed
            = new BEvent<BEHandle<EControllerID, EInputButton>>("INPUT_ButtonPressed");

        public static BEvent<BEHandle<EControllerID, EInputButton>> INPUT_ButtonReleased
            = new BEvent<BEHandle<EControllerID, EInputButton>>("INPUT_ButtonReleased");

        public static BEvent<BEHandle<EControllerID, EInputAxis, float, float>> INPUT_AxisUpdated
            = new BEvent<BEHandle<EControllerID, EInputAxis, float, float>>("INPUT_AxisUpdated");


        #endregion

        #region Network
        public static BEvent<BEHandle<AbstractBEventDispatcher>> NETWORK_NewBEventDispatcherSet
        = new BEvent<BEHandle<AbstractBEventDispatcher>>("NETWORK_NewBEventDispatcherSet");

        public static BEvent<BEHandle<ENetworkState>> NETWORK_NetworkStateUpdated
                = new BEvent<BEHandle<ENetworkState>>("NETWORK_NetworkStateUpdated");

        public static BEvent<BEHandle> NETWORK_StartedHost 
                = new BEvent<BEHandle>("NETWORK_StartedHost");

        public static BEvent<BEHandle<ENetworkID>> NETWORK_ConnectedToHost
                = new BEvent<BEHandle<ENetworkID>>("NETWORK_ConnectedToHost");

        public static BEvent<BEHandle> NETWORK_ConnectionStoped
                = new BEvent<BEHandle>("NETWORK_ConnectionStoped");

        public static BEvent<BEHandle<ENetworkID>> NETWORK_NewNetworkIDConnected 
                = new BEvent<BEHandle<ENetworkID>>("NETWORK_NewNetworkIDConnected");

        public static BEvent<BEHandle<ENetworkID>> NETWORK_NetworkIDDisconnected
                = new BEvent<BEHandle<ENetworkID>>("NETWORK_NetworkIDDisconnected");

        public static BEvent<BEHandle<string[]>> NETWORK_DiscoveredHostsUpdated
                = new BEvent<BEHandle<string[]>>("NETWORK_DiscoveredHostsUpdated");

        public static BEvent<BEHandle<EPlayerID, EControllerID>> NETWORK_PlayerJoined
                = new BEvent<BEHandle<EPlayerID, EControllerID>>("NETWORK_PlayerJoined");

        public static BEvent<BEHandle<EPlayerID, EControllerID>> NETWORK_PlayerLeft
                = new BEvent<BEHandle<EPlayerID, EControllerID>>("NETWORK_PlayerLeft");

        public static BEvent<BEHandle<ENetworkID, int>> NETWORK_CalculateRTT
                = new BEvent<BEHandle<ENetworkID, int>>("NETWORK_CalculateRTT");

        public static BEvent<BEHandle> NETWORK_RequestPing
                = new BEvent<BEHandle>("NETWORK_RequestPing");

        public static BEvent<BEHandle<int>> NETWORK_SharePing
                = new BEvent<BEHandle<int>>("NETWORK_SharePing");

        #endregion

        #region DropZone
        public static BEvent<BEHandle<BAnchorInformation>> DZ_DroneBallSpawned
                = new BEvent<BEHandle<BAnchorInformation>>("DZ_DroneBallSpawned");

        public static BEvent<BEHandle<EPlayerID>> DZ_BallDroneCaught
                = new BEvent<BEHandle<EPlayerID>>("DZ_BallDroneCaught");

        public static BEvent<BEHandle<EPlayerID>> DZ_BallDroneReleased
                = new BEvent<BEHandle<EPlayerID>>("DZ_BallDroneReleased");


        #endregion

        #region Black Iris
        public static BEvent<BEHandle> BI_PlayVideo
                = new BEvent<BEHandle>("BI_PlayVideo");

        public static BEvent<BEHandle<int>> BI_SynchFrame
                = new BEvent<BEHandle<int>>("BI_SynchFrame");


        #endregion

        // TODO: Depricated
        private void DebugLog(string messageLog)
        {
            if (MotherOfManagers.Instance.IsDebugLogEvents == true)
            {
                Debug.Log("<color=green>[EVENT (old)]</color> " + messageLog);
            }
        }
    }
}
