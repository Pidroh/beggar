//using UnityEngine.U2D;
using UnityEngine;
using UnityEngine.UI;

namespace HeartUnity.View
{



    public class IconLabel : UIUnit
    {
        public Image icon;
        public void ChangeSprite(Sprite sprite)
        {
            icon.sprite = sprite;
            icon.rectTransform.sizeDelta = new Vector2(icon.sprite.rect.width, icon.sprite.rect.height);
        }
    }
}
