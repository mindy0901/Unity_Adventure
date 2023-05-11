using UnityEngine;

public class EnemiesManager : MonoBehaviour {
    private Enemy enemy;

    [SerializeField] bool isAlive = true;
    [SerializeField] float attackDamage = 10f;
    [SerializeField] float maxHealth = 1000f;
    [SerializeField] float currentHealth;
    [SerializeField] float respawnTime = 30f;
    [SerializeField] GameObject deathParticle;

    private void Awake() {
        enemy = GetComponent<Enemy>();
    }

    private void Start() {
        currentHealth = maxHealth;
    }

    public void DealDamage() {
        PlayerManager.instance.TakeDamage(attackDamage);
    }

    public void TakeDamage(float damage) {
        if (!isAlive) return;

        currentHealth -= damage;

        enemy.UpdateEnemyState(EnemyState.Damage);

        if (currentHealth <= 0) {
            Kill();
            PlayerManager.instance.TakeHeal(maxHealth * 5 / 100);
            GameManager.instance.IncreaseScore();
        }
    }

    public void Kill() {
        string enemyName = enemy.name.Replace("(Clone)", "");

        currentHealth = 0;
        isAlive = false;
        AudioManager.instance.PlayRandomSFX($"{enemyName} Death");
        Instantiate(deathParticle, transform.up * 2f, transform.rotation);

        gameObject.SetActive(false);

        Invoke(nameof(RespawnEnemy), respawnTime);
    }

    private void RespawnEnemy() {
        currentHealth = maxHealth;
        isAlive = true;
        transform.position = enemy.enemyOriginPosition;
        enemy.slimeStunning = false;

        gameObject.SetActive(true);
    }
}
