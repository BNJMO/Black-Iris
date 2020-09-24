using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubii.Services;
using Ubii.TopicData;
using Ubii.UtilityFunctions.Parser;
using System.Threading.Tasks;
using System;
using System;
using Newtonsoft.Json;

namespace BNJMO
{
    public class UbiiBEventDispatcher : AbstractBEventDispatcher, IUbiiClient
    {

        #region Public Events

        #endregion

        #region Public Methods
        /// <summary>
        /// True if the dispatcher is connected to the Ubii backend server.
        /// </summary>
        public bool IsConnectedToUbii { get; private set; } = false;

        /* AbstractBEventDispatcher */
        public override BEventDispatcherType GetBEventDispatcherType()
        {
            return BEventDispatcherType.UBI_INTERACT;
        }

        public override ENetworkID[] GetConnectedNetworkIDs()
        {
            List<ENetworkID> list = new List<ENetworkID>();
            foreach (ENetworkID networkID in connectedNetworkIDs.Keys)
            {
                list.Add(networkID);
            }
            return list.ToArray();
        }

        public override void StartHost()
        {
            Publish_TOPIC_START_HOST();
        }

        public override string[] GetAvailableHosts()
        {
            return discoveredHosts.ToArray();
        }

        public override void ConnectToHost(int hostID)
        {
            Publish_TOPIC_REQUEST_JOIN_HOST(hostID);
        }

        public override void Disconnect()
        {
            if (LocalNetworkID == ENetworkID.HOST)
            {
                Publish_TOPIC_HOST_DISCONNECTED();

            }
            else if (IsClient() == true)
            {
                Publish_TOPIC_CLIENT_DISCONNECTED();
            }

            OnDisconnected();
        }

        public override void OnBEventInvoked<H>(BEvent<H> bEvent, H bEHandle, BEventReplicationType eventInvocationType, ENetworkID targetNetworkID)
        {
            // Broadcast locally if still not connected
            if (LocalNetworkID == ENetworkID.LOCAL)
            {
                bEvent.OnProceedInvokation(bEHandle);
                return;
            }

            switch (eventInvocationType)
            {
                case BEventReplicationType.TO_ALL:
                    //Broadcast locally
                    bEvent.OnProceedInvokation(bEHandle);

                    // Publish to all other clients on the network
                    foreach (ENetworkID networkID in connectedNetworkIDs.Keys)
                    {
                        if (networkID != LocalNetworkID)
                        {
                            Publish_TOPIC_BROADCAST_BEVENT(bEHandle, networkID);
                        }
                    }
                    break;

                case BEventReplicationType.TO_ALL_OTHERS:
                    // Publish to all other clients on the network
                    foreach (ENetworkID networkID in connectedNetworkIDs.Keys)
                    {
                        if (networkID != LocalNetworkID)
                        {
                            Publish_TOPIC_BROADCAST_BEVENT(bEHandle, networkID);
                        }
                    }
                    break;

                case BEventReplicationType.LOCAL:
                    // Broadcast locally
                    bEvent.OnProceedInvokation(bEHandle);
                    break;

                case BEventReplicationType.TO_TARGET:
                    if (LocalNetworkID == targetNetworkID)
                    {
                        // Broadcast locally
                        bEvent.OnProceedInvokation(bEHandle);
                    }
                    else if (IS_NOT_NONE(targetNetworkID))
                    {
                        Publish_TOPIC_BROADCAST_BEVENT(bEHandle, targetNetworkID);
                    }
                    break;
            }
        }

        /* IUbiiClient */
        public Task<ServiceReply> CallService(ServiceRequest request)
        {
            if (IS_NOT_NULL(ubiiClient))
            {
                return ubiiClient.CallService(request);
            }
            return null;
        }

        public void Publish(TopicData topicData)
        {
            if (IS_NOT_NULL(ubiiClient))
            {
                ubiiClient.Publish(topicData);
            }
        }

        public Task<ServiceReply> Subscribe(string topic, Action<TopicDataRecord> callback)
        {
            if (IS_NOT_NULL(ubiiClient))
            {
                return ubiiClient.Subscribe(topic, callback);
            }
            return null;
        }

        public Task<ServiceReply> SubscribeRegex(string regex, Action<TopicDataRecord> callback)
        {
            if (IS_NOT_NULL(ubiiClient))
            {
                return ubiiClient.SubscribeRegex(regex, callback);
            }
            return null;
        }

        public Task<ServiceReply> Unsubscribe(string topic)
        {
            if (IS_NOT_NULL(ubiiClient))
            {
                return ubiiClient.Unsubscribe(topic);
            }
            return null;
        }

        #endregion

        #region Inspector Variables


        #endregion

        #region Private Variables
        private NetMQUbiiClient ubiiClient;
        private Queue<TopicDataRecord> jobsQueue = new Queue<TopicDataRecord>();
        private string localGuid;
        private string joinedHostGuid;
        private Dictionary<ENetworkID, string> connectedNetworkIDs = new Dictionary<ENetworkID, string>();
        private List<string> discoveredHosts = new List<string>();
        private IEnumerator requestHostEnumerator;

        private const string TOPIC_START_HOST = "START_HOST"; // 1
        private const string TOPIC_REQUEST_HOST_IP = "REQUEST_HOST_IP"; // 2
        private const string TOPIC_BROADCAST_HOST_IP = "BROADCAST_HOST_IP"; // 3
        private const string TOPIC_REQUEST_JOIN_HOST = "REQUEST_JOIN_HOST"; // 4
        private const string TOPIC_CLIENT_CONNECTED = "CLIENT_CONNECTED"; // 5
        private const string TOPIC_CLIENT_DISCONNECTED = "CLIENT_DISCONNECTED"; // 6
        private const string TOPIC_HOST_DISCONNECTED = "HOST_DISCONNECTED"; // 7
        private const string TOPIC_BROADCAST_BEVENT = "BROADCAST_BEVENT"; // 8
        private const string TOPIC_TEST_INT = "TEST_INT"; // 9

        #endregion

        #region Life Cycle

        protected override void Awake()
        {
            base.Awake();

            localGuid = Guid.NewGuid().ToString();
            LocalNetworkID = ENetworkID.LOCAL;
        }

        protected async override void Start()
        {
            base.Start();

            // Initializing NetMQUbiiClient (sets serverSpecification & clientRegistration, starts sockets)
            // TODO: Set advanced specs

            string ubiiIP = MotherOfManagers.Instance.UbiiBackendServerIP;
            int ubiiPort = MotherOfManagers.Instance.UbiiBackendServerPort;
            ubiiClient = new NetMQUbiiClient(null, "testClient", ubiiIP, ubiiPort); 
            LogConsole("Initilizing connection to : " + ubiiIP + " - " + ubiiPort);
            await ubiiClient.Initialize();
            LogConsole("Connection established");

            LogConsole("Subscribing to topics");
            await Subscribe(TOPIC_START_HOST, On_AsyncJobRecieved);
            await Subscribe(TOPIC_REQUEST_HOST_IP, On_AsyncJobRecieved);
            await Subscribe(TOPIC_BROADCAST_HOST_IP, On_AsyncJobRecieved); 
            await Subscribe(TOPIC_REQUEST_JOIN_HOST, On_AsyncJobRecieved);
            await Subscribe(TOPIC_CLIENT_CONNECTED, On_AsyncJobRecieved);
            await Subscribe(TOPIC_CLIENT_DISCONNECTED, On_AsyncJobRecieved);
            await Subscribe(TOPIC_HOST_DISCONNECTED, On_AsyncJobRecieved);
            await Subscribe(TOPIC_BROADCAST_BEVENT, On_AsyncJobRecieved);
            await Subscribe(TOPIC_TEST_INT, On_AsyncJobRecieved);

            IsConnectedToUbii = true;
            LogConsole("Ubii connected");

            StartNewCoroutine(ref requestHostEnumerator, RequestHostCoroutine());
        }

        protected override void Update()
        {
            base.Update();

            while (jobsQueue.Count > 0)
            {
                On_SyncJobRecieved(jobsQueue.Dequeue());
            }

            // Debug localNetowrkID 
            LogCanvas("LocalNetworkID", LocalNetworkID.ToString());
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (IS_NOT_NULL(ubiiClient))
            {
                Disconnect();

                // Stop Ubii
                ubiiClient.ShutDown();
            }
        }

        #endregion

        #region Events Callbacks

        /* async Topic Callbacks */
        private void On_AsyncJobRecieved(TopicDataRecord topicDataRecord)
        {
            // Add Job
            jobsQueue.Enqueue(topicDataRecord);
        }

        /* sync Topic Callbacks */
        private void On_SyncJobRecieved(TopicDataRecord topicDataRecord)
        {
            switch (topicDataRecord.Topic)
            {
                case TOPIC_START_HOST:
                    On_TOPIC_START_HOST(topicDataRecord);
                    break;

                case TOPIC_REQUEST_HOST_IP:
                    On_TOPIC_REQUEST_HOST_IP(topicDataRecord);
                    break;

                case TOPIC_BROADCAST_HOST_IP:
                    On_TOPIC_BROADCAST_HOST_IP(topicDataRecord);
                    break;

                case TOPIC_REQUEST_JOIN_HOST:
                    On_TOPIC_REQUEST_JOIN_HOST(topicDataRecord);
                    break;

                case TOPIC_CLIENT_CONNECTED:
                    On_TOPIC_CLIENT_CONNECTED(topicDataRecord);
                    break;

                case TOPIC_CLIENT_DISCONNECTED:
                    On_TOPIC_CLIENT_DISCONNECTED(topicDataRecord);
                    break;

                case TOPIC_HOST_DISCONNECTED:
                    On_TOPIC_HOST_DISCONNECTED(topicDataRecord);
                    break;

                case TOPIC_BROADCAST_BEVENT:
                    On_TOPIC_BROADCAST_BEVENT(topicDataRecord);
                    break;
                
                case TOPIC_TEST_INT:
                    On_TOPIC_TEST_INT(topicDataRecord);
                    break;
            }
        }


        private void On_TOPIC_START_HOST(TopicDataRecord topicDataRecord)
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicRecieve)
            {
                Debug.Log("On_TOPIC_START_HOST");
            }

            if (topicDataRecord.String == localGuid)
            {
                LocalNetworkID = ENetworkID.HOST;
                joinedHostGuid = localGuid;

                connectedNetworkIDs.Add(LocalNetworkID, BEventManager.Instance.LocalIPAddress);

                BEventManager.Instance.OnUpdatedNetworkState(ENetworkState.HOST, this);

                BEventsCollection.NETWORK_StartedHost.Invoke(new BEHandle());
                BEventsCollection.NETWORK_NewNetworkIDConnected.Invoke(new BEHandle<ENetworkID>(LocalNetworkID));
            }
        }

        private void On_TOPIC_REQUEST_HOST_IP(TopicDataRecord topicDataRecord)
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicRecieve)
            {
                Debug.Log("On_TOPIC_REQUEST_HOST_IP");
            }

            if (LocalNetworkID == ENetworkID.HOST)
            {
                Publish_TOPIC_BROADCAST_HOST_IP();
            }
        }

        private void On_TOPIC_BROADCAST_HOST_IP(TopicDataRecord topicDataRecord)
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicRecieve)
            {
                Debug.Log("On_TOPIC_BROADCAST_HOST_IP");
            }

            if (LocalNetworkID == ENetworkID.LOCAL)
            {
                string newHostGuid = topicDataRecord.String;
                if (discoveredHosts.Contains(newHostGuid) == false)
                {
                    discoveredHosts.Add(newHostGuid);

                    // TODO: Start a connection Timeout coroutine

                    BEventsCollection.NETWORK_DiscoveredHostsUpdated.Invoke(new BEHandle<string[]>(discoveredHosts.ToArray()));
                }
            }
        }

        private void On_TOPIC_REQUEST_JOIN_HOST(TopicDataRecord topicDataRecord)
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicRecieve)
            {
                Debug.Log("On_TOPIC_REQUEST_JOIN_HOST");
            }

            if (LocalNetworkID == ENetworkID.HOST)
            {
                // Split message
                string[] parameters = topicDataRecord.String.Split('|');
                if (ARE_EQUAL(parameters.Length, 2))
                {
                    string requestingGuid = parameters[0];
                    string requestedHostGuid = parameters[1];

                    // Check if this instanced is the one concerned
                    if (requestedHostGuid == localGuid
                        && IS_VALUE_NOT_CONTAINED(connectedNetworkIDs, requestingGuid))
                    {
                        ENetworkID networkID = GetNextFreeNetworkNetworkID();
                        if (ARE_NOT_EQUAL(networkID, ENetworkID.NONE))
                        {
                            connectedNetworkIDs.Add(networkID, requestingGuid);

                            // Publish topic
                            Publish_TOPIC_CLIENT_CONNECTED(requestingGuid, networkID);

                            // Resend a Publish_TOPIC_CLIENT_CONNECTED for every other connected client (after a short delay?)
                            StartCoroutine(RefreshConnectedClientsCoroutine());

                            // Trigger local BEvent
                            BEventsCollection.NETWORK_NewNetworkIDConnected.Invoke(new BEHandle<ENetworkID>(networkID));
                        }
                    }
                }
            }
        }

        private void On_TOPIC_CLIENT_CONNECTED(TopicDataRecord topicDataRecord)
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicRecieve)
            {
                Debug.Log("On_TOPIC_CLIENT_CONNECTED");
            }

            if (LocalNetworkID != ENetworkID.HOST)
            {
                // Split parameters
                string[] parameters = topicDataRecord.String.Split('|');
                if (ARE_EQUAL(parameters.Length, 3))
                {
                    string hostGuid = parameters[0];
                    string connectedClientGuid = parameters[1];
                    ENetworkID networkID = ENetworkID.NONE;
                    Enum.TryParse(parameters[2], out networkID);

                    if (IS_NOT_NONE(networkID))
                    {
                        // Is requesting connection client
                        if (LocalNetworkID == ENetworkID.LOCAL
                            && localGuid == connectedClientGuid)
                        {
                            LocalNetworkID = networkID;
                            joinedHostGuid = hostGuid;

                            // Update network state
                            BEventManager.Instance.OnUpdatedNetworkState(ENetworkState.CLIENT, this);

                            // Trigger event 
                            BEventsCollection.NETWORK_ConnectedToHost.Invoke(new BEHandle<ENetworkID>(networkID));

                            // Add host and self to connected clients (other connected clients will be reset from the server later)
                            if (ARE_EQUAL(connectedNetworkIDs.Count, 0))
                            {
                                connectedNetworkIDs.Add(ENetworkID.HOST, joinedHostGuid);
                                BEventsCollection.NETWORK_NewNetworkIDConnected.Invoke(new BEHandle<ENetworkID>(ENetworkID.HOST));

                                connectedNetworkIDs.Add(LocalNetworkID, localGuid);
                                BEventsCollection.NETWORK_NewNetworkIDConnected.Invoke(new BEHandle<ENetworkID>(networkID));
                            }
                        }
                        // Is a client connected to the same host 
                        else if (IsClient() == true
                            && hostGuid == joinedHostGuid
                            && connectedNetworkIDs.ContainsKey(networkID) == false)
                        {
                            // Add new client
                            connectedNetworkIDs.Add(networkID, connectedClientGuid);
                            BEventsCollection.NETWORK_NewNetworkIDConnected.Invoke(new BEHandle<ENetworkID>(networkID));
                        }
                    }
                }
            }
        }

        private void On_TOPIC_CLIENT_DISCONNECTED(TopicDataRecord topicDataRecord)
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicRecieve)
            {
                Debug.Log("On_TOPIC_CLIENT_DISCONNECTED");
            }

            if (LocalNetworkID != ENetworkID.LOCAL)
            {
                string[] parameters = topicDataRecord.String.Split('|');
                if (ARE_EQUAL(parameters.Length, 3))
                {
                    string disconnectedClientGuid = parameters[0];
                    string hostGuid = parameters[1];
                    ENetworkID networkID = ENetworkID.NONE;
                    Enum.TryParse(parameters[2], out networkID);

                    // For both Host and Clients
                    if (hostGuid == joinedHostGuid
                        && IS_KEY_CONTAINED(connectedNetworkIDs, networkID))
                    {
                        connectedNetworkIDs.Remove(networkID);

                        BEventsCollection.NETWORK_NetworkIDDisconnected.Invoke(new BEHandle<ENetworkID>(networkID));
                    }
                }
            }
        }

        private void On_TOPIC_HOST_DISCONNECTED(TopicDataRecord topicDataRecord)
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicRecieve)
            {
                Debug.Log("On_TOPIC_HOST_DISCONNECTED");
            }

            string disconnectedHostGuid = topicDataRecord.String;
            if (IsClient() == true
                && joinedHostGuid == disconnectedHostGuid) 
            {
                OnDisconnected();
            }
        }

        private void On_TOPIC_BROADCAST_BEVENT(TopicDataRecord topicDataRecord)
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicRecieve)
            {
                Debug.Log("On_TOPIC_BROADCAST_BEVENT");
            }

            if (LocalNetworkID != ENetworkID.LOCAL)
            {
                // Split the parameters
                string[] parameters = topicDataRecord.String.Split('|');
                if (parameters.Length >= 3)
                {
                    string joinedHostGuid = parameters[0];
                    ENetworkID targetNetworkID = ENetworkID.NONE;
                    Enum.TryParse(parameters[1], out targetNetworkID);
                    string serializedBEHandle = parameters[2];

                    // Same host and Is targeted client
                    if (joinedHostGuid == this.joinedHostGuid
                        && targetNetworkID == LocalNetworkID)
                    {
                        // Did the serialized BEHandle got split?
                        if (parameters.Length > 3)
                        {
                            LogConsoleWarning("The BEhandle got split. Trying to reconstruct it");
                            for (int i = 3; i < parameters.Length; i++)
                            {
                                serializedBEHandle += '|' + parameters[i];
                            }
                        }

                        // Proceed the deserialization of the BEHandle
                        BEventManager.Instance.OnBEventReplicated(serializedBEHandle);
                    }
                }
            }
        }

        private void On_TOPIC_TEST_INT(TopicDataRecord topicDataRecord)
        {
            LogConsole("On_TOPIC_TEST_INT : " + topicDataRecord.Double + " - " + BUtils.GetTimeAsString());
        }

        #endregion

        #region Topic Publish Methods
        private void Publish_TOPIC_START_HOST()
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicPublish)
            {
                LogConsole("Publish_TOPIC_START_HOST");
            }

            if (ARE_EQUAL(LocalNetworkID, ENetworkID.LOCAL))
            {
                Publish(UbiiParser.UnityToProto(TOPIC_START_HOST, localGuid));
            }
        }
                
        private void Publish_TOPIC_REQUEST_HOST_IP()
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicPublish)
            {
                LogConsole("Publish_TOPIC_REQUEST_HOST_IP");
            }

            if (ARE_EQUAL(LocalNetworkID, ENetworkID.LOCAL))
            {
                Publish(UbiiParser.UnityToProto(TOPIC_REQUEST_HOST_IP, localGuid));
            }
        }
                     
        private void Publish_TOPIC_BROADCAST_HOST_IP()
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicPublish)
            {
                LogConsole("Publish_TOPIC_BROADCAST_HOST_IP");
            }

            if (ARE_EQUAL(LocalNetworkID, ENetworkID.HOST))
            {
                Publish(UbiiParser.UnityToProto(TOPIC_BROADCAST_HOST_IP, localGuid));
            }
        }

        // TODO : publish a list of all already connected clients to the host
        private void Publish_TOPIC_REQUEST_JOIN_HOST(int hostID)
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicPublish)
            {
                LogConsole("Publish_TOPIC_REQUEST_JOIN_HOST");
            }

            if (ARE_EQUAL(LocalNetworkID, ENetworkID.LOCAL)
                && (discoveredHosts.Count > hostID))
            {
                string hostGuid = discoveredHosts[hostID];
                Publish(UbiiParser.UnityToProto(TOPIC_REQUEST_JOIN_HOST, localGuid + "|" + hostGuid));
            }
        }
                        
        private void Publish_TOPIC_CLIENT_CONNECTED(string connectedClientGuid, ENetworkID networkID)
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicPublish)
            {
                LogConsole("Publish_TOPIC_CLIENT_CONNECTED");
            }

            if (ARE_EQUAL(LocalNetworkID, ENetworkID.HOST))
            {
                Publish(UbiiParser.UnityToProto(TOPIC_CLIENT_CONNECTED, localGuid + "|" + connectedClientGuid + "|" + networkID));
            }
        }
                        
        private void Publish_TOPIC_CLIENT_DISCONNECTED()
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicPublish)
            {
                LogConsole("Publish_TOPIC_CLIENT_DISCONNECTED");
            }

            if (IS_TRUE(IsClient()))
            {
                Publish(UbiiParser.UnityToProto(TOPIC_CLIENT_DISCONNECTED, localGuid + '|' + joinedHostGuid + "|" + LocalNetworkID));
            }
        }
                        
        private void Publish_TOPIC_HOST_DISCONNECTED()
        {
            LogConsole("Publish_TOPIC_HOST_DISCONNECTED");

            if (ARE_EQUAL(LocalNetworkID, ENetworkID.HOST))
            {
                Publish(UbiiParser.UnityToProto(TOPIC_HOST_DISCONNECTED, localGuid));
            }
        }
                        
        private void Publish_TOPIC_BROADCAST_BEVENT(AbstractBEHandle bEHandle, ENetworkID targetNetworkID)
        {
            // Debug
            if (MotherOfManagers.Instance.DebugUbiiTopicPublish)
            {
                LogConsole("Publish_TOPIC_BROADCAST_BEVENT");
            }

            if (ARE_NOT_EQUAL(LocalNetworkID, targetNetworkID))
            {
                string serializedBEHandle = BUtils.SerializeObject(bEHandle);


                Publish(UbiiParser.UnityToProto(TOPIC_BROADCAST_BEVENT, joinedHostGuid + "|" + targetNetworkID + "|" + serializedBEHandle));
            }
        }

        public void Publish_TOPIC_TEST_INT(int integer)
        {
            LogConsole("Publish_TOPIC_TEST_INT : " + integer + " - " + BUtils.GetTimeAsString());
            Publish(UbiiParser.UnityToProto(TOPIC_TEST_INT, integer));
        }
        #endregion

        #region Others
        private IEnumerator BroadcastHostIPCoroutine()
        {
            while (LocalNetworkID == ENetworkID.HOST)
            {
                Publish_TOPIC_BROADCAST_HOST_IP();

                yield return new WaitForSeconds(2.0f);
            }
        }

        private IEnumerator RequestHostCoroutine()
        {
            while (BEventManager.Instance.LocalNetworkID == ENetworkID.LOCAL)
            {
                Publish_TOPIC_REQUEST_HOST_IP();

                yield return new WaitForSeconds(3.0f);
            }
        }

        private ENetworkID GetNextFreeNetworkNetworkID()
        {
            ENetworkID netowrkID = ENetworkID.NONE;
            foreach (ENetworkID networkIDitr in System.Enum.GetValues(typeof(ENetworkID)))
            {
                if (networkIDitr == ENetworkID.NONE)
                {
                    continue;
                }

                if (connectedNetworkIDs.ContainsKey(networkIDitr) == false)
                {
                    netowrkID = networkIDitr;
                    break;
                }
            }
            return netowrkID;
        }

        private void OnDisconnected()
        {
            // Reinitialize variables
            connectedNetworkIDs.Clear();
            LocalNetworkID = ENetworkID.LOCAL;
            joinedHostGuid = "";
            discoveredHosts.Clear();

            // Update network state
            BEventManager.Instance.OnUpdatedNetworkState(ENetworkState.NOT_CONNECTED, this);

            // Trigger event
            BEventsCollection.NETWORK_ConnectionStoped.Invoke(new BEHandle(), true);
            BEventsCollection.NETWORK_DiscoveredHostsUpdated.Invoke(new BEHandle<string[]>(GetAvailableHosts()));

            // Restart network discovery
            StartNewCoroutine(ref requestHostEnumerator, RequestHostCoroutine());
        }

        private bool IsClient()
        {
            return LocalNetworkID != ENetworkID.NONE
                && LocalNetworkID != ENetworkID.LOCAL
                && LocalNetworkID != ENetworkID.HOST;
        }

        private IEnumerator RefreshConnectedClientsCoroutine()
        {
            yield return new WaitForSeconds(0.5f);

            foreach (KeyValuePair<ENetworkID, string> pair in connectedNetworkIDs)
            {
                Publish_TOPIC_CLIENT_CONNECTED(pair.Value, pair.Key);
            }
        }

        #endregion

    }
}
