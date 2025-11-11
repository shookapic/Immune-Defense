using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to a GameObject in the DeckCreator scene.
/// It will instantiate a button from a prefab and wire it to load the Main Menu scene.
/// </summary>
public class DeckCreatorController : MonoBehaviour
{
    [Tooltip("Drag the button prefab to instantiate the Back button from.")]
    public GameObject buttonPrefab;

    [Tooltip("Optional parent for the instantiated button (RectTransform). If null, button is instantiated at scene root.")]
    public Transform buttonsParent;

    [Tooltip("Name of the main menu scene to load when returning")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("Label for the back button")]
    public string buttonLabel = "Back";

    [Tooltip("If true and parent is a RectTransform, reset anchored position/scale to defaults after instantiation.")]
    public bool resetRectTransform = true;

    void Start()
    {
        if (buttonPrefab == null)
        {
            Debug.LogError("DeckCreatorController: buttonPrefab is not assigned.");
            return;
        }

        CreateBackButton();
    }

    void CreateBackButton()
    {
        GameObject go = null;
        if (buttonsParent != null)
            go = Instantiate(buttonPrefab, buttonsParent) as GameObject;
        else
            go = Instantiate(buttonPrefab) as GameObject;

        if (go == null)
        {
            Debug.LogError("DeckCreatorController: failed to instantiate button prefab.");
            return;
        }

        go.name = "BackButton";

        // If parent is RectTransform and requested, reset transform to defaults so layout works predictably
        if (resetRectTransform && buttonsParent != null)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one;
                rt.anchoredPosition = Vector2.zero;
                rt.localRotation = Quaternion.identity;
            }
        }

        // Try TextMeshPro first (reflection) then fallback to UnityEngine.UI.Text
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
                    prop.SetValue(tmpComp, buttonLabel, null);
                    textSet = true;
                }
            }
        }

        if (!textSet)
        {
            var uiText = go.GetComponentInChildren<Text>();
            if (uiText != null)
            {
                uiText.text = buttonLabel;
                textSet = true;
            }
        }

        if (!textSet)
            Debug.LogWarning("DeckCreatorController: Could not find a Text or TextMeshProUGUI component in the button prefab to set its label.");

        // Hook up the Button callback
        var button = go.GetComponent<Button>();
        if (button == null)
            button = go.GetComponentInChildren<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnBackPressed());
        }
        else
        {
            Debug.LogWarning("DeckCreatorController: No Button component found in button prefab. Cannot hook OnClick.");
        }
    }

    void OnBackPressed()
    {
        Debug.Log("DeckCreatorController: Back button pressed.");
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogWarning("DeckCreatorController: mainMenuSceneName is empty. Set a valid scene name.");
            return;
        }
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
