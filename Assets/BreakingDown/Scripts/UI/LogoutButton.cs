using UnityEngine;
using UnityEngine.SceneManagement;

namespace BD
{
    public class LogoutButton : MonoBehaviour
    {
        public async void LogOut()
        {
            GetComponent<UnityEngine.UI.Button>().interactable = false;

            await APIManager.LogOutAsync();
            Debug.Log("Logged out");
            SceneManager.LoadScene(0); // Go to login screen
        }
    }
}