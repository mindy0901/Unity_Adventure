using UnityEngine;
using TMPro;

public class Popup : MonoBehaviour {
    public static Popup CreateDamagePopup(Vector3 position, float damage, bool isCriticalDamage) {
        Transform damagePopupTransform = Instantiate(GameAssets.i.damagePopupPrefab, position, Quaternion.identity);
        Popup damagePopup = damagePopupTransform.GetComponent<Popup>();
        damagePopup.SetupDamagePopup(damage, isCriticalDamage);

        return damagePopup;
    }

    public static Popup CreateHealthPopup(Vector3 position, float health) {
        Transform healthPopupTransform = Instantiate(GameAssets.i.damagePopupPrefab, position, Quaternion.identity);
        Popup healthPopup = healthPopupTransform.GetComponent<Popup>();
        healthPopup.SetupHealthPopup(health);

        return healthPopup;
    }

    private TextMeshPro textMesh;
    private Color currentTextColor;

    [SerializeField] float disappearTimer = 0.5f;
    [SerializeField] float moveYSpeed = 2f;
    [SerializeField] float disappearSpeed = 1f;

    [SerializeField] float healthDisappearTimer = 1f;
    [SerializeField] float healthMoveYSpeed = 1f;
    [SerializeField] float healthDisappearSpeed = 2f;

    void Awake() {
        textMesh = GetComponent<TextMeshPro>();

    }

    private void Update() {
        if (textMesh != null) {
            transform.LookAt(Camera.main.transform);

            transform.position += new Vector3(0, moveYSpeed) * Time.deltaTime;

            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

            if (disappearTimer > disappearTimer * 0.5f) {
                float increaseScaleAmount = 1f;
                transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;

            } else {
                float decreaseScaleAmount = 1f;
                transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;

            }

            disappearTimer -= Time.deltaTime;

            if (disappearTimer < 0) {
                currentTextColor.a -= disappearSpeed * Time.deltaTime;
                textMesh.color = currentTextColor;

                if (currentTextColor.a < 0) {
                    Destroy(gameObject);
                }
            }
        }
    }

    public void SetupDamagePopup(float amount, bool isCriticalDamage) {
        textMesh.text = amount.ToString();

        if (isCriticalDamage) {
            textMesh.fontSize = 4;
            currentTextColor = Color.red;

        } else {
            textMesh.fontSize = 3;
            currentTextColor = Color.white;
        }

        textMesh.color = currentTextColor;
    }

    public void SetupHealthPopup(float amount) {
        textMesh.text = amount.ToString();

        textMesh.fontSize = 3;

        currentTextColor = Color.green;

        textMesh.color = currentTextColor;

        disappearTimer = healthDisappearTimer;
        moveYSpeed = healthMoveYSpeed;
        disappearSpeed = healthDisappearSpeed;
    }
}
