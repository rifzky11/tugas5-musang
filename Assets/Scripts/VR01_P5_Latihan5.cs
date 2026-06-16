using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VR01_P5_Latihan5 : MonoBehaviour
{

    public TextMeshProUGUI HPText;
    public Button DamageButton, HealButton;
    public Image Overlay;

    private int currentHP;
    public int maxHP = 100;
    public int DamageAmount, HealAmount;
    public bool isTakingDamage;

    public void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        currentHP = maxHP;
        Overlay.color = new Color(1f, 1f, 1f, 0);
        DamageButton.onClick.AddListener(TakeDamage);
    }

    public void TakeDamage()
    {
        currentHP -= DamageAmount;
        Overlay.color = new Color(1f, 0, 0, 0.5f);
        isTakingDamage = true;
    }

    private void ResetOverlay()
    {
        Overlay.color = new Color(1f, 1f, 1f, 0);
    }

    public void Update()
    {
        if (isTakingDamage)
        {
            float timer = 1f;
            while (timer > 0)
            {
                timer -= 1f;
            }

            if (timer <= 0)
            {
                ResetOverlay();
                isTakingDamage = false;
            }
        }
    }

}
