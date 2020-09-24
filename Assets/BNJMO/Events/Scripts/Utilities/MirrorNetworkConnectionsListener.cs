using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BNJMO;
using Mirror;
using Mirror.Discovery;

[DisallowMultipleComponent]
[AddComponentMenu("Network/NetworkConnectionsListener")]
[RequireComponent(typeof(NetworkDiscovery))]
public class MirrorNetworkConnectionsListener : AbstractSingletonManager<MirrorNetworkConnectionsListener>
{
    public event System.Action<long[]> DiscoveredServersUpdated;

    public readonly Dictionary<long, ServerResponse> DiscoveredServers = new Dictionary<long, ServerResponse>();
    Vector2 scrollViewPos = Vector2.zero;

    public NetworkDiscovery networkDiscovery;

    [SerializeField] private bool drawHUD = true;
    

    private Dictionary<long, int> discoveryTimeouts = new Dictionary<long, int>();
    private int counter = 0;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (networkDiscovery == null)
        {
            networkDiscovery = GetComponent<NetworkDiscovery>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
            UnityEditor.Undo.RecordObjects(new Object[] { this, networkDiscovery }, "Set NetworkDiscovery");
        }
    }
#endif
    protected override void OnGUI()
    {
        base.OnGUI();

        if (NetworkManager.singleton == null)
            return;

        if (NetworkServer.active || NetworkClient.active)
            return;

        if (!NetworkClient.isConnected && !NetworkServer.active && !NetworkClient.active)
            DrawGUI();
    }

    private void DrawGUI()
    {
        if (drawHUD == true)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Find Servers"))
            {
                FindServers();
            }

            // LAN Host
            if (GUILayout.Button("Start Host"))
            {
                StartHost();
            }

            // Dedicated server
            if (GUILayout.Button("Start Server"))
            {
                StartServer();
            }

            GUILayout.EndHorizontal();

            // show list of found server

            GUILayout.Label($"Discovered Servers [{DiscoveredServers.Count}]:");

            // servers
            scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);

            foreach (ServerResponse info in DiscoveredServers.Values)
                if (GUILayout.Button(info.EndPoint.Address.ToString()))
                    Connect(info);

            GUILayout.EndScrollView();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        BEventsCollection.NETWORK_NetworkStateUpdated += On_NETWORK_NetworkStateUpdated;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        BEventsCollection.NETWORK_NetworkStateUpdated -= On_NETWORK_NetworkStateUpdated;
    }

    protected override void Start()
    {
        base.Start();

        FindServers();
    }

    protected override void Update()
    {
        base.Update();

        LogCanvas("NetworkDiscovery", "NetworkDiscovery : " + DiscoveredServers.Count);
    }

    public void FindServers()
    {
        networkDiscovery.StartDiscovery();
    }

    public void StartServer()
    {
        if (NetworkClient.isConnected == false
            && NetworkServer.active == false
            && NetworkClient.active == false)
        {
            ReinitializeDiscovery();
            NetworkManager.singleton.StartServer();
            networkDiscovery.AdvertiseServer();
        }
        else
        {
            LogConsoleWarning("Trying to start a new server but this client is already connected");
        }
    }

    public void StartHost()
    {
        if (NetworkClient.isConnected == false
            && NetworkServer.active == false
            && NetworkClient.active == false)
        {
            ReinitializeDiscovery();
            NetworkManager.singleton.StartHost();
            networkDiscovery.AdvertiseServer();
        }
        else
        {
            LogConsoleWarning("Trying to start a new host but this client is already connected");
        }
    }

    public void Disconnect()
    {
        if (NetworkServer.active == true
            || NetworkClient.isConnected == true)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            LogConsoleWarning("Trying to disconnect but not connected anywhere.");
        }
    }


    public bool ConnectToFirstServer()
    {
        if (DiscoveredServers.Count > 0)
        {
            foreach (ServerResponse info in DiscoveredServers.Values)
            {
                LogConsole("Connecting to first server found!");
                Connect(info);
                return true;
            }
        }

        return false;
    }


    public void OnDiscoveredServer(ServerResponse info)
    {
        if (BEventManager.Instance.NetworkState == ENetworkState.NOT_CONNECTED)
        {
            if (DiscoveredServers.ContainsKey(info.serverId) == false)
            {
                DiscoveredServers.Add(info.serverId, info);
                discoveryTimeouts.Add(info.serverId, 0);
                InvokeEventIfBound(DiscoveredServersUpdated, DiscoveredServers.Keys.ToArray());
            }

            // Timeout coroutine
            StartCoroutine(DiscoveryTimeoutCoroutine(info.serverId));
        }
    }

    private IEnumerator DiscoveryTimeoutCoroutine(long serverId)
    {
        int myCounter = counter++;
        discoveryTimeouts[serverId] = myCounter;

        yield return new WaitForSeconds(networkDiscovery.ActiveDiscoveryInterval + 1.0f);

        // No other coroutine updated the counter 
        if (discoveryTimeouts.ContainsKey(serverId)
            && discoveryTimeouts[serverId] == myCounter)
        {
            discoveryTimeouts.Remove(serverId);
            DiscoveredServers.Remove(serverId);
            InvokeEventIfBound(DiscoveredServersUpdated, DiscoveredServers.Keys.ToArray());
        }
    }

    private void Connect(ServerResponse info)
    {
        NetworkManager.singleton.StartClient(info.uri);
    }


    private void On_NETWORK_NetworkStateUpdated(BEHandle<ENetworkState> handle)
    {
        if (handle.Arg1 != ENetworkState.NOT_CONNECTED)
        {
            ReinitializeDiscovery();
            InvokeEventIfBound(DiscoveredServersUpdated, DiscoveredServers.Keys.ToArray());
        }
        else
        {
            FindServers();
        }
    }

    private void ReinitializeDiscovery()
    {
        DiscoveredServers.Clear();
        discoveryTimeouts.Clear();
        StopAllCoroutines();
    }

}