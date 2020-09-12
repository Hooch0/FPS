using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    
    public static UIManager Instance { get; private set; }

    public GameObject InteractionPanel;

    public TextMeshProUGUI InteractionText;

    public TextMeshProUGUI CurrentAmmo;
    public TextMeshProUGUI ReserveAmmo;

    private void OnEnable()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }

    private void Start()
    {
        PlayerController.Instance.PlayerInteractable += OnPlayerInteractable;

    }
    
    private void Update()
    {
        UpdateWeaponUI();
    }

    private void UpdateWeaponUI()
    {
        Inventory inventory = PlayerController.Instance.InventorySystem;

        if (inventory.CurrentWeapon != null)
        {
            CurrentAmmo.text = inventory.CurrentWeapon.CurrentAmmo.ToString();
            ReserveAmmo.text = inventory.Ammo.GetAmmo(inventory.CurrentWeapon.Data.AmmoType).CurrentReserveAmmo.ToString();
        }
        else
        {
            CurrentAmmo.text = "--";
            ReserveAmmo.text = "--";
        }
    }


    public void OnPlayerInteractable(string message)
    {
        if (string.IsNullOrEmpty(message) == true)
        {
            InteractionPanel.SetActive(false);
            return;
        }

        InteractionPanel.SetActive(true);
        InteractionText.text = message;
    }
    
}
//Goal: have UIManager monitor the current weapon equiped and get its "Current Ammo" and the ammo bags "Current Reserve Ammo"