//using UnityEngine.U2D;

using System;

namespace HeartUnity.View
{
    public class ButtonBinding
    {
        public int button;
        public int key;

        public ButtonBinding(int key, int button)
        {
            this.button = button;
            this.key = key;
        }

        public ButtonBinding(int key, DefaultButtons button) : this(key: key, button: (int)button) { }
        public ButtonBinding(char key, DefaultButtons button) : this(key: key, button: (int)button) { }

        public ButtonBinding(int key, Enum binding) : this(key: key, button: Convert.ToInt32(binding))
        {
        }
    }
}