using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    [SerializeField] Menu menu;

    private int score;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);

        } else {
            Destroy(gameObject);
        }

    }

    public void GameOver() {
        menu.Active(score);
    }

    public void IncreaseScore() {
        score++;
    }
}
