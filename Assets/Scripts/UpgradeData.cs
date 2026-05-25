using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "CYN/Upgrade Card")]
public class UpgradeData : ScriptableObject
{
    [Header("UI Presentation")]
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Limits")]
    [Tooltip("How many times this upgrade can be selected; 0 means unlimited.")]
    public int maxLevel = 0; 

    [Header("Player Stats")]
    public float addMaxHealth = 0f;
    public float moveSpeedMultiplier = 0f;

    [Header("Weapon Stats (All Weapons)")]
    public float damageMultBonus = 0f;
    public float swingSpeedMultBonus = 0f;

    [Header("Weapon-Specific Stats (e.g., Sword)")]
    public float massInertiaChange = 0f;

    public virtual void ApplyUpgrade(PlayerController player, PlayerStats stats, Weapon weapon)
    {
        if (addMaxHealth > 0)
        {
            stats.maxHealth += addMaxHealth;
            stats.currentHealth += addMaxHealth;
            stats.OnHealthChanged.Invoke(stats.currentHealth, stats.maxHealth);
        }

        if (weapon != null)
        {
            weapon.damageMultiplier += damageMultBonus;
            weapon.speedMultiplier += swingSpeedMultBonus;

            if (massInertiaChange != 0 && weapon is Sword sword)
            {
                sword.MassInertia += massInertiaChange; 
            }
        }
    }
}