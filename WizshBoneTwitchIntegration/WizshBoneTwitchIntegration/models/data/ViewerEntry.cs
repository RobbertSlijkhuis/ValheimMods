using System.Collections.Generic;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Models
{
    internal class ViewerEntry
    {
        public string name;
        public string color1;
        //public string color2;
        public Color parsedColor1;
        //public Color parsedColor2;
        public List<string> effects = new List<string>();

        public ViewerEntry() { }

        public void Init()
        {
            ColorUtility.TryParseHtmlString(color1, out parsedColor1);
            //ColorUtility.TryParseHtmlString(color2, out parsedColor2);
        }
    }
}
