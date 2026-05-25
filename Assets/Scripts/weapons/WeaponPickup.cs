using UnityEngine;
using System.Collections.Generic;

public class WeaponPickup : MonoBehaviour
{
    [Tooltip("All weapon pickup objects available at the start.")]
    [SerializeField] private List<GameObject> allPickupObjects;

    [Tooltip("Player-held weapon instance to activate (e.g., Weapon_Sword, Weapon_Axe).")]
    [SerializeField] private Weapon playerWeaponToEquip;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.Instance.currentState == GameManager.GameState.WeaponSelection)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            
            if (player != null && playerWeaponToEquip != null)
            {
                player.EquipWeapon(playerWeaponToEquip);
                GameManager.Instance.StartRun();

                foreach (GameObject pickup in allPickupObjects)
                {
                    if (pickup != null) Destroy(pickup);
                }
            }
        }
    }
}