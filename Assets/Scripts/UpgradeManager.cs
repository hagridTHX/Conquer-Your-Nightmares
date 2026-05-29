using UnityEngine;
using System.Collections.Generic;
using System.Linq; 

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private bool disableUpgradesForTesting = false; 
    [SerializeField] private List<UpgradeData> allPossibleUpgrades;
    [SerializeField] private List<UpgradeEntry> activeUpgrades = new List<UpgradeEntry>();
    [SerializeField] private RarityTier[] rarityTiers;

    public bool DisableUpgradesForTesting { get => disableUpgradesForTesting; set => disableUpgradesForTesting = value; }
    public List<UpgradeEntry> ActiveUpgrades { get => activeUpgrades; set => activeUpgrades = value; }
    public List<UpgradeData> AllPossibleUpgrades { get => allPossibleUpgrades; set => allPossibleUpgrades = value; }
    public RarityTier[] RarityTiers { get => rarityTiers; set => rarityTiers = value; }

    [System.Serializable]
    public class UpgradeEntry
    {
        public UpgradeData data;
        public int currentLevel;
    }

    [System.Serializable]
    public class RarityTier
    {
        public string rarityName;       
        public Sprite rarityIcon;       
        public float multiplier = 1f;   
        public int dropWeight = 100;
        public Color rarityColor = Color.white; 
    }

    public class UpgradeOption
    {
        public UpgradeData BaseData;
        public RarityTier Tier;
    }

    public void AddUpgrade(UpgradeOption option)
    {
        UpgradeEntry entry = activeUpgrades.Find(x => x.data == option.BaseData);

        if (entry == null)
        {
            entry = new UpgradeEntry { data = option.BaseData, currentLevel = 1 };
            activeUpgrades.Add(entry);
        }
        else
        {
            entry.currentLevel++;
        }

        PlayerStats stats = GetComponent<PlayerStats>();
        PlayerController controller = GetComponent<PlayerController>();
        Weapon currentWeapon = GetComponentInChildren<Weapon>(); 

        option.BaseData.ApplyUpgrade(controller, stats, currentWeapon, option.Tier.multiplier);
    }

    public bool CanLevelUp(UpgradeData upgrade)
    {
        if (upgrade.MaxLevel <= 0) return true; 
        UpgradeEntry entry = activeUpgrades.Find(x => x.data == upgrade);
        if (entry == null) return true; 
        return entry.currentLevel < upgrade.MaxLevel;
    }

    private RarityTier GetRandomRarity()
    {
        if (rarityTiers == null || rarityTiers.Length == 0) return new RarityTier();

        int totalWeight = 0;
        foreach (var tier in rarityTiers) totalWeight += tier.dropWeight;

        int randomVal = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var tier in rarityTiers)
        {
            currentWeight += tier.dropWeight;
            if (randomVal < currentWeight) return tier;
        }
        return rarityTiers[0]; 
    }

    public List<UpgradeOption> GetRandomUpgrades(int count)
    {
        List<UpgradeData> available = allPossibleUpgrades.Where(u => CanLevelUp(u)).ToList();
        
        for (int i = 0; i < available.Count; i++)
        {
            UpgradeData temp = available[i];
            int randomIndex = Random.Range(i, available.Count);
            available[i] = available[randomIndex];
            available[randomIndex] = temp;
        }

        List<UpgradeData> drawnData = available.Take(count).ToList();
        List<UpgradeOption> drawnOptions = new List<UpgradeOption>();

        foreach (var data in drawnData)
        {
            drawnOptions.Add(new UpgradeOption { BaseData = data, Tier = GetRandomRarity() });
        }

        return drawnOptions;
    }
}