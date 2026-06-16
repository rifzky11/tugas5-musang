using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VR01_P5_Latihan3 : MonoBehaviour
{

    public int MaxAmmo, CurrentAmmo;
    public TextMeshProUGUI TextAmmo;
    public Button ButtonShoot, ButtonReload;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentAmmo = MaxAmmo; 
        TextAmmo.text = CurrentAmmo + "/" + MaxAmmo;

        ButtonShoot.onClick.AddListener(Shoot);
        ButtonReload.onClick.AddListener(Reload);
    }

    public void Shoot()
    {
        CurrentAmmo -= 1;
        TextAmmo.text = CurrentAmmo + "/" + MaxAmmo;

        if (CurrentAmmo == 0)
        {
            ButtonShoot.interactable = false;

        }
    }

    public void Reload()
    {
        CurrentAmmo = MaxAmmo;
        TextAmmo.text = CurrentAmmo + "/" + MaxAmmo;
        ButtonShoot.interactable = true;
    }
}
