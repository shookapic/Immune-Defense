using UnityEngine;
using UnityEngine.UI;
using GameSystems;

namespace UI
{
    /// <summary>
    /// Button that automatically plays a click sound using AudioController when pressed.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class AudioButton : Button
    {
        protected override void Start()
        {
            base.Start();
            onClick.AddListener(PlayClickSound);
        }

        private void PlayClickSound()
        {
            if (AudioController.Instance != null)
                AudioController.Instance.PlayButtonClick();
        }
    }
}
