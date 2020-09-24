using System;
using System.Collections.Generic;
using Mirror.Examples.Basic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BNJMO
{
    [Serializable]
    public struct PlayerPrefab
    {
        public EPlayerID PlayerID;
        public AbstractPlayer Prefab;
    }

    public class PlayerManager : AbstractSingletonManager<PlayerManager>
    {
        #region Public Events


        #endregion

        #region Public Methods
        // TODO: Make these private and add with public getters
        /// <summary> Positions in the scene (or around PlayerManager if not found) where the players will be spawned. </summary>
        public Dictionary<EPlayerID, AbstractPlayerSpawnPosition> PlayersSpawnPositions { get; } = new Dictionary<EPlayerID, AbstractPlayerSpawnPosition>();

        /// <summary> Added whenever a player has spawned. Removed when he dies. </summary>
        public Dictionary<EPlayerID, AbstractPlayer> ActivePlayers { get; } = new Dictionary<EPlayerID, AbstractPlayer>();

        /// <summary> All assigned players for every team. </summary>
        public Dictionary<ETeamID, List<EPlayerID>> Teams { get; } = new Dictionary<ETeamID, List<EPlayerID>>();

        /// <summary> Assigned team of every player </summary>
        public Dictionary<EPlayerID, ETeamID> PlayersTeam { get; } = new Dictionary<EPlayerID, ETeamID>();

        public List<EPlayerID> GetJoinedPlayers()
        {
            List<EPlayerID> result = new List<EPlayerID>();
            foreach (KeyValuePair<EPlayerID, PlayerJoinStatus> pair in partyStatusMap)
            {
                PlayerJoinStatus playerJoinStatus = pair.Value;
                if (playerJoinStatus.HasJoined == true)
                {
                    result.Add(pair.Key);
                }
            }
            return result;
        }

        public bool HasPlayerJoined(EPlayerID playerID)
        {
            foreach (KeyValuePair<EPlayerID, PlayerJoinStatus> pair in partyStatusMap)
            {
                PlayerJoinStatus playerJoinStatus = pair.Value;
                if ((playerID == pair.Key)
                    && (playerJoinStatus.HasJoined == true))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsPlayerAlive(EPlayerID playerID)
        {
            return ActivePlayers.ContainsKey(playerID);
        }

        public EPlayerID[] GetPlayersInTeam(ETeamID inTeamID)
        {
            return Teams[inTeamID].ToArray();
        }

        public EPlayerID GetAssignedPlayerID(EControllerID controllerID)
        {
            if (IS_KEY_CONTAINED(ControllersMap, controllerID))
            {
                return ControllersMap[controllerID];
            }
            return EPlayerID.NONE;
        }

        public EControllerID GetAssignedControllerID(EPlayerID playerID)
        {
            if (IS_KEY_CONTAINED(partyStatusMap, playerID))
            {
                return partyStatusMap[playerID].ControllerID;
            }
            return EControllerID.NONE;
        }

        /* Player Management */
        public AbstractPlayer SpawnPlayer(EPlayerID playerID)
        {
            AbstractPlayer spawnedPlayer = null;

            if (IS_TRUE(HasPlayerJoined(playerID))
                && IS_KEY_NOT_CONTAINED(ActivePlayers, playerID)
                && IS_KEY_CONTAINED(playersPrefabsMap, playerID))
            {
                AbstractPlayer playerPrefab = playersPrefabsMap[playerID];

                Vector3 playerPosition = Vector3.zero;
                Quaternion playerRotation = Quaternion.identity;
                AbstractPlayerSpawnPosition playerSpawnPosition = null;
                if (PlayersSpawnPositions.ContainsKey(playerID))
                {
                    playerSpawnPosition = PlayersSpawnPositions[playerID];
                    playerPosition = playerSpawnPosition.Position;
                    playerRotation = playerSpawnPosition.Rotation;
                }
                else
                {
                    LogConsoleWarning("No spawn position defined for : " + playerID + ". Spawning at root");
                }

                spawnedPlayer = Instantiate(playerPrefab, playerPosition, playerRotation);
                spawnedPlayer.PlayerID = playerID;
                spawnedPlayer.TeamID = PlayersTeam[playerID];

                ActivePlayers.Add(playerID, spawnedPlayer);

                // Place player under parent of SpawnPosition
                if (playerSpawnPosition
                    && MotherOfManagers.Instance.SpawnPlayersUnderSameTransformAsSpawnPositions == true
                    && playerSpawnPosition.transform.parent != null)
                {
                    spawnedPlayer.transform.parent = playerSpawnPosition.transform.parent;
                }

                BEventsCollection.PLAYERS_PlayerSpawned.Invoke(new BEHandle<EPlayerID, IPlayer>(playerID, spawnedPlayer));
            }
            return spawnedPlayer;
        }

        public void SpawnAllJoinedPlayers()
        {
            LogConsole("Spawning remaining AI players");
            // Fill empty slots with AI
            int emptySlots = 4 - GetJoinedPlayers().Count;
            int remainingAI = MotherOfManagers.Instance.MaximumNumberOfAIToSpawn;
            while ((emptySlots > 0) && (remainingAI > 0))
            {
                EControllerID aIControllerID = BUtils.GetAIControllerIDFrom(MotherOfManagers.Instance.MaximumNumberOfAIToSpawn - remainingAI + 1);
                JoinNextAIPlayer(aIControllerID);
                emptySlots--;
                remainingAI--;
            }

            foreach (EPlayerID playerID in GetJoinedPlayers())
            {
                if (IS_KEY_NOT_CONTAINED(ActivePlayers, playerID))
                {
                    SpawnPlayer(playerID);
                }
            }
        }

        //public void RespawnPlayer(EPlayerID playerID)
        //{
        //    if ((IS_KEY_NOT_CONTAINED(ActivePlayers, playerID))
        //        && (IS_KEY_CONTAINED(PlayersDeathPositions, playerID))
        //        && (IS_KEY_CONTAINED(PlayersDeathRotations, playerID))
        //        && (IS_KEY_CONTAINED(PlayersSpawnPositions, playerID)))
        //    {
        //        Vector3 startPostion = PlayersDeathPositions[playerID];
        //        Quaternion startRotation = PlayersDeathRotations[playerID];
        //        Vector3 endPosition = PlayersSpawnPositions[playerID].Position;
        //        Quaternion endRotation = PlayersSpawnPositions[playerID].Rotation;

        //        PlayerRespawnGhost playerRespawnGhost = Instantiate(playerRespawnGhostPrefab, startPostion, startRotation);
        //        playerRespawnGhost.PlayerID = playerID;
        //        playerRespawnGhost.RespawnAnimationDone += On_RespawnAnimationDone;
        //        playerRespawnGhost.StartRespawnAnimation(startPostion, startRotation, endPosition, endRotation);
        //    }
        //}


        //private void On_RespawnAnimationDone(PlayerRespawnGhost playerRespawnGhost)
        //{
        //    playerRespawnGhost.RespawnAnimationDone -= On_RespawnAnimationDone;

        //    LogConsole(playerRespawnGhost.PlayerID + " completed respawn animation");
        //    SpawnPlayer(playerRespawnGhost.PlayerID);
        //    playerRespawnGhost.DestroyGhost();
        //}

        public void DestroyPlayer(EPlayerID playerID)
        {
            if (IS_KEY_CONTAINED(ActivePlayers, playerID))
            {
                AbstractPlayer player = ActivePlayers[playerID];
                if ((IS_KEY_CONTAINED(PlayersDeathPositions, playerID))
                    && (IS_KEY_CONTAINED(PlayersDeathRotations, playerID)))
                {
                    PlayersDeathPositions[playerID] = player.Position;
                    PlayersDeathRotations[playerID] = player.Rotation;
                }

                ActivePlayers.Remove(playerID);
                BEventsCollection.PLAYERS_PlayerDied.Invoke(new BEHandle<EPlayerID, IPlayer>(playerID, player));
                player.DestroyPawn();
            }
        }

        public bool JoinPlayer(EPlayerID playerID, EControllerID controllerID)
        {
            if (IS_KEY_CONTAINED(partyStatusMap, playerID)
                && IS_KEY_CONTAINED(ControllersMap, controllerID)
                && IS_NOT_TRUE(partyStatusMap[playerID].HasJoined))
            {
                partyStatusMap[playerID].ControllerID = controllerID;
                partyStatusMap[playerID].HasJoined = true;

                // Update assigned PlayerID in the controller map (instead of Spectator)
                ControllersMap[controllerID] = playerID;

                BEventsCollection.PLAYERS_PlayerJoined.Invoke(new BEHandle<EPlayerID, EControllerID>(playerID, controllerID));

                return true;
            }
            return false;
        }

        public EPlayerID JoinPlayer(EControllerID controllerID)
        {
            EPlayerID playerID = GetNextNotJoinedPlayerID();

            if (IS_NOT_NONE(playerID)
                && IS_KEY_CONTAINED(partyStatusMap, playerID)
                && IS_KEY_CONTAINED(ControllersMap, controllerID)
                && IS_NOT_TRUE(partyStatusMap[playerID].HasJoined))
            {
                partyStatusMap[playerID].ControllerID = controllerID;
                partyStatusMap[playerID].HasJoined = true;

                // Update assigned PlayerID in the controller map (instead of Spectator)
                ControllersMap[controllerID] = playerID;

                BEventsCollection.PLAYERS_PlayerJoined.Invoke(new BEHandle<EPlayerID, EControllerID>(playerID, controllerID));
            }

            return playerID;
        }

        public void LeavePlayer(EPlayerID playerID)
        {
            if ((IS_KEY_CONTAINED(partyStatusMap, playerID))
            && (IS_TRUE(partyStatusMap[playerID].HasJoined)))
            {
                // Destroy Player
                if (ActivePlayers.ContainsKey(playerID))
                {
                    DestroyPlayer(playerID);
                }

                // Reset controller to Spectator
                EControllerID controllerID = partyStatusMap[playerID].ControllerID;
                if (IS_KEY_CONTAINED(ControllersMap, controllerID))
                {
                    ControllersMap[controllerID] = EPlayerID.SPECTATOR;
                }

                // Reset entry of playerID in party status 
                partyStatusMap[playerID] = new PlayerJoinStatus(EControllerID.NONE);

                // Trigger global event
                BEventsCollection.PLAYERS_PlayerLeft.Invoke(new BEHandle<EPlayerID, EControllerID>(playerID, controllerID));

                // Are the rest of the joined players ready?
                CheckIfAllPlayersAreReady();
            }
        }

        public void SetPlayerReady(EPlayerID playerID)
        {
            LogConsole(playerID + " ready");
            if ((IS_KEY_CONTAINED(partyStatusMap, playerID))
            && (IS_TRUE(partyStatusMap[playerID].HasJoined))
            && (IS_NOT_TRUE(partyStatusMap[playerID].IsReady)))
            {
                partyStatusMap[playerID].IsReady = true;

                BEventsCollection.PLAYERS_PlayerReady.Invoke(new BEHandle<EPlayerID>(playerID));

                CheckIfAllPlayersAreReady();
            }
        }

        public void CancelPlayerReady(EPlayerID playerID)
        {
            if ((IS_KEY_CONTAINED(partyStatusMap, playerID))
            && (IS_TRUE(partyStatusMap[playerID].HasJoined))
            && (IS_TRUE(partyStatusMap[playerID].IsReady)))
            {
                partyStatusMap[playerID].IsReady = false;

                BEventsCollection.PLAYERS_PlayerCanceledReady.Invoke(new BEHandle<EPlayerID>(playerID));
            }
        }
        
        // TODO
        public void ChangePlayerController(EPlayerID playerID, EControllerID controllerID)
        {
        }
        #endregion

        #region Inspector Variables

        [BoxGroup("Player Manager", centerLabel: true)]
        [BoxGroup("Player Manager")] 
        [SerializeField] 
        private AbstractPlayerSpawnPosition playerSpawnPositionPrefab;

        [BoxGroup("Player Manager")] 
        [SerializeField] 
        private PlayerPrefab[] playersPrefabs = new PlayerPrefab[0];

        #endregion

        #region Private Variables
        /// <summary> The join status of all the players in the current party </summary>
        private Dictionary<EPlayerID, PlayerJoinStatus> partyStatusMap { get; } = new Dictionary<EPlayerID, PlayerJoinStatus>();
        public Dictionary<EControllerID, EPlayerID> ControllersMap { get; } = new Dictionary<EControllerID, EPlayerID>();
        private Dictionary<EPlayerID, AbstractPlayer> playersPrefabsMap { get; } = new Dictionary<EPlayerID, AbstractPlayer>();

        // Still useful?
        private Dictionary<EPlayerID, Vector3> PlayersDeathPositions = new Dictionary<EPlayerID, Vector3>();
        private Dictionary<EPlayerID, Quaternion> PlayersDeathRotations = new Dictionary<EPlayerID, Quaternion>();
        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            LoadPlayerResources();
            ReinitializeDictionaries();
            ReinitializeControllersMap();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Input events
            BEventsCollection.INPUT_ControllerConnected.Event += On_INPUT_ControllerConnected;
            BEventsCollection.INPUT_ControllerDisconnected.Event += On_INPUT_ControllerDisconnected;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            // Input events
            BEventsCollection.INPUT_ControllerConnected.Event -= On_INPUT_ControllerConnected;
            BEventsCollection.INPUT_ControllerDisconnected.Event -= On_INPUT_ControllerDisconnected;
        }

        protected override void Update()
        {
            base.Update();

            UpdatePartyDebugText();
        }

        protected override void OnNewSceneReinitialize(EAppScene newScene, EAppScene lastScene)
        {
            base.OnNewSceneReinitialize(newScene, lastScene);

            FindPlayerSpawnPositionsInScene();

            ReinitializeDictionaries();
        }
        #endregion

        #region Events Callbacks


        #endregion


        #region Party Join
        private EPlayerID GetNextNotJoinedPlayerID()
        {
            foreach (var pair in partyStatusMap)
            {
                EPlayerID playerID = pair.Key;
                PlayerJoinStatus playerJoinStatus = pair.Value;

                if (playerJoinStatus.HasJoined == false)
                {
                    return playerID;
                }
            }
            return EPlayerID.NONE;
        }

        private EPlayerID JoinNextAIPlayer(EControllerID controllerID)
        {
            foreach (var pair in partyStatusMap)
            {
                EPlayerID playerID = pair.Key;
                PlayerJoinStatus playerJoinStatus = pair.Value;

                if (playerJoinStatus.HasJoined == false)
                {
                    JoinPlayer(playerID, controllerID);
                    return playerID;
                }
            }
            return EPlayerID.NONE;
        }

        private void CheckIfAllPlayersAreReady()
        {
            bool areAllReady = false;
            foreach (PlayerJoinStatus playerJoinStatus in partyStatusMap.Values)
            {
                if (playerJoinStatus.HasJoined == true)
                {
                    areAllReady = true;
                    if (playerJoinStatus.IsReady == false)
                    {
                        areAllReady = false;
                        break;
                    }
                }
            }
            if (areAllReady == true)
            {
                BEventsCollection.PLAYERS_AllPlayersReady.Invoke(new BEHandle());
            }
        }
        #endregion

        #region Events Callbacks
        private void On_INPUT_ControllerConnected(BEHandle<EControllerID> eventHandle)
        {
            EControllerID controllerID = eventHandle.Arg1;

            // Add connected controller as a spectator
            if (IS_KEY_NOT_CONTAINED(ControllersMap, controllerID))
            {
                ControllersMap.Add(controllerID, EPlayerID.SPECTATOR);

                BEventsCollection.PLAYERS_PlayerJoined.Invoke(new BEHandle<EPlayerID, EControllerID>(EPlayerID.SPECTATOR, controllerID));
            }
        }

        private void On_INPUT_ControllerDisconnected(BEHandle<EControllerID> eventHandle)
        {
            EControllerID controllerID = eventHandle.Arg1;

            if (IS_KEY_CONTAINED(ControllersMap, controllerID))
            {
                EPlayerID playerID = ControllersMap[controllerID];

                // Was a joined controller?
                if (playerID != EPlayerID.SPECTATOR
                    && IS_KEY_CONTAINED(partyStatusMap, playerID))
                {
                    partyStatusMap[playerID].Flush();
                }

                // Destroy Player
                if (ActivePlayers.ContainsKey(playerID))
                {
                    DestroyPlayer(playerID);
                }

                ControllersMap.Remove(controllerID);

                BEventsCollection.PLAYERS_PlayerLeft.Invoke(new BEHandle<EPlayerID, EControllerID>(playerID, controllerID));

            }
        }

        #endregion


        #region Initialization
        private void ReinitializeDictionaries()
        {
            ActivePlayers.Clear();

            Teams.Clear();
            Teams[ETeamID.TEAM_1] = new List<EPlayerID>();
            Teams[ETeamID.TEAM_2] = new List<EPlayerID>();
            Teams[ETeamID.TEAM_3] = new List<EPlayerID>();
            Teams[ETeamID.TEAM_4] = new List<EPlayerID>();

            partyStatusMap.Clear();
            PlayersTeam.Clear();
            PlayersDeathPositions.Clear();
            PlayersDeathRotations.Clear();

            // Adding all players (Besides Spectator and None)
            foreach (EPlayerID playerID in Enum.GetValues(typeof(EPlayerID)))
            {
                if ((playerID != EPlayerID.NONE)
                    && (playerID != EPlayerID.SPECTATOR))
                {
                    PlayersTeam.Add(playerID, ETeamID.NONE);
                    PlayersDeathPositions.Add(playerID, Vector3.zero);
                    PlayersDeathRotations.Add(playerID, Quaternion.identity);
                    partyStatusMap.Add(playerID, new PlayerJoinStatus(EControllerID.NONE));
                }
            }
        }

        private void LoadPlayerResources()
        {
            playersPrefabsMap.Clear();

            foreach (PlayerPrefab playerPrefab in playersPrefabs)
            {
                if ((IS_KEY_NOT_CONTAINED(playersPrefabsMap, playerPrefab.PlayerID))
                    && (IS_NOT_NULL(playerPrefab.Prefab)))
                {
                    playersPrefabsMap.Add(playerPrefab.PlayerID, playerPrefab.Prefab);
                }
            }

            IS_NOT_NULL(playerSpawnPositionPrefab);
        }

        private void FindPlayerSpawnPositionsInScene()
        {
            PlayersSpawnPositions.Clear();

            // Try to find already placed player spawn positions in the scene
            AbstractPlayerSpawnPosition[] spawnPositions = FindObjectsOfType<AbstractPlayerSpawnPosition>();
            foreach (AbstractPlayerSpawnPosition spawnPosition in spawnPositions)
            {
                PlayersSpawnPositions.Add(spawnPosition.PayerID, spawnPosition);
            }

            // Determine spawn positions relative to this transform if no PlayerSpawnPosition found in scene
            if (MotherOfManagers.Instance.IsSpawnGhostPlayerPositionsIfNotFound == true)
            {
                int angle;
                for (int i = 1; i < 5; i++)
                {
                    angle = 90 * i;
                    EPlayerID playerID = BUtils.GetPlayerIDFrom(i);
                    if (PlayersSpawnPositions.ContainsKey(playerID) == false)
                    {
                        AbstractPlayerSpawnPosition spawnGhost = Instantiate(playerSpawnPositionPrefab);
                        spawnGhost.PayerID = playerID;
                        spawnGhost.Position = transform.position + Vector3.forward * 3.0f + Vector3.left * 3.0f;
                        spawnGhost.transform.RotateAround(transform.position, Vector3.up, angle);
                        spawnGhost.Rotation = transform.rotation;
                        PlayersSpawnPositions.Add(playerID, spawnGhost);
                    }
                }
            }
        }

        private void ReinitializeControllersMap()
        {
            List<EControllerID> keys = new List<EControllerID>();
            foreach (EControllerID key in ControllersMap.Keys)
            {
                keys.Add(key);

            }
            foreach (EControllerID controllerID in keys)
            {
                ControllersMap[controllerID] = EPlayerID.SPECTATOR;
            }
        }
        #endregion

        #region Debug
        private void UpdatePartyDebugText()
        {
            string playerStatusLog = "Party join status : \n";
            foreach (var pair in partyStatusMap)
            {
                playerStatusLog += pair.Key + " : " + pair.Value.ControllerID + " - joined : " + pair.Value.HasJoined + " | is ready : " + pair.Value.IsReady + "\n";
            }
            LogCanvas(BConsts.DEBUGTEXT_JoinedPlayers, playerStatusLog);
        }
        #endregion

        #region Others

        private void AssignPlayerToTeam(EPlayerID playerID, ETeamID teamID)
        {
            // First remove from old team
            ETeamID oldTeamID = PlayersTeam[playerID];
            if (oldTeamID != ETeamID.NONE)
            {
                Teams[oldTeamID].Remove(playerID);
            }

            Teams[teamID].Add(playerID);
            PlayersTeam[playerID] = teamID;
        }

        private void RemovePlayerFromTeam(EPlayerID playerID, ETeamID teamID)
        {
            // TODO: Implement removing player from team
        }

        #endregion

    }
}