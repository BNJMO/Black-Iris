using UnityEngine;
using Mirror;
using Newtonsoft.Json;

namespace BNJMO
{
    public class MirrorPlayerBEventListener : NetworkBehaviour
    {
        public string IpAdress { get { return (NetworkIdentity != null) ? NetworkIdentity.connectionToClient.address : ""; } }
        public NetworkIdentity NetworkIdentity { get; private set; }
        public ENetworkID NetworkID { get { return networkID; } }

        [SerializeField] private ENetworkID networkID = ENetworkID.NONE;

        private void Awake()
        {
            NetworkIdentity = GetComponent<NetworkIdentity>();
        }

        // Owning client is spawned
        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            // Server?
            if (BEventManager.Instance.LocalNetworkID == ENetworkID.HOST)
            {
                Server_RequestServerNetworkID();
            }
            else // Client
            {
                Client_RequestClientNetoworkID();
            }
        }

        private void OnDestroy()
        {
            if (BEventManager.IsInstanceSet)
            {
                MirrorBEventDispatcher mirrorBEventDispatcher = (MirrorBEventDispatcher)BEventManager.Instance.BEventDispatcher;
                if (mirrorBEventDispatcher)
                {
                    mirrorBEventDispatcher.OnMirrorPlayerLeft(this, NetworkID);
                }
            }
        }

        #region Requesting Network ID 
        [Server]
        private void Server_RequestServerNetworkID()
        {
            //Debug.Log("Server_RequestServerNetworkID");

            MirrorBEventDispatcher mirrorBEventDispatcher = (MirrorBEventDispatcher)BEventManager.Instance.BEventDispatcher;
            if (mirrorBEventDispatcher)
            {
                Debug.Log("Assigned Network ID : " + NetworkID);
                /*networkID = */
                mirrorBEventDispatcher.Server_OnHostStarted(this);
                //mirrorBEventDispatcher.LocalNetworkID = NetworkID;
            }
        }

        [Client]
        private void Client_RequestClientNetoworkID()
        {
            //Debug.Log("Client_RequestClientNetoworkID");

            Cmd_OnRequestNetworkID(NetworkIdentity);
        }

        [Command]
        private void Cmd_OnRequestNetworkID(NetworkIdentity networkIdentity)
        {
            //Debug.Log("Cmd_OnRequestNetworkID : " + networkIdentity.connectionToClient.ToString());

            MirrorBEventDispatcher mirrorBEventDispatcher = (MirrorBEventDispatcher)BEventManager.Instance.BEventDispatcher;
            if (mirrorBEventDispatcher)
            {
                ENetworkID newNetworkID = mirrorBEventDispatcher.Server_OnNewMirrorPlayerJoined(this);
                if (newNetworkID != ENetworkID.NONE)
                {
                    Debug.Log("Assigned network ID : " + newNetworkID);
                    Target_OnAssignedNetworkID(networkIdentity.connectionToClient, newNetworkID);
                }
            }
        }

        [TargetRpc]
        private void Target_OnAssignedNetworkID(NetworkConnection networkConnection, ENetworkID assignedNetworkID)
        {
            //Debug.Log("Target_OnAssignedNetworkID : " + assignedNetworkID);

            networkID = assignedNetworkID;

            // Set up MirrorBEventDispatcher
            MirrorBEventDispatcher mirrorBEventDispatcher = (MirrorBEventDispatcher) BEventManager.Instance.BEventDispatcher;
            if (mirrorBEventDispatcher)
            {
                //mirrorBEventDispatcher.LocalNetworkID = NetworkID;
                //mirrorBEventDispatcher.LocalMirrorPlayerBEventListener = this;
                mirrorBEventDispatcher.Target_OnConnectedToHost(networkID, this);
            }
        }

        [Server]
        public void Server_RefreshAssignedNetworkID(ENetworkID assignedNetworkID)
        {
            //Debug.Log("Server_RefreshAssignedNetworkID : " + assignedNetworkID);
            
            Rpc_OnRefreshAssignedNetworkID(assignedNetworkID);
        }

        [ClientRpc]
        private void Rpc_OnRefreshAssignedNetworkID(ENetworkID assignedNetworkID)
        {
            //Debug.Log("Rpc_OnRefreshAssignedNetworkID : " + assignedNetworkID);

            networkID = assignedNetworkID;

            // Connect network if not connected yet
            MirrorBEventDispatcher mirrorBEventDispatcher = (MirrorBEventDispatcher)BEventManager.Instance.BEventDispatcher;
            if (mirrorBEventDispatcher)
            {
                mirrorBEventDispatcher.OnRefreshConnectedNetworkIDs(assignedNetworkID);
            }
        }

        [ClientRpc]
        public void Rpc_OnClientDisconnected(ENetworkID networkID)
        {
            MirrorBEventDispatcher mirrorBEventDispatcher = (MirrorBEventDispatcher)BEventManager.Instance.BEventDispatcher;
            if (mirrorBEventDispatcher)
            {
                mirrorBEventDispatcher.OnClientDisconnected(networkID);
            }
        }
        #endregion

        #region BEvents Dispatching
        [Client]
        public void Client_RequestBroadcastEvent(AbstractBEHandle bEHandle, BEventReplicationType eventInvocationType, ENetworkID callingNetworkID, ENetworkID targetNetworkID)
        {
            //Debug.Log("Client_RequestBroadcastEvent : " + callingNetworkID + " - " + eventInvocationType);

            string serializedBEHandle = BUtils.SerializeObject(bEHandle);


            Cmd_OnRequestBroadcastEvent(serializedBEHandle, eventInvocationType, callingNetworkID, targetNetworkID);
        }

        [Command]
        private void Cmd_OnRequestBroadcastEvent(string serializedBEHandle, BEventReplicationType eventInvocationType, ENetworkID callingNetworkID, ENetworkID targetNetworkID)
        {
            //Debug.Log("Cmd_OnRequestBroadcastEvent : " + callingNetworkID + " - " + eventInvocationType);

            Server_OnBroadcastEvent(serializedBEHandle, eventInvocationType, callingNetworkID, targetNetworkID);
        }

        [Server]
        public void Server_RequestBroadcastEvent(AbstractBEHandle bEHandle, BEventReplicationType eventInvocationType, ENetworkID callingNetworkID, ENetworkID targetNetworkID)
        {
            //Debug.Log("Server_RequestBroadcastEvent : " + callingNetworkID + " - " + eventInvocationType);

            string serializedBEHandle = BUtils.SerializeObject(bEHandle);


            Server_OnBroadcastEvent(serializedBEHandle, eventInvocationType, callingNetworkID, targetNetworkID);
        }

        [Server]
        private void Server_OnBroadcastEvent(string serializedBEHandle, BEventReplicationType eventInvocationType, ENetworkID callingNetworkID, ENetworkID targetNetworkID)
        {
            //Debug.Log("Server_OnBroadcastEvent : " + callingNetworkID + " - " + eventInvocationType);

            if (eventInvocationType == BEventReplicationType.TO_TARGET)
            {
                MirrorBEventDispatcher mirrorBEventDispatcher = (MirrorBEventDispatcher)BEventManager.Instance.BEventDispatcher;
                if (mirrorBEventDispatcher)
                {
                    NetworkIdentity targetNetworkIdentity = mirrorBEventDispatcher.GetNetworkIdentity(targetNetworkID);
                    Target_OnBroadcastEvent(targetNetworkIdentity.connectionToClient, serializedBEHandle, callingNetworkID, targetNetworkID);
                }
            }
            else // ALL or ALL_OTHERS
            {
                Rpc_OnBroadcastEvent(serializedBEHandle, callingNetworkID);
            }
        }

        [ClientRpc]
        private void Rpc_OnBroadcastEvent(string serializedBEHandle, ENetworkID callingNetworkID)
        {
            //Debug.Log("Rpc_OnBroadcastEvent : " + callingNetworkID);

            MirrorBEventDispatcher mirrorBEventDispatcher = (MirrorBEventDispatcher)BEventManager.Instance.BEventDispatcher;
            if ((mirrorBEventDispatcher)
                && (mirrorBEventDispatcher.LocalNetworkID != callingNetworkID))
            {
                BEventManager.Instance.OnBEventReplicated(serializedBEHandle);
            }
        }
                
        [TargetRpc]
        private void Target_OnBroadcastEvent(NetworkConnection networkConnection, string serializedBEHandle, ENetworkID callingNetworkID, ENetworkID targetNetworkID)
        {
            //Debug.Log("Target_OnBroadcastEvent : " + callingNetworkID);

            MirrorBEventDispatcher mirrorBEventDispatcher = (MirrorBEventDispatcher)BEventManager.Instance.BEventDispatcher;
            if ((mirrorBEventDispatcher)
                && (mirrorBEventDispatcher.LocalNetworkID != callingNetworkID))
            {
                if (targetNetworkID != BEventManager.Instance.LocalNetworkID)
                {
                    Debug.LogError("Recieved a target rpc but the local networkID is not the same. Target : " + targetNetworkID + " - Local : " + BEventManager.Instance.LocalNetworkID);
                }

                BEventManager.Instance.OnBEventReplicated(serializedBEHandle);
            }
        }
        #endregion

        #region Test
        [Command]
        public void Cmd_TestInteger(int integer)
        {
            Debug.Log("Cmd_TestInteger : " + integer + " - " + BUtils.GetTimeAsString());
            Rpc_TestInteger(integer);
        }

        [ClientRpc]
        public void Rpc_TestInteger(int integer)
        {
            Debug.Log("Rpc_TestInteger : " + integer + " - " + BUtils.GetTimeAsString());
        }
        #endregion
    }
}
