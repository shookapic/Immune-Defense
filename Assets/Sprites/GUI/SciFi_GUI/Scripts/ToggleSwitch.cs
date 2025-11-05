using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KwaaktjeUI {
    public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
    {
        [Header("Slider")] 
        [SerializeField, Range(0, 1f)]
        private float sliderValue;
        private Slider _slider;

        [Header("Colors and Sprites")] 
        [SerializeField] private Image background;
        [SerializeField] private Image toggle;
        [SerializeField] private Sprite backgroundSpriteTurnedOn;
        [SerializeField] private Color backgroundColorTurnedOn;
        [SerializeField] private Sprite toggleSpriteTurnedOn;
        [SerializeField] private Color toggleColorTurnedOn;
        [SerializeField] private Sprite backgroundSpriteTurnedOff;
        [SerializeField] private Color backgroundColorTurnedOff;
        [SerializeField] private Sprite toggleSpriteTurnedOff;
        [SerializeField] private Color toggleColorTurnedOff;


        protected void OnValidate()
        {
            SetupToggleComponents();

            _slider.value = sliderValue;
        }

        private void SetupToggleComponents()
        {
            if (_slider != null)
                return;

            SetupSliderComponent();
        }

        private void SetupSliderComponent()
        {
            _slider = GetComponent<Slider>();

            if (_slider == null)
            {
                Debug.Log("No slider found!", this);
                return;
            }

            _slider.interactable = false;
            var sliderColors = _slider.colors;
            sliderColors.disabledColor = Color.white;
            _slider.colors = sliderColors;
            _slider.transition = Selectable.Transition.None;

            // AdjustVisuals();
        }

        private void AdjustVisuals()
        {
            if (IsOn())
            {
                if (background != null){
                    background.sprite = backgroundSpriteTurnedOn;
                    background.color = backgroundColorTurnedOn;
                }
                if (toggle != null){
                    toggle.sprite = toggleSpriteTurnedOn;
                    toggle.color = toggleColorTurnedOn;
                }
            }
            else
            {
                if (background != null){
                    background.sprite = backgroundSpriteTurnedOff;
                    background.color = backgroundColorTurnedOff;
                }
                if (toggle != null){
                    toggle.sprite = toggleSpriteTurnedOff;
                    toggle.color = toggleColorTurnedOff;
                }
            }
        }

        protected void Awake()
        {
            SetupSliderComponent();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Toggle();
        }

        
        private void Toggle()
        {
            if (IsOn())
            {
                _slider.value = 0;
            } else {
                _slider.value = 1;
            }   
            sliderValue = _slider.value;
            AdjustVisuals();
        }

        private bool IsOn()
        {
            return _slider.value > 0.5;
        }
    }
}
