using System;
using System.Threading.Tasks;
using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SessionManager : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] GameObject sessionPanel;

    [SerializeField] Button hostGameButton;
    [SerializeField] Button joinWithCodeButton;
    [SerializeField] Button quitApplicationButton;
    [SerializeField] Button viewSessionsButton;
    [SerializeField] Button copyCodeButton;

    [SerializeField] TextMeshProUGUI codeText;
    [SerializeField] TMP_InputField inputCode;

    [SerializeField] LobbySync LobbySyncInstance;

    // Session lifecycle events other systems can observe
    public event Action<ISession> OnSessionStarted;
    public event Action OnSessionEnded;

    private ISession _session;
    public bool IsHost => _session?.IsHost ?? (NetworkManager.Singleton && NetworkManager.Singleton.IsHost);

    void Awake()
    {
        SetupButtonListeners();
        ShowMainPanel();
    }

    private void SetupButtonListeners()
    {
        hostGameButton.onClick.AddListener(() => _ = CreateSessionAsHost());
        joinWithCodeButton.onClick.AddListener(() => _ = JoinSessionByCode(inputCode.text));
        viewSessionsButton.onClick.AddListener(ShowSessions);
        quitApplicationButton.onClick.AddListener(() => _ = QuitAsync());
        copyCodeButton.onClick.AddListener(CopyCodeToClipboard);
    }

    private async Task PreMultiplayer()
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("Unity Services initialized and signed in.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SessionManager] Init failed: {e}");
            throw;
        }
    }

    async Task CreateSessionAsHost()
    {
        await PreMultiplayer();

        try
        {
            string displayName = string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName)
                ? AuthenticationService.Instance.PlayerId.Substring(0, 6)
                : AuthenticationService.Instance.PlayerName;

            SessionOptions options = new SessionOptions
            {
                MaxPlayers = 4,
                IsPrivate = false,
                Name = $"{displayName}'s Lobby"
            }
            .WithRelayNetwork();

            _session = await MultiplayerService.Instance.CreateSessionAsync(options);

            codeText.text = "Code: " + _session.Code;

            ShowLobby();

            OnSessionStarted?.Invoke(_session);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SessionManager] Create session failed: {e}");
        }
    }

    private async Task JoinSessionByCode(string code)
    {
        await PreMultiplayer();

        try
        {
            _session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);

            ShowLobby();

            OnSessionStarted?.Invoke(_session);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SessionManager] Join session failed: {e}");
        }
    }

    public async Task DisbandSession()
    {
        try
        {
            if (_session == null || !_session.IsHost) return;

            await _session.AsHost().DeleteAsync();

            if (LobbySyncInstance) LobbySyncInstance.ClearLobbyRpc();

            if (NetworkManager.Singleton && NetworkManager.Singleton.IsHost)
                NetworkManager.Singleton.Shutdown();

            _session = null;

            ShowMainPanel();

            OnSessionEnded?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SessionManager] DisbandSession failed: {e}");
        }
    }

    public async Task LeaveSession()
    {
        try
        {
            if (_session == null) return;

            await _session.LeaveAsync();

            if (NetworkManager.Singleton && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost))
                NetworkManager.Singleton.Shutdown();

            _session = null;

            Debug.Log("Successfully did session.LeaveSync()");

            ShowMainPanel();

            OnSessionEnded?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SessionManager] LeaveSession failed: {e}");
        }
    }

    private void CopyCodeToClipboard()
    {
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);

        string code = _session?.Code;
        if (string.IsNullOrEmpty(code) && codeText != null && !string.IsNullOrEmpty(codeText.text))
        {
            var txt = codeText.text.Trim();
            const string prefix = "Code:";
            if (txt.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                code = txt.Substring(prefix.Length).Trim();
            else
                code = txt.Trim();
        }

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("[SessionManager] No session code to copy.");
            return;
        }

        GUIUtility.systemCopyBuffer = code;
        if (codeText) StartCoroutine(FlashCopied(code));
    }

    private IEnumerator FlashCopied(string code)
    {
        string prev = codeText.text;
        codeText.text = $"Code: {code}\n<color=#6aff6a>Copied!</color>";
        yield return new WaitForSeconds(1f);
        codeText.text = $"Code: {code}";
    }

    public void ShowLobby()
    {
        mainPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        sessionPanel.SetActive(false);
    }

    public void ShowSessions()
    {
        mainPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        sessionPanel.SetActive(true);
    }

    public void ShowMainPanel()
    {
        lobbyPanel?.SetActive(false);
        sessionPanel?.SetActive(false);
        mainPanel?.SetActive(true);
    }

    private async Task QuitAsync()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    try
    {
        if (_session != null)
        {
            if (IsHost)
                await _session.AsHost().DeleteAsync();
            else
                await _session.LeaveAsync();
        }
    }
    catch (Exception e)
    {
        Debug.LogWarning($"[SessionManager] Quit cleanup warning: {e}");
    }
    finally
    {
        if (NetworkManager.Singleton)
            NetworkManager.Singleton.Shutdown();

        Application.Quit();
    }
#endif
    }

    void OnEnable()
    {
        if (NetworkManager.Singleton)
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    void OnDisable()
    {
        if (NetworkManager.Singleton)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton && !IsHost && clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("[SessionManager] Disconnected from host, returning to main menu.");
            ShowMainPanel();
            OnSessionEnded?.Invoke();
        }
    }
}
