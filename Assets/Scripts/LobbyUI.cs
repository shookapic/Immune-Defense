using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] LobbySync lobbySync;
    [SerializeField] RectTransform[] slots;
    [SerializeField] GameObject cardPrefab;

    [SerializeField] Button DisbandButton;
    [SerializeField] Button LeaveButton;
    [SerializeField] Button StartButton;

    private SessionManager _sessionManager;
    private readonly Dictionary<ulong, GameObject> _cards = new();

    private void Awake()
    {
        _sessionManager = FindFirstObjectByType<SessionManager>();
        if (_sessionManager == null)
        {
            Debug.LogError("[LobbyUI] SessionManager not found in scene.");
            enabled = false;
            return;
        }

        // These need to be PUBLIC methods on SessionManager (or use public wrappers)
        DisbandButton.onClick.AddListener(() => _ = _sessionManager.DisbandSession());
        LeaveButton.onClick.AddListener(() => _ = _sessionManager.LeaveSession());
        StartButton.onClick.AddListener(StartGame);
    }

    private void OnEnable()
    {
        RefreshHostControls();

        if (!lobbySync) lobbySync = FindFirstObjectByType<LobbySync>();
        if (lobbySync && lobbySync.IsSpawned)
        {
            lobbySync.Players.OnListChanged += OnPlayersChanged;
            RebuildAll();
        }
        else
        {
            StartCoroutine(WaitAndBind());
        }
    }

    private void OnDisable()
    {
        if (lobbySync)
            lobbySync.Players.OnListChanged -= OnPlayersChanged;
    }

    private void RefreshHostControls()
    {
        bool isHost = NetworkManager.Singleton && NetworkManager.Singleton.IsHost;
        DisbandButton.enabled = isHost;
        StartButton.enabled = isHost;
        LeaveButton.enabled = !isHost;
    }

    private IEnumerator WaitAndBind()
    {
        while (!lobbySync || !lobbySync.IsSpawned)
        {
            lobbySync = lobbySync ? lobbySync : FindFirstObjectByType<LobbySync>();
            yield return null;
        }

        lobbySync.Players.OnListChanged += OnPlayersChanged;
        RebuildAll();
        RefreshHostControls();
    }

    private void OnPlayersChanged(NetworkListEvent<PlayerCardData> e)
    {
        // For a small lobby, full rebuild is fine
        RebuildAll();
    }

    private void RebuildAll()
    {
        foreach (var go in _cards.Values) Destroy(go);
        _cards.Clear();

        int i = 0;
        foreach (var p in lobbySync.Players)
        {
            if (i >= slots.Length) break;

            var slot = slots[i++];
            var go = Instantiate(cardPrefab, slot, false);
            _cards[p.ClientId] = go;

            // Stretch to slot (optional)
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            // Bind fields (add null checks if needed)
            var nameTr = go.transform.Find("Name");
            var pingTr = go.transform.Find("Ping");
            var colorTr = go.transform.Find("Color");

            var nameText = nameTr ? nameTr.GetComponent<TMP_Text>() : null;
            var pingText = pingTr ? pingTr.GetComponent<TMP_Text>() : null;
            var colorImg = colorTr ? colorTr.GetComponent<Image>() : null;

            if (nameText) nameText.text = p.Name.ToString();
            if (pingText) pingText.text = $"{p.PingMs} ms";
            if (colorImg) colorImg.color = ColorFromIndex(p.ColorIndex);
        }
    }

    private void StartGame()
    {
        // host-only action
        if (!(NetworkManager.Singleton && NetworkManager.Singleton.IsHost))
            return;

        // TODO: trigger your game start here (e.g., load scene / signal ready).
        Debug.Log("[LobbyUI] StartGame clicked by host");
    }

    private static Color ColorFromIndex(int idx)
    {
        Color[] palette = {
            new(0.93f,0.26f,0.26f),
            new(0.96f,0.67f,0.14f),
            new(0.97f,0.85f,0.16f),
            new(0.30f,0.85f,0.39f),
            new(0.20f,0.60f,0.98f),
            new(0.49f,0.35f,0.98f),
            new(0.94f,0.33f,0.69f),
            new(0.50f,0.78f,0.80f),
        };
        return palette[Mathf.Abs(idx) % palette.Length];
    }
}
