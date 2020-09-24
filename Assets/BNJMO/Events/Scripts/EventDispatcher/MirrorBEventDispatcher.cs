using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

namespace BNJMO
{
    public class MirrorBEventDispatcher : AbstractBEventDispatcher
    {
        #region Public Methods
        public override BEventDispatcherType GetBEventDispatcherType()
        {
            return BEventDispatcherType.MIRROR;
        }

        public MirrorPlayerBEventListener LocalMirrorPlayerBEventListener { get; set; }

        public NetworkIdentity GetNetworkIdentity(ENetworkID networkID)
        {
            NetworkIdentity networkIdentity = null;
            if (IS_KEY_CONTAINED(connectedNetworkIDs, networkID))
            {
                networkIdentity = connectedNetworkIDs[networkID].NetworkIdentity;
            }
            return networkIdentity;
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
            if (MirrorNetworkConnectionsListener.IsInstanceSet)
            {
                LocalNetworkID = ENetworkID.HOST;

                MirrorNetworkConnectionsListener.Instance.StartHost();
            }
        }

        public override string[] GetAvailableHosts()
        {
            return discoveredHostIPs.ToArray();
        }

        public override void ConnectToHost(int hostID)
        {
            if (MirrorNetworkConnectionsListener.IsInstanceSet)
            {
                if (MirrorNetworkConnectionsListener.Instance.ConnectToFirstServer() == false)
                {
                    LogConsoleError("Could not connect to host : " + hostID);
                }
            }
        }

        public override void Disconnect()
        {
            if (MirrorNetworkConnectionsListener.IsInstanceSet)
            {
                MirrorNetworkConnectionsListener.Instance.Disconnect();
            }
        }
        #endregion

        #region Private Variables
        private float connectionTimeoutInterval = 5.0f;
        private Dictionary<ENetworkID, MirrorPlayerBEventListener> connectedNetworkIDs = new Dictionary<ENetworkID, MirrorPlayerBEventListener>();
        private List<string> discoveredHostIPs = new List<string>();

        #endregion

        #region Life Cycle
        protected override void OnGUI()
        {
            base.OnGUI();

            string ipAddress = BUtils.GetLocalIPAddress();
            GUI.Box(new Rect(10, Screen.height - 50, 100, 30), ipAddress);
        }

        protected override void Awake()
        {
            base.Awake();

            LocalNetworkID = ENetworkID.LOCAL;


            // Listen to new descovered networks
            if (MirrorNetworkConnectionsListener.IsInstanceSet)
            {
                MirrorNetworkConnectionsListener.Instance.DiscoveredServersUpdated += On_DiscoveredServersUpdated;
            }
        }

        protected override void Update()
        {
            base.Update();

            // Server
            if (LocalNetworkID == ENetworkID.HOST)
            {
                foreach (var pair in connectedNetworkIDs)
                {
                    LogCanvas(BConsts.DEBUGTEXT_MirrorPlayers, pair.Key.ToString());
                }
            }
            else // Client
            {
                if (LocalMirrorPlayerBEventListener)
                {
                    LogCanvas(BConsts.DEBUGTEXT_MirrorPlayers, "Server : " + LocalMirrorPlayerBEventListener.NetworkID);
                }
            }
        }
        #endregion

        #region BEvent Dispatch
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

                    // Server
                    if (LocalNetworkID == ENetworkID.HOST)
                    {
                        if (IS_NOT_NULL(LocalMirrorPlayerBEventListener))
                        {
                            // Broadcast it to everyone in connectedNetworkControllers
                            LocalMirrorPlayerBEventListener.Server_RequestBroadcastEvent(bEHandle, eventInvocationType, LocalNetworkID, targetNetworkID);
                        }
                    }
                    else // Client
                    {
                        if (IS_NOT_NULL(LocalMirrorPlayerBEventListener))
                        {
                            // Send to server, which then broadcasts it to everyone in connectedNetworkControllers
                            LocalMirrorPlayerBEventListener.Client_RequestBroadcastEvent(bEHandle, eventInvocationType, LocalNetworkID, targetNetworkID);
                        }
                    }
                    break;

                case BEventReplicationType.TO_ALL_OTHERS:
                    // Server
                    if (LocalNetworkID == ENetworkID.HOST)
                    {
                        if (IS_NOT_NULL(LocalMirrorPlayerBEventListener))
                        {
                            // Broadcast it to everyone in connectedNetworkControllers
                            LocalMirrorPlayerBEventListener.Server_RequestBroadcastEvent(bEHandle, eventInvocationType, LocalNetworkID, targetNetworkID);
                        }
                    }
                    else // Client
                    {
                        if (IS_NOT_NULL(LocalMirrorPlayerBEventListener))
                        {
                            // Send to server, which then broadcasts it to everyone in connectedNetworkControllers
                            LocalMirrorPlayerBEventListener.Client_RequestBroadcastEvent(bEHandle, eventInvocationType, LocalNetworkID, targetNetworkID);
                        }
                    }
                    break;

                case BEventReplicationType.LOCAL:
                    // Broadcast locally
                    bEvent.OnProceedInvokation(bEHandle);
                    break;

                case BEventReplicationType.TO_TARGET:
                    // Server
                    if (LocalNetworkID == ENetworkID.HOST)
                    {
                        if (IS_NOT_NULL(LocalMirrorPlayerBEventListener))
                        {
                            // Broadcast it to target in connectedNetworkControllers
                            LocalMirrorPlayerBEventListener.Server_RequestBroadcastEvent(bEHandle, eventInvocationType, LocalNetworkID, targetNetworkID);
                        }
                    }
                    else // Client
                    {
                        if (IS_NOT_NULL(LocalMirrorPlayerBEventListener))
                        {
                            // Send to server, which then broadcasts it to target in connectedNetworkControllers
                            LocalMirrorPlayerBEventListener.Client_RequestBroadcastEvent(bEHandle, eventInvocationType, LocalNetworkID, targetNetworkID);
                        }
                    }
                    break;
            }
        }
        #endregion

        #region Events Callbacks
        private void On_DiscoveredServersUpdated(long[] discoveredServers)
        {
            // See if the new updated list is the same as the previous one
            List<string> newList = new List<string>();
            foreach (KeyValuePair<long, ServerResponse> pair in MirrorNetworkConnectionsListener.Instance.DiscoveredServers)
            {
                ServerResponse serverResponse = pair.Value;
                newList.Add((serverResponse.serverId).ToString());
            }

            if (Enumerable.SequenceEqual(discoveredHostIPs, newList) == false)
            {
                LogConsole("List updated");
                discoveredHostIPs = newList;
                BEventsCollection.NETWORK_DiscoveredHostsUpdated.Invoke(new BEHandle<string[]>(discoveredHostIPs.ToArray()));
            }
        }
        #endregion

        #region Connection Protocol
        /// <summary>
        /// Called from the MirrorPlayerBEventListener on the server when spawned to connect to the next server NetworkID
        /// </summary>
        /// <param name="mirrorPlayerListener"> New MirrorPlayerBEventListener </param>
        public void Server_OnHostStarted(MirrorPlayerBEventListener mirrorPlayerListener)
        {
            if (IS_NOT_NULL(mirrorPlayerListener)
                && IS_KEY_NOT_CONTAINED(connectedNetworkIDs, ENetworkID.HOST))
            {
                LocalNetworkID = ENetworkID.HOST;

                LogConsole("Adding Server Player Mirror Listener : " + LocalNetworkID);
                connectedNetworkIDs.Add(LocalNetworkID, mirrorPlayerListener);
                LocalMirrorPlayerBEventListener = mirrorPlayerListener;

                Server_RefreshConnectedNetworkControllers();

                BEventManager.Instance.OnUpdatedNetworkState(ENetworkState.HOST, this);
                BEventsCollection.NETWORK_StartedHost.Invoke(new BEHandle());
                BEventsCollection.NETWORK_NewNetworkIDConnected.Invoke(new BEHandle<ENetworkID>(LocalNetworkID));
            }
        }

        /// <summary>
        /// Called from a MirrorPlayerBEventListener when spawned to connect to the next available NetworkID
        /// </summary>
        /// <param name="mirrorPlayerListener"> New MirrorPlayerBEventListener </param>
        /// <returns> Assigned NetworkID </returns>
        public ENetworkID Server_OnNewMirrorPlayerJoined(MirrorPlayerBEventListener mirrorPlayerListener)
        {
            string ipAddress = "";
            ENetworkID networkID = ENetworkID.NONE;

            if (IS_NOT_NULL(mirrorPlayerListener))
            {
                ipAddress = mirrorPlayerListener.IpAdress;

                // Assign a NetworkID
                networkID = GetNextFreeNetworkNetworkID();
                if (ARE_NOT_EQUAL(networkID, ENetworkID.NONE))
                {
                    LogConsole("Adding new Player Mirror Listener : " + networkID);
                    connectedNetworkIDs.Add(networkID, mirrorPlayerListener);

                    BEventsCollection.NETWORK_NewNetworkIDConnected.Invoke(new BEHandle<ENetworkID>(networkID));

                    Server_RefreshConnectedNetworkControllers();
                }
            }

            // Validity Check
            if (networkID == ENetworkID.NONE)
            {
                LogConsoleWarning("No free Controller ID found for new connected Net Controller : " + ipAddress);
            }
            return networkID;
        }

        public void Target_OnConnectedToHost(ENetworkID networkID, MirrorPlayerBEventListener mirrorPlayerBEventListener)
        {
            if (IS_NOT_NONE(networkID))
            {
                LocalNetworkID = networkID;
                LocalMirrorPlayerBEventListener = mirrorPlayerBEventListener;

                BEventManager.Instance.OnUpdatedNetworkState(ENetworkState.CLIENT, this);
                BEventsCollection.NETWORK_ConnectedToHost.Invoke(new BEHandle<ENetworkID>(networkID));
            }
        }

        /// <summary>
        /// Called from the MirrorPlayerBEventListener when destroyed to disconnect the associated NetworkID
        /// </summary>
        /// <param name="mirrorPlayerListener"> MirrorPlayerBEventListener to disconnect </param>
        /// <returns> Assigned NetworkID </returns>
        public void OnMirrorPlayerLeft(MirrorPlayerBEventListener mirrorPlayerListener, ENetworkID networkID)
        {
            // Is Local Player disconnected?
            if (networkID == LocalNetworkID)
            {
                // Is local player the server?
                if (LocalNetworkID == ENetworkID.HOST)
                {
                    connectedNetworkIDs.Clear();
                }

                LocalMirrorPlayerBEventListener = null;
                LocalNetworkID = ENetworkID.LOCAL;

                BEventManager.Instance.OnUpdatedNetworkState(ENetworkState.NOT_CONNECTED, this);
                BEventsCollection.NETWORK_ConnectionStoped.Invoke(new BEHandle(), true);
            }
            else if (LocalNetworkID == ENetworkID.HOST
                && IS_KEY_CONTAINED(connectedNetworkIDs, networkID))
            {
                connectedNetworkIDs.Remove(networkID);

                Server_InformAllDisconnectedClient(networkID);
            }
        }

        public void OnRefreshConnectedNetworkIDs(ENetworkID assignedNetworkID)
        {
            // New reported connection for the first time
            if (connectedNetworkIDs.ContainsKey(assignedNetworkID) == false)
            {
                BEventsCollection.NETWORK_NewNetworkIDConnected.Invoke(new BEHandle<ENetworkID>(assignedNetworkID));
            }
        }

        public void OnClientDisconnected(ENetworkID networkID)
        {
            BEventsCollection.NETWORK_NetworkIDDisconnected.Invoke(new BEHandle<ENetworkID>(networkID));
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

        private void Server_InformAllDisconnectedClient(ENetworkID networkID)
        {
            foreach (KeyValuePair<ENetworkID, MirrorPlayerBEventListener> pair in connectedNetworkIDs)
            {
                MirrorPlayerBEventListener mirrorPlayerBEventListener = pair.Value;

                mirrorPlayerBEventListener.Rpc_OnClientDisconnected(networkID);
            }
        }

        private void Server_RefreshConnectedNetworkControllers() // TODO : Replace by OnClientConnected?
        {
            foreach (KeyValuePair<ENetworkID, MirrorPlayerBEventListener> pair in connectedNetworkIDs)
            {
                MirrorPlayerBEventListener mirrorPlayerBEventListener = pair.Value;
                ENetworkID networkIDItr = pair.Key;
                LogConsole("Sending refresh for : " + networkIDItr);
                mirrorPlayerBEventListener.Server_RefreshAssignedNetworkID(networkIDItr);
            }
        }
        #endregion

        #region Others
        
        #endregion
    }
}
