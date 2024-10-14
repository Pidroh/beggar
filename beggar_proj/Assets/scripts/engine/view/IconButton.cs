//using UnityEngine.U2D;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HeartUnity.View
{

    public class IconButton : UIUnit
    {
        public Image icon;

        public void ChangeSprite(Sprite sprite)
        {
            icon.sprite = sprite;
            if(sprite != null)
                icon.rectTransform.sizeDelta = new Vector2(icon.sprite.rect.width, icon.sprite.rect.height);
        }
    }
}