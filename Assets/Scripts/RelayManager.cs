using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using Unity.Netcode;

public class SessionManager : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject hostPanel;
    [SerializeField] GameObject joinPanel;

    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] Button quitButton;

    [SerializeField] TextMeshProUGUI codeText;
    [SerializeField] TMP_InputField inputCode;

    private ISession _session;
    void Awake()
    {
        // Always set up listeners early
        hostButton.onClick.AddListener(() => _ = CreateSessionAsHost());
        joinButton.onClick.AddListener(() => _ = JoinSessionByCode(inputCode.text));
        quitButton.onClick.AddListener(Quit);

        // Hide panels before render
        hostPanel?.SetActive(false);
        joinPanel?.SetActive(false);
        mainPanel?.SetActive(true);
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



    private async Task CreateSessionAsHost()
    {
        await PreMultiplayer();

        try
        {
            DisableMainPanel();
            hostPanel?.SetActive(true);

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

            // Now the underlying transport should already be configured by the session API
            // You can just start host
            NetworkManager.Singleton.StartHost();
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
            DisableMainPanel();
            joinPanel?.SetActive(true);

            _session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);

            // Session API should have internally configured transport
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SessionManager] Join session failed: {e}");
        }
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void DisableMainPanel() => mainPanel?.SetActive(false);
}
