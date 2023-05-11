using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class PlayerManager : MonoBehaviour {
    public static PlayerManager instance;

    public HealthBar healthBar;

    public bool isAlive = true;

    public float attack = 0f;
    public float armor = 1f;
    public float critDamage = 1.5f;
    public float critRate = 30f;
    public float maxHealth = 100f;
    public float currentHealth;


    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);

        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    public void DealDamage(GameObject target, float weaponDamage) {
        EnemiesManager enemyManager = target.GetComponent<EnemiesManager>();

        if (enemyManager != null) {
            float totalDamage = attack + weaponDamage;

            bool isCritical = Random.Range(0, 100) < critRate;

            if (isCritical) totalDamage *= critDamage;

            float absoluteDamage = Mathf.Abs(totalDamage);

            enemyManager.TakeDamage(absoluteDamage);

            Popup.CreateDamagePopup(target.transform.position + Vector3.up * 1f, absoluteDamage, isCritical);
        }

    }


    public void TakeDamage(float damage) {
        if (!isAlive) return;

        float absoluteDamage = Mathf.Abs(damage - armor);

        currentHealth -= absoluteDamage;

        Player.instance.BeHit();

        Popup.CreateDamagePopup(transform.position + Vector3.up * 1.8f, absoluteDamage, false);

        healthBar.SetHealth(currentHealth);

        PlayerController.instance.ResetCombo("NormalAttack");

        if (currentHealth <= 0) {
            Death();
        }
    }

    public void TakeHeal(float amount) {
        if (!isAlive) return;

        float absoluteHealth = Mathf.Abs(amount);

        currentHealth += absoluteHealth;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Popup.CreateHealthPopup(transform.position + Vector3.up * 1.8f, absoluteHealth);

        healthBar.SetHealth(currentHealth);
    }

    public void Revive() {
        Player.instance.ResetPosition();

        currentHealth = maxHealth;

        healthBar.SetHealth(currentHealth);

        isAlive = true;
    }

    public void Death() {
        Player.instance.Death();

        currentHealth = 0;
        isAlive = false;

        GameManager.instance.GameOver();
    }
}
