using UnityEngine;
using System.Collections.Generic;
using System.Linq; 

public class UpgradeManager : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("When enabled, the level-up card panel is suppressed.")]
    public bool disableUpgradesForTesting = false; 

    [Header("All Upgrades Pool")]
    [Tooltip("Assign all upgrade card assets from the Project.")]
    public List<UpgradeData> allPossibleUpgrades;

    [Header("Active Upgrades (Read-Only)")]
    public List<UpgradeEntry> activeUpgrades = new List<UpgradeEntry>();

    [System.Serializable]
    public class UpgradeEntry
    {
        public UpgradeData data;
        public int currentLevel;
    }

    public void AddUpgrade(UpgradeData upgrade)
    {
        UpgradeEntry entry = activeUpgrades.Find(x => x.data == upgrade);

        if (entry == null)
        {
            entry = new UpgradeEntry { data = upgrade, currentLevel = 1 };
            activeUpgrades.Add(entry);
        }
        else
        {
            entry.currentLevel++;
        }

        PlayerStats stats = GetComponent<PlayerStats>();
        PlayerController controller = GetComponent<PlayerController>();
        
        Weapon currentWeapon = GetComponentInChildren<Weapon>(); 

        upgrade.ApplyUpgrade(controller, stats, currentWeapon);
    }

    public bool CanLevelUp(UpgradeData upgrade)
    {
        if (upgrade.maxLevel <= 0) return true; 
        UpgradeEntry entry = activeUpgrades.Find(x => x.data == upgrade);
        if (entry == null) return true; 
        return entry.currentLevel < upgrade.maxLevel;
    }

    public List<UpgradeData> GetRandomUpgrades(int count)
    {
        List<UpgradeData> available = allPossibleUpgrades.Where(u => CanLevelUp(u)).ToList();
        
        for (int i = 0; i < available.Count; i++)
        {
            UpgradeData temp = available[i];
            int randomIndex = Random.Range(i, available.Count);
            available[i] = available[randomIndex];
            available[randomIndex] = temp;
        }

        return available.Take(count).ToList();
    }
}