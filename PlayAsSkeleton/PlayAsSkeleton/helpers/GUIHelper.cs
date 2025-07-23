using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;

namespace ModularMagic_Utilities.Helpers
{
    internal class GUIHelper
    {
        public static GameObject CreateSliderField(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position,
            float minValue, float maxValue, float defaultValue, bool wholeNumbers, float width = 0f, float height = 0f)
        {
            GameObject sliderField = DefaultControls.CreateSlider(GUIManager.Instance.ValheimControlResources);
            sliderField.transform.SetParent(parent, false);
            Slider sliderComponent = sliderField.GetComponent<Slider>();
            GUIManager.Instance.ApplySliderStyle(sliderComponent);
            sliderComponent.minValue = minValue;
            sliderComponent.maxValue = maxValue;
            sliderComponent.value = defaultValue;
            sliderComponent.wholeNumbers = wholeNumbers;

            // Set positions and anchors
            RectTransform tf = sliderField.transform as RectTransform;
            tf.anchoredPosition = position;
            tf.anchorMin = anchorMin;
            tf.anchorMax = anchorMax;

            // Margmas: You need the transform component. Maybe even RectTransform, so ((RectTransform)sliderField.transform)
            //// Optionally set width and height
            //if (width > 0f)
            //{
            //    sliderField.SetWidth(width);
            //}

            //if (height > 0f)
            //{
            //    sliderField.SetHeight(height);
            //}

            return sliderField;
        }

        public static GameObject CreateToggle(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position,
           float width = 0f, float height = 0f)
        {
            GameObject checkboxField = GUIManager.Instance.CreateToggle(
                parent: parent,
                width: width,
                height: height
            );

            Toggle toggle = checkboxField.GetComponent<Toggle>();
            RectTransform tf = toggle.GetComponent<RectTransform>();
            tf.anchoredPosition = position;
            tf.anchorMin = anchorMin;
            tf.anchorMax = anchorMax;

            return checkboxField;
        }
    }
}
