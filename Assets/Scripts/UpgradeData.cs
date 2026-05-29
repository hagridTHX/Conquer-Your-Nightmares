using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "CYN/Upgrade Card")]
public class UpgradeData : ScriptableObject
{
    [SerializeField] private string upgradeName;
    [TextArea] [SerializeField] private string description;
    
    [SerializeField] private int maxLevel = 0; 

    [SerializeField] private float addMaxHealth = 0f;
    [SerializeField] private float moveSpeedMultiplier = 0f;

    [SerializeField] private float damageMultBonus = 0f;
    [SerializeField] private float swingSpeedMultBonus = 0f;

    [SerializeField] private float massInertiaChange = 0f;

    public string UpgradeName { get => upgradeName; set => upgradeName = value; }
    public string Description { get => description; set => description = value; }
    public int MaxLevel { get => maxLevel; set => maxLevel = value; }
    public float AddMaxHealth { get => addMaxHealth; set => addMaxHealth = value; }
    public float MoveSpeedMultiplier { get => moveSpeedMultiplier; set => moveSpeedMultiplier = value; }
    public float DamageMultBonus { get => damageMultBonus; set => damageMultBonus = value; }
    public float SwingSpeedMultBonus { get => swingSpeedMultBonus; set => swingSpeedMultBonus = value; }
    public float MassInertiaChange { get => massInertiaChange; set => massInertiaChange = value; }

    public string GetFormattedDescription(float rarityMultiplier)
    {
        float mainStat = 0f;

        if (AddMaxHealth != 0) mainStat = AddMaxHealth;
        else if (MoveSpeedMultiplier != 0) mainStat = MoveSpeedMultiplier;
        else if (DamageMultBonus != 0) mainStat = DamageMultBonus;
        else if (SwingSpeedMultBonus != 0) mainStat = SwingSpeedMultBonus;
        else if (MassInertiaChange != 0) mainStat = MassInertiaChange;

        float finalValue = mainStat * rarityMultiplier;

        if (Description.Contains("{0}"))
        {
            return Description.Replace("{0}", finalValue.ToString("0.##"));
        }
        
        return Description;
    }

    public virtual void ApplyUpgrade(PlayerController player, PlayerStats stats, Weapon weapon, float rarityMultiplier)
    {
        if (AddMaxHealth > 0)
        {
            float finalHealthBonus = AddMaxHealth * rarityMultiplier;
            stats.maxHealth += finalHealthBonus;
            stats.currentHealth += finalHealthBonus;
            stats.OnHealthChanged.Invoke(stats.currentHealth, stats.maxHealth);
        }

        if (weapon != null)
        {
            weapon.damageMultiplier += (DamageMultBonus * rarityMultiplier);
            weapon.speedMultiplier += (SwingSpeedMultBonus * rarityMultiplier);

            if (MassInertiaChange != 0 && weapon is Sword sword)
            {
                sword.MassInertia += (MassInertiaChange * rarityMultiplier); 
            }
        }
    }
}