using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class PlayerNetworkManager : AbstractSingletonManager<PlayerNetworkManager>
    {
        #region Public Events


        #endregion

        #region Public Methods


        #endregion

        #region Inspector Variables


        #endregion

        #region Private Variables
        private ENetworkID localNetworkID = ENetworkID.NONE;
        //private EPlayerID localPlayerID = EPlayerID.NONE;
        private Dictionary<EControllerID, EPlayerID> partyMap = new Dictionary<EControllerID, EPlayerID>();

        #endregion

        #region Life Cycle
        protected override void OnEnable()
        {
            base.OnEnable();

            BEventsCollection.NETWORK_NetworkStateUpdated.Event += On_NETWORK_NetworkStateUpdated;
            BEventsCollection.NETWORK_NewNetworkIDConnected.Event += On_NETWORK_NewNetworkIDConnected;
            BEventsCollection.NETWORK_NetworkIDDisconnected.Event += On_NETWORK_NetworkIDDisconnected;
            BEventsCollection.NETWORK_PlayerJoined.Event += On_NETWORK_PlayerJoined;
            BEventsCollection.NETWORK_PlayerLeft.Event += On_NETWORK_PlayerLeft;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            BEventsCollection.NETWORK_NetworkStateUpdated.Event -= On_NETWORK_NetworkStateUpdated;
            BEventsCollection.NETWORK_NewNetworkIDConnected.Event -= On_NETWORK_NewNetworkIDConnected;
            BEventsCollection.NETWORK_NetworkIDDisconnected.Event -= On_NETWORK_NetworkIDDisconnected;
            BEventsCollection.NETWORK_PlayerJoined.Event -= On_NETWORK_PlayerJoined;
            BEventsCollection.NETWORK_PlayerLeft.Event -= On_NETWORK_PlayerLeft;
        }

        #endregion

        #region Events Callbacks

        private void On_NETWORK_NetworkStateUpdated(BEHandle<ENetworkState> handle)
        {
            // Connected ?
            if (handle.Arg1 == ENetworkState.CLIENT
                || handle.Arg1 == ENetworkState.HOST)
            {
                localNetworkID = BEventManager.Instance.LocalNetworkID;

                //EControllerID controllerID = BUtils.GetControllerIDFrom(localNetworkID);

                //if (localNetworkID == ENetworkID.HOST)
                //{
                //    // Connect Controller
                //    InputManager.Instance.ConnectController(controllerID);

                //    // Join Player
                //    localPlayerID = PlayerManager.Instance.OnNextFreePlayerJoinRequest(controllerID); // TODO : Move to OnButtonPressed

                //    if (IS_NOT_NONE(localPlayerID))
                //    {
                //        partyMap.Add(controllerID, localPlayerID);
                //    }
                //}
            }
            else // Disconnected
            {
                // Disconnect all connected net controllers
                foreach (EControllerID controllerID in partyMap.Keys)
                {
                    InputManager.Instance.DisconnectController(controllerID);
                }

                // Reinitialize 
                localNetworkID = ENetworkID.NONE;
                //localPlayerID = EPlayerID.NONE;
                partyMap.Clear();
            }
        }

        private void On_NETWORK_NewNetworkIDConnected(BEHandle<ENetworkID> handle)
        {
            if (BEventManager.Instance.LocalNetworkID == ENetworkID.HOST)
            {
                EControllerID newControllerID = BUtils.GetControllerIDFrom(handle.Arg1);
                if (IS_NOT_NONE(newControllerID)
                    && InputManager.Instance.IsControllerConnected(newControllerID) == false)
                {
                    StartCoroutine(JoinConnectedPlayerCoroutine(handle, newControllerID));
                }
            }
        }

        private IEnumerator JoinConnectedPlayerCoroutine(BEHandle<ENetworkID> handle, EControllerID newControllerID)
        {
            // Wait a delay
            yield return new WaitForSeconds(1.0f);

            // Connect Controller
            InputManager.Instance.ConnectController(newControllerID);

            // Join Player
            EPlayerID newPlayerID = PlayerManager.Instance.JoinPlayer(newControllerID); // TODO : Move to OnButtonPressed
            if (IS_NOT_NONE(newPlayerID))
            {
                // Spawn Player
                AbstractNetPlayer newPlayer = (AbstractNetPlayer)PlayerManager.Instance.SpawnPlayer(newPlayerID);
                if (IS_NOT_NULL(newPlayer))
                {
                    newPlayer.SetOwner(handle.Arg1);
                }

                // Update map
                partyMap.Add(newControllerID, newPlayerID);

                // Replicate all connected net controllers to all other clients (Refresh)
                foreach (ENetworkID networkIDitr in BEventManager.Instance.GetConnectedNetworkIDs())
                {
                    if (networkIDitr != localNetworkID) // != Host
                    {
                        foreach (KeyValuePair<EControllerID, EPlayerID> pair in partyMap)
                        {
                            BEventsCollection.NETWORK_PlayerJoined.Invoke(new BEHandle<EPlayerID, EControllerID>(pair.Value, pair.Key), BEventReplicationType.TO_TARGET, true, networkIDitr);
                        }
                    }
                }
            }
        }

        private void On_NETWORK_NetworkIDDisconnected(BEHandle<ENetworkID> handle)
        {
            if (BEventManager.Instance.LocalNetworkID == ENetworkID.HOST)
            {
                EControllerID controllerID = BUtils.GetControllerIDFrom(handle.Arg1);
                if (IS_NOT_NONE(controllerID)
                    && IS_TRUE(InputManager.Instance.IsControllerConnected(controllerID))
                    && IS_KEY_CONTAINED(partyMap, controllerID))
                {
                    EPlayerID playerID = partyMap[controllerID];
                    if (IS_NOT_NONE(playerID))
                    {
                        partyMap.Remove(controllerID);

                        // Disconnect Controller (-> Remove from party)
                        InputManager.Instance.DisconnectController(controllerID);

                        BEventsCollection.NETWORK_PlayerLeft.Invoke(new BEHandle<EPlayerID, EControllerID>(playerID, controllerID), BEventReplicationType.TO_ALL_OTHERS, true);
                    }
                }
            }
        }

        /* On Clients */
        private void On_NETWORK_PlayerJoined(BEHandle<EPlayerID, EControllerID> handle)
        {
            if (ARE_NOT_EQUAL(localNetworkID, handle.InvokingNetworkID)
                && ARE_NOT_EQUAL(localNetworkID, ENetworkID.HOST)
                && InputManager.Instance.IsControllerConnected(handle.Arg2) == false)
            {
                // Connect Controller
                InputManager.Instance.ConnectController(handle.Arg2);

                // Update Party Map
                if (IS_KEY_NOT_CONTAINED(partyMap, handle.Arg2))
                {
                    partyMap.Add(handle.Arg2, handle.Arg1);
                }

                // Join Player
                IS_TRUE(PlayerManager.Instance.JoinPlayer(handle.Arg1, handle.Arg2));

                // Spawn Player
                ENetworkID networkID = BUtils.GetNetworkIDFrom(handle.Arg2);
                AbstractNetPlayer newPlayer = (AbstractNetPlayer)PlayerManager.Instance.SpawnPlayer(handle.Arg1);
                if (IS_NOT_NULL(newPlayer)
                    && IS_NOT_NONE(networkID))
                {
                    newPlayer.SetOwner(networkID);
                }
            }
        }

        private void On_NETWORK_PlayerLeft(BEHandle<EPlayerID, EControllerID> handle)
        {
            if (ARE_NOT_EQUAL(localNetworkID, handle.InvokingNetworkID)
                && ARE_NOT_EQUAL(localNetworkID, ENetworkID.HOST)
                && InputManager.Instance.IsControllerConnected(handle.Arg2) == true)
            {
                // Connect Controller
                InputManager.Instance.DisconnectController(handle.Arg2);

                // Update Party Map
                if (IS_KEY_CONTAINED(partyMap, handle.Arg2))
                {
                    partyMap.Remove(handle.Arg2);
                }
            }
        }

        #endregion

        #region Others


        #endregion
    }
}
