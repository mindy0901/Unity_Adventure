using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour {
    public Text pointsText;

    public void Active(int score) {
        gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;

        pointsText.text = score.ToString() + " POINTS";
    }

    public void Restart() {
        gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;

        PlayerManager.instance.Revive();
    }
}
