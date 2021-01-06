using UnityEngine;
using VRTK;

namespace MyDataVisualizer
{

    public class MenuToggle : MonoBehaviour
    {
        public VRTK_ControllerEvents controllerEvents;
        public GameObject menu;
        bool menuState = false;

        void OnEnable() 
        {
            controllerEvents.ButtonTwoReleased += toggleMenu;
        }

        void OnDisable()
        {
            controllerEvents.ButtonTwoReleased -= toggleMenu;
        }

        private void toggleMenu(object sender, ControllerInteractionEventArgs eventArgs)
        {
            menuState = !menuState;
            menu.SetActive(menuState);
        }
    }
}
