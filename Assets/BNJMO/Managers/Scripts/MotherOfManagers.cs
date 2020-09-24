using Sirenix.OdinInspector;
using UnityEngine;

namespace BNJMO
{
    [System.Serializable]
    public struct PlayerTupple
    {
        public EControllerID controllerID;
        public EPlayerID playerID;
    }

    public class MotherOfManagers : AbstractSingletonManager<MotherOfManagers>
    {
        // When adding a new attribute here, remember to add profile setup in SpawnManager class !!!

        [BoxGroup("Mother Of Managers", centerLabel: true)]

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Scene")]
        public EAppScene StartScene = EAppScene.NONE;

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/App")]
        public int TargetFramRate = 30;


        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Events (Networking)")]
        public BEventDispatcherType EventDispatcherType;

        [SerializeField]
        [FoldoutGroup("Mother Of Managers/Events (Networking)")]
        public EBEHandleSerializationMethod bEHandleSerializationMethod = EBEHandleSerializationMethod.JSON_NEWTONSOFT; // TODO: Use   

        [SerializeField]
        [FoldoutGroup("Mother Of Managers/Events (Networking)")]
        [DisableIf("@this.EventDispatcherType != BEventDispatcherType.UBI_INTERACT")]
        public string UbiiBackendServerIP = "localhost";

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Events (Networking)")]
        [DisableIf("@this.EventDispatcherType != BEventDispatcherType.UBI_INTERACT")]
        public int UbiiBackendServerPort = 8101;


        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/AR")]
        public ARTrackingMode ARTrackingMode = ARTrackingMode.NONE;

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/AR")]
        public EPlayAreaType PlayAreaType = EPlayAreaType.NONE;


        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Input")]
        public bool ConnectTouchController = false;

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Input")]
        public bool ConnectAIControllers = false;

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Input")]
        public bool TransformInpuAxisToCameraDirection = false;


        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Game")]
        public bool IsUseDebugGameMode = false;

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Game")]
        public EGameMode DebugGameMode = EGameMode.NONE;


        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Player")]
        public bool IsSpawnGhostPlayerPositionsIfNotFound = false;

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Player")]
        public bool SpawnPlayersUnderSameTransformAsSpawnPositions = false;


        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/AI")]
        public int MaximumNumberOfAIToSpawn = 0;


        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Debug")]
        public bool IsDebugLogEvents = true;

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Debug")]
        public bool IsDebugEventsNetworkID = false;

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Debug")]
        public bool LogMissingDebugTexts = true;

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Debug")]
        public bool DebugButtonEvents = false;

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Debug")]
        public bool DebugJoystickEvents = false;

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Debug")]
        public bool DebugUIButtonsEvents = false;

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Debug")]
        [DisableIf("@this.EventDispatcherMode != BEventDispatcherMode.UBI_INTERACT")]
        public bool DebugUbiiTopicPublish = false;

        [SerializeField] 
        [FoldoutGroup("Mother Of Managers/Debug")]
        [DisableIf("@this.EventDispatcherMode != BEventDispatcherMode.UBI_INTERACT")]
        public bool DebugUbiiTopicRecieve = false;



        protected override void Start()
        {
            base.Start();
            Application.targetFrameRate = TargetFramRate;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}