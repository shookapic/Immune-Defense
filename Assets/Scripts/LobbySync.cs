using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class LobbySync : NetworkBehaviour
{
    // Forwarded event so UI and other systems can observe list changes without touching NetworkList directly.
    public event Action<NetworkListEvent<PlayerCardData>> OnPlayersChanged;

    public NetworkList<PlayerCardData> Players { get; private set; } =
        new NetworkList<PlayerCardData>(writePerm: NetworkVariableWritePermission.Server);

    private UnityTransport _transport;

    public override void OnNetworkSpawn()
    {
        // forward NetworkList changes
        Players.OnListChanged += HandlePlayersListChanged;

        _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;

            var hostId = NetworkManager.ServerClientId;
            AddOrUpdatePlayer(hostId, GetLocalDisplayName(), ColorFromId(hostId));

            InvokeRepeating(nameof(UpdatePings), 0.5f, 1.0f);
        }

        // Queue name RPC from clients once spawned
        if (IsClient && !IsServer)
            SubmitNameRpc(GetLocalDisplayName());
    }


    public override void OnNetworkDespawn()
    {
        Players.OnListChanged -= HandlePlayersListChanged;

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            CancelInvoke(nameof(UpdatePings));
        }
    }

    private void HandlePlayersListChanged(NetworkListEvent<PlayerCardData> evt)
    {
        OnPlayersChanged?.Invoke(evt);
    }

    private void OnClientConnected(ulong clientId)
    {
        AddOrUpdatePlayer(clientId, MakeDefaultName(clientId), ColorFromId(clientId));
    }

    private void OnClientDisconnected(ulong clientId)
    {
        var idx = IndexOfClient(clientId);
        if (idx >= 0) Players.RemoveAt(idx);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SubmitNameRpc(string name, RpcParams rpcParams = default)
    {
        var senderId = rpcParams.Receive.SenderClientId;
        var finalName = string.IsNullOrWhiteSpace(name) ? MakeDefaultName(senderId) : name;
        AddOrUpdatePlayer(senderId, finalName, ColorFromId(senderId));
    }

    // Server-only helper on LobbySync:
    [Rpc(SendTo.Server)]
    public void ClearLobbyRpc()
    {
        if (!IsServer) return;
        Players.Clear();
    }

    private void AddOrUpdatePlayer(ulong clientId, string name, int colorIndex)
    {
        var idx = IndexOfClient(clientId);
        var data = new PlayerCardData { ClientId = clientId, Name = name, ColorIndex = colorIndex, PingMs = 0 };
        if (idx >= 0) { Players[idx] = data; Debug.Log($"[LobbySync] Updated {clientId} -> {name}"); }
        else { Players.Add(data); Debug.Log($"[LobbySync] Added {clientId} -> {name}"); }
    }

    private int IndexOfClient(ulong clientId)
    {
        for (int i = 0; i < Players.Count; i++)
            if (Players[i].ClientId == clientId) return i;
        return -1;
    }

    private void UpdatePings()
    {
        if (_transport == null) return;

        // Host RTT is always 0 from its own perspective
        for (int i = 0; i < Players.Count; i++)
        {
            var p = Players[i];
            short ping = 0;

            if (p.ClientId != NetworkManager.ServerClientId)
            {
                // Unity Transport 2.x: GetCurrentRtt returns milliseconds
                ping = (short)Mathf.Clamp(_transport.GetCurrentRtt(p.ClientId), 0, short.MaxValue);
            }

            if (p.PingMs != ping)
            {
                p.PingMs = ping;
                Players[i] = p;
            }
        }
    }

    // ===== Helpers =====
    private static string MakeDefaultName(ulong clientId) => $"Player {clientId}";
    private static string GetLocalDisplayName()
    {
        try
        {
            var name = Unity.Services.Authentication.AuthenticationService.Instance.PlayerName;
            if (!string.IsNullOrWhiteSpace(name)) return name;
            var pid = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
            return string.IsNullOrEmpty(pid) ? "Player" : $"P{pid[..Mathf.Min(6, pid.Length)]}";
        }
        catch { return "Player"; }
    }

    // Deterministic color index from clientId (0..7)
    private static int ColorFromId(ulong clientId) => (int)(clientId % 8);
}