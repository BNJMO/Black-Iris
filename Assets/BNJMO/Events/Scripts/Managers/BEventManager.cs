using UnityEngine;
using Mirror;

namespace BNJMO
{
    public class BEventManager : AbstractSingletonManager<BEventManager>
    {
        #region Public Events

        #endregion

        #region Public Methods
        public AbstractBEventDispatcher BEventDispatcher { get; private set; }

        public ENetworkState NetworkState { get; private set; } = ENetworkState.NOT_CONNECTED;

        public ENetworkID LocalNetworkID
        {
            get
            {
                if (IS_NOT_NULL(BEventDispatcher))
                {
                    return BEventDispatcher.LocalNetworkID;
                }
                return ENetworkID.NONE;
            }
        }

        public string LocalIPAddress { get; private set; } = "localhost";

        public ENetworkID[] GetConnectedNetworkIDs()
        {
            if (IS_NOT_NULL(BEventDispatcher))
            {
                return BEventDispatcher.GetConnectedNetworkIDs();
            }
            return new ENetworkID[0];
        }

        public void StartHost()
        {
            if (ARE_EQUAL(LocalNetworkID, ENetworkID.LOCAL)
                && IS_NOT_NULL(BEventDispatcher))
            {
                BEventDispatcher.StartHost();
            }
        }

        public string[] GetAvailableHosts()
        {
            if (IS_NOT_NULL(BEventDispatcher))
            {
                return BEventDispatcher.GetAvailableHosts();
            }
            return new string[0];
        }

        public void Disconnect()
        {
            if (ARE_NOT_EQUAL(LocalNetworkID, ENetworkID.LOCAL)
                && IS_NOT_NULL(BEventDispatcher))
            {
                BEventDispatcher.Disconnect();
            }
        }

        public void ConnectToHost(int hostID)
        {
            if (ARE_EQUAL(LocalNetworkID, ENetworkID.LOCAL)
                && IS_NOT_NULL(BEventDispatcher))
            {
                BEventDispatcher.ConnectToHost(hostID);
            }
        }

        public void SetBEventDispatcher(BEventDispatcherType bEventDispatcherMode)
        {
            // Disconnect
            if (NetworkState != ENetworkState.NOT_CONNECTED)
            {
                Disconnect();
            }

            // Remove existing Dispatcher
            AbstractBEventDispatcher bEventDispatcher = FindObjectOfType<AbstractBEventDispatcher>();
            if (bEventDispatcher)
            {
                RemoveBEventDispatcher(bEventDispatcher);
            }

            // Add new Dispatcher
            if (AddBeventDispatcher(bEventDispatcherMode))
            {
                BEventsCollection.NETWORK_NewBEventDispatcherSet.Invoke(new BEHandle<AbstractBEventDispatcher>(BEventDispatcher));
            }
        }

        public int GetPing()
        {
            PingCalculator pingCalculator = FindObjectOfType<PingCalculator>();
            if (IS_NOT_NULL(pingCalculator))
            {
                return pingCalculator.AveragePing;
            }
            return 0;
        }

        #endregion

        #region Inspector Variables


        #endregion

        #region Private Variables

        #endregion

        #region Life Cycle
        protected override void OnEnable()
        {
            base.OnEnable();

        }

        protected override void OnDisable()
        {
            base.OnDisable();

        }

        protected override void Awake()
        {
            base.Awake();

            LocalIPAddress = BUtils.GetLocalIPAddress();

            SetBEventDispatcher(MotherOfManagers.Instance.EventDispatcherType);
        }

        protected override void Update()
        {
            base.Update();

            LogCanvas(BConsts.DEBUGTEXT_NetworkState, NetworkState.ToString());
        }

        #endregion

        #region Events Callbacks
        /* Callbacks from Dispatcher */
        public void OnBEventInvoked<H>(BEvent<H> bEvent, H bEHandle, BEventReplicationType bEInvocationType, ENetworkID targetNetworkID) where H : AbstractBEHandle
        {
            if (BEventDispatcher)
            {
                bEHandle.InvokingNetworkID = LocalNetworkID;
                BEventDispatcher.OnBEventInvoked(bEvent, bEHandle, bEInvocationType, targetNetworkID);
            }
        }

        public void OnBEventReplicated(string serializedBEHandle)
        {
            //AbstractBEHandle deserializedBEHandle = JsonConvert.DeserializeObject<AbstractBEHandle>(serializedBEHandle);
            AbstractBEHandle deserializedBEHandle = BUtils.DeserializeObject<AbstractBEHandle>(serializedBEHandle);

            string callingBEventName = deserializedBEHandle.InvokingBEventName;

            if ((IS_NOT_NULL(BEventsCollection.Instance.AllReplicatedBEvent))
                && (IS_KEY_CONTAINED(BEventsCollection.Instance.AllReplicatedBEvent, callingBEventName))
                && (IS_NOT_NULL(BEventsCollection.Instance.AllReplicatedBEvent[callingBEventName])))
            {
                BEventsCollection.Instance.AllReplicatedBEvent[callingBEventName].OnReplicatedEvent(serializedBEHandle);
            }
        }

        public void OnUpdatedNetworkState(ENetworkState newNetworkState, AbstractBEventDispatcher bEventDispatcher)
        {
            if (ARE_EQUAL(BEventDispatcher, bEventDispatcher))
            {
                NetworkState = newNetworkState;
                BEventsCollection.NETWORK_NetworkStateUpdated.Invoke(new BEHandle<ENetworkState>(NetworkState));
            }
        }
        #endregion

        #region Others
        private bool AddBeventDispatcher(BEventDispatcherType bEventDispatcherMode)
        {
            switch (bEventDispatcherMode)
            {
                case BEventDispatcherType.LOCAL:
                    BEventDispatcher = gameObject.AddComponent<LocalBEventDispatcher>();
                    return true;

                case BEventDispatcherType.MIRROR:
                    // Spawn Mirror's NetworkManager

                    NetworkManager mirrorNetworkManager = FindObjectOfType<NetworkManager>();
                    if (mirrorNetworkManager == null)
                    {
                        LogConsole("Spawning Mirror Network Manager");
                        GameObject mirroNetworkManagerPrefab = Resources.Load<GameObject>(BConsts.PATH_MirrorNetworkManager);
                        if (IS_NOT_NULL(mirroNetworkManagerPrefab))
                        {
                            GameObject mirrorNetworkManagerSpawned = Instantiate(mirroNetworkManagerPrefab);
                            mirrorNetworkManagerSpawned.transform.parent = transform;
                            mirrorNetworkManager = mirrorNetworkManagerSpawned.GetComponent<NetworkManager>();
                        }
                    }

                    // Add Mirror BEvent Dispatcher
                    if (IS_NOT_NULL(mirrorNetworkManager))
                    {
                        BEventDispatcher = gameObject.AddComponent<MirrorBEventDispatcher>();
                        return true;
                    }
                    return false;


                case BEventDispatcherType.UBI_INTERACT:
                    BEventDispatcher = gameObject.AddComponent<UbiiBEventDispatcher>();
                    return true;


                case BEventDispatcherType.NONE:
                    LogConsoleWarning("No Event Dispatcher Mode was selected! Default mode will be used : " + BEventDispatcherType.LOCAL);
                    BEventDispatcher = gameObject.AddComponent<LocalBEventDispatcher>();
                    return true;
            }
            return false;
        }


        private void RemoveBEventDispatcher(AbstractBEventDispatcher bEventDispatcher)
        {
            switch (bEventDispatcher.GetBEventDispatcherType())
            {
                case BEventDispatcherType.MIRROR:
                    NetworkManager mirrorNetworkManager = FindObjectOfType<NetworkManager>();
                    if (mirrorNetworkManager != null)
                    {
                        Destroy(mirrorNetworkManager.gameObject);
                    }
                    break;
            }

            // Destroy Dispatcher Component
            Destroy(bEventDispatcher);
        }

        #endregion
    }
}