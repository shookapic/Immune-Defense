using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Tooltip("Drag your button prefab here. The prefab must contain a Button component (or a child with one).")]
    public GameObject buttonPrefab;

    [Tooltip("Optional: parent transform (e.g. a VerticalLayoutGroup). If null, the instantiated object will be placed at scene root.")]
    public Transform buttonsParent;

    [Tooltip("Name of the scene to load when pressing Play")]
    public string gameSceneName = "Game";

    [Tooltip("Name of the scene to load when pressing Deck Editor")]
    public string deckSceneName = "DeckEditor";

    void Start()
    {
        if (buttonPrefab == null)
        {
            Debug.LogError("MainMenuController: buttonPrefab is not assigned.");
            return;
        }

        CreateButton("Play", () => LoadSceneSafe(gameSceneName));
        CreateButton("Deck Editor", () => LoadSceneSafe(deckSceneName));
        CreateButton("Quit", () => QuitApplication());
    }

    void CreateButton(string label, Action onClickAction)
    {
        GameObject go = null;
        if (buttonsParent != null)
            go = Instantiate(buttonPrefab, buttonsParent) as GameObject;
        else
            go = Instantiate(buttonPrefab) as GameObject;

        if (go == null)
        {
            Debug.LogError("Failed to instantiate button prefab.");
            return;
        }

        go.name = "MenuButton - " + label;

        // Try to set TextMeshProUGUI.text via reflection to avoid compile-time dependency
        bool textSet = false;
        var tmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
        if (tmpType != null)
        {
            var tmpComp = go.GetComponentInChildren(tmpType as Type) as UnityEngine.Component;
            if (tmpComp != null)
            {
                var prop = tmpType.GetProperty("text");
                if (prop != null)
                {
                    prop.SetValue(tmpComp, label, null);
                    textSet = true;
                }
            }
        }

        if (!textSet)
        {
            var uiText = go.GetComponentInChildren<Text>();
            if (uiText != null)
            {
                uiText.text = label;
                textSet = true;
            }
        }

        if (!textSet)
            Debug.LogWarning("MainMenuController: Could not find a Text or TextMeshProUGUI component in the button prefab to set its label.");

        // Find Button component and set callback
        var button = go.GetComponent<Button>();
        if (button == null)
            button = go.GetComponentInChildren<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickAction());
        }
        else
        {
            Debug.LogWarning("MainMenuController: No Button component found in the assigned prefab. Cannot hook onClick.");
        }
    }

    void LoadSceneSafe(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("MainMenuController: sceneName is empty. Assign a valid scene name in the inspector.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    void QuitApplication()
    {
#if UNITY_EDITOR
        // In the editor stop play mode
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
