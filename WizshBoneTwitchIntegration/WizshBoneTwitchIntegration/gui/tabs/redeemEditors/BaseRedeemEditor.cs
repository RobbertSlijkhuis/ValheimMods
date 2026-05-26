using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace WizshBoneTwitchIntegration.Gui
{
    internal abstract class BaseRedeemEditor : IRedeemEditor
    {
        protected const float RowHeight  = 28f;
        protected const float RowSpacing = 6f;
        protected const float LabelWidth = 200f;
        protected const float InputWidth = 160f;

        public abstract void BuildUI(GameObject parent);
        public abstract void ApplyTo(Models.RedeemData target);
        public abstract void Reset();

        protected InputField AddRow(GameObject parent, string label, ref float y)
        {
            float centreY = y - RowHeight / 2f;

            GUIManager.Instance.CreateText(
                text: label,
                parent: parent.transform,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(0f, 1f),
                position: new Vector2(LabelWidth / 2f, centreY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 12,
                color: GUIManager.Instance.ValheimBeige,
                outline: true,
                outlineColor: Color.black,
                width: LabelWidth,
                height: RowHeight,
                addContentSizeFitter: false
            );

            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(parent.transform, false);

            RectTransform rt = inputObj.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0f, 1f);
            rt.anchorMax        = new Vector2(0f, 1f);
            rt.pivot            = new Vector2(0f, 0.5f);
            rt.sizeDelta        = new Vector2(InputWidth, RowHeight);
            rt.anchoredPosition = new Vector2(LabelWidth + 10f, centreY);

            inputObj.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            InputField inputField = inputObj.AddComponent<InputField>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(5f, 2f);
            textRt.offsetMax = new Vector2(-5f, -2f);

            Text text = textObj.AddComponent<Text>();
            text.font            = GUIManager.Instance.AveriaSerifBold;
            text.fontSize        = 12;
            text.color           = Color.white;
            text.supportRichText = false;

            inputField.textComponent = text;
            inputField.text          = "";
            inputObj.SetActive(true);

            y -= RowHeight + RowSpacing;
            return inputField;
        }

        protected void AddNote(GameObject parent, string message, ref float y)
        {
            float centreY = y - RowHeight / 2f;

            GUIManager.Instance.CreateText(
                text: message,
                parent: parent.transform,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(0f, 1f),
                position: new Vector2(LabelWidth / 2f, centreY),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 12,
                color: GUIManager.Instance.ValheimYellow,
                outline: true,
                outlineColor: Color.black,
                width: LabelWidth + InputWidth + 10f,
                height: RowHeight,
                addContentSizeFitter: false
            );

            y -= RowHeight + RowSpacing;
        }

        protected static string GetStringOr(InputField field, string fallback)
        {
            string v = field.text.Trim();
            return v.Length > 0 ? v : fallback;
        }

        protected static float GetFloatOr(InputField field, float fallback)
            => float.TryParse(field.text, out float v) ? v : fallback;

        protected static int GetIntOr(InputField field, int fallback)
            => int.TryParse(field.text, out int v) ? v : fallback;

        protected static bool GetBool(InputField field)
            => field.text.Trim().ToLower() == "true";
    }
}