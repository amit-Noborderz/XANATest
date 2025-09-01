using ControlFreak2;
using UnityEngine;
using UnityEngine.UI;

namespace BD
{
    public class BDCanvasButtonsHandler : MonoBehaviour
    {
        public RectTransform joystick, LK, LP, HP, HK, SP, B, Crouch, Jump;
        public Image[] buttonImgs;
        public TouchButtonSpriteAnimator[] buttonImgs2;
        public static BDCanvasButtonsHandler inst;

        private void Awake()
        {
            if (inst != null && inst != this)
            {
                Destroy(this);
            }
            else
            {
                inst = this;
            }
        }
    }
}