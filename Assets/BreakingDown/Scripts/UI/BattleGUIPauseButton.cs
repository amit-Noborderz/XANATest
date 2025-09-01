using UnityEngine;

// Controls the visibility of the pause button in the battle GUI.
// Only visible in offline mode.
namespace BD
{
    public class BattleGUIPauseButton : MonoBehaviour
    {
        private void Start()
        {
            if (UFE.gameMode == UFE3D.GameMode.NetworkGame)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }

        }
    }
}