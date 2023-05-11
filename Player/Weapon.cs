using UnityEngine;

public class Weapon : MonoBehaviour {
    private BoxCollider boxCollider;

    public GameObject hitParticle;
    public float weaponDamage = 35;

    void Awake() {
        boxCollider = GetComponent<BoxCollider>();
    }

    private void Start() {
        boxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log(other.tag);
        if (other.CompareTag("Enemy")) {
            PlayerManager.instance.DealDamage(other.gameObject, weaponDamage);
            Instantiate(hitParticle, other.transform.position + Vector3.up * 0.6f + other.transform.forward * 0.3f, other.transform.rotation);
        }
    }

    public void EnableColliders() {
        boxCollider.enabled = true;
    }

    public void DisableColliders() {
        boxCollider.enabled = false;
    }
}
