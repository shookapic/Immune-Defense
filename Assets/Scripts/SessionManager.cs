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


// TODO:
// 1. Client should disconnect and change back to main menu, when hosts disbands
// 2. Make PlayerCards appear
// 3. Cleanup code 


public class SessionManager : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] GameObject sessionPanel;

    [SerializeField] Button hostGameButton;
    [SerializeField] Button joinWithCodeButton;
    [SerializeField] Button quitApplicationButton;
    [SerializeField] Button leaveSessionButton;
    [SerializeField] Button disbandSessionButton;
    [SerializeField] Button viewSessionsButton;
    [SerializeField] Button copyCodeButton;

    [SerializeField] TextMeshProUGUI codeText;
    [SerializeField] TMP_InputField inputCode;

    [SerializeField] LobbySync LobbySyncInstance;

    private ISession _session;
    public bool IsHost => _session?.IsHost ?? (NetworkManager.Singleton && NetworkManager.Singleton.IsHost);
    void Awake()
    {
        // Always set up listeners early
        hostGameButton.onClick.AddListener(() => _ = CreateSessionAsHost());
        joinWithCodeButton.onClick.AddListener(() => _ = JoinSessionByCode(inputCode.text));
        viewSessionsButton.onClick.AddListener(ShowSessions);
        quitApplicationButton.onClick.AddListener(() => _ = QuitAsync());
        copyCodeButton.onClick.AddListener(CopyCodeToClipboard);

        ShowMainPanel();
        
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
            throw; // propagate so caller knows init failed
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
            .WithRelayNetwork();    // pick Relay networking

            _session = await MultiplayerService.Instance.CreateSessionAsync(options);

            string joinCode = _session.Code;
            codeText.text = "Code: " + joinCode;

            ShowLobby();
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
            JoinSessionOptions options = new JoinSessionOptions
            {

            };

            _session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);

            ShowLobby();
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

            if (LobbySyncInstance) LobbySyncInstance.ClearLobbyRpc(); // BEFORE shutdown

            if (NetworkManager.Singleton && NetworkManager.Singleton.IsHost)
                NetworkManager.Singleton.Shutdown();

            _session = null;

            ShowMainPanel();
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
        }
        catch (Exception e)
        {
            Debug.LogError($"[SessionManager] LeaveSession failed: {e}");
        }
    }

    private void CopyCodeToClipboard()
    {
        // Deselect the button so it doesn’t stay highlighted
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);

        // Prefer the live session’s code
        string code = _session?.Code;

        // Fallback: try to parse from the label if you formatted it as "Code: XYZ123"
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

        // Optional: quick visual confirmation
        if (codeText) StartCoroutine(FlashCopied(code));
    }

    private IEnumerator FlashCopied(string code)
    {
        string prev = codeText.text;
        codeText.text = $"Code: {code}\n<color=#6aff6a>Copied!</color>";
        yield return new WaitForSeconds(1f);
        codeText.text = $"Code: {code}";
    }

    private void ShowLobby()
    {
        mainPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        sessionPanel.SetActive(false);
    }

    private void ShowSessions()
    {
        mainPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        sessionPanel.SetActive(true);
    }

    private void ShowMainPanel()
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

}
