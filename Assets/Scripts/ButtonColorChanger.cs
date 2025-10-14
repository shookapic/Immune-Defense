using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonColorChanger : Button
{
    private TextMeshProUGUI _buttonText;

    protected override void Awake()
    {
        base.Awake();
        _buttonText = GetComponentInChildren<TextMeshProUGUI>();

        // Set up button colors
        var cb = colors;
        cb.normalColor = Color.blue;
        cb.highlightedColor = Color.cyan;
        cb.pressedColor = Color.magenta;
        cb.selectedColor = Color.yellow;
        cb.disabledColor = Color.gray;
        colors = cb;

        // Set up text appearance
        if (_buttonText != null)
        {
            _buttonText.color = Color.white;
            //_buttonText.fontSize = 24;
            _buttonText.fontStyle = FontStyles.Bold;
        }
    }
}
