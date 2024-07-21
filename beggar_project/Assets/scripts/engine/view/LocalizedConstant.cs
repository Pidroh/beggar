//using UnityEngine.U2D;
using TMPro;
using UnityEngine;
using static HeartUnity.Local;

namespace HeartUnity.View
{
    public class LocalizedConstant : MonoBehaviour 
    {
        public string key;
        private LanguageSet lastLang;
        public TextMeshProUGUI text;

        public void Update()
        {
            if (Local.Instance.Lang != lastLang) 
            {
                if (text == null) text = GetComponent<TextMeshProUGUI>();
                lastLang = Local.Instance.Lang;
                text.text = Local.GetText(key);
            }
        }

        public static void UtilityPingAll() {
            var aus = Resources.FindObjectsOfTypeAll<LocalizedConstant>();
            foreach (var lc in aus)
            {
                lc.Update();
            }
        }
    }
}
