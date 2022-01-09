using UnityEngine;
using Utility;

namespace Managers
{
    public class KeybindManager : MonoSingleton<KeybindManager>
    {
        public KeyCode moveLeft,moveRight,moveDown,moveUp;
        public KeyCode shootPrimary, shootSecondary;
        public KeyCode sprint, dash;
        public KeyCode zoomIn, zoomOut;

        private void OnEnable()
        {
            moveLeft = KeyCode.A;
            moveRight = KeyCode.D;
            moveDown = KeyCode.S;
            moveUp = KeyCode.W;
            shootPrimary = KeyCode.Mouse0;
            shootSecondary = KeyCode.Mouse1;
            sprint = KeyCode.LeftShift;
            dash = KeyCode.Space;
        }
        
    }
}
