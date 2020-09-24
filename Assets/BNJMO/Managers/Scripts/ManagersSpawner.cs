using Sirenix.OdinInspector;
using UnityEngine;

namespace BNJMO
{
    /// <summary>
    /// Utility class that makes it possible to start and test any scene (instead of entry scene) by spawning needed managers if they are not found in the scene.
    /// </summary>
    public class ManagersSpawner : MotherOfManagers
    {
        [BoxGroup("Managers Spawner", centerLabel: true)]
        [SerializeField]
        private GameObject motherOfManagersPrefab;

        protected override void OnValidate()
        {
            // Do not call base.OnValidate() to prevent setting Instance of MotherOfManagers to this one
        }

        protected override void Awake()
        {
            // Do not call base.Awake() to prevent setting Instance of MotherOfManagers to this one

            InitializeComponents();
            InitializeObjecsInScene();

            MotherOfManagers managersInstance = FindObjectOfType<MotherOfManagers>();
            if ((managersInstance == null) || (managersInstance == this))
            {
                // Load managers prefab 

                if (IS_NOT_NULL(motherOfManagersPrefab))
                {
                    // Set up Mother Of Managers Profile
                    managersInstance = motherOfManagersPrefab.GetComponent<MotherOfManagers>();

                    /* Scene */
                    managersInstance.StartScene = StartScene;
                    managersInstance.TargetFramRate = TargetFramRate;

                    /* Events */
                    managersInstance.EventDispatcherType = EventDispatcherType;
                    managersInstance.bEHandleSerializationMethod = bEHandleSerializationMethod;
                    managersInstance.UbiiBackendServerIP = UbiiBackendServerIP;
                    managersInstance.UbiiBackendServerPort = UbiiBackendServerPort;

                    /* AR */
                    managersInstance.ARTrackingMode = ARTrackingMode;
                    managersInstance.PlayAreaType = PlayAreaType;

                    /* Input */
                    managersInstance.ConnectTouchController = ConnectTouchController;
                    managersInstance.ConnectAIControllers = ConnectAIControllers; 
                    managersInstance.TransformInpuAxisToCameraDirection = TransformInpuAxisToCameraDirection;

                    /* Game */
                    managersInstance.IsUseDebugGameMode = IsUseDebugGameMode;
                    managersInstance.DebugGameMode = DebugGameMode;

                    /* Player */
                    managersInstance.IsSpawnGhostPlayerPositionsIfNotFound = IsSpawnGhostPlayerPositionsIfNotFound;
                    managersInstance.SpawnPlayersUnderSameTransformAsSpawnPositions = SpawnPlayersUnderSameTransformAsSpawnPositions;
                    
                    /* AI */
                    managersInstance.MaximumNumberOfAIToSpawn = MaximumNumberOfAIToSpawn;

                    /* Debug */
                    managersInstance.IsDebugLogEvents = IsDebugLogEvents;
                    managersInstance.IsDebugEventsNetworkID = IsDebugEventsNetworkID;
                    managersInstance.LogMissingDebugTexts = LogMissingDebugTexts;
                    managersInstance.DebugButtonEvents = DebugButtonEvents;
                    managersInstance.DebugJoystickEvents = DebugJoystickEvents;
                    managersInstance.DebugUIButtonsEvents = DebugUIButtonsEvents;
                    managersInstance.DebugUbiiTopicPublish = DebugUbiiTopicPublish;
                    managersInstance.DebugUbiiTopicRecieve = DebugUbiiTopicRecieve;

                    // Spawn managers prefab
                    Instantiate(motherOfManagersPrefab);
                }
            }
        }
    }
}