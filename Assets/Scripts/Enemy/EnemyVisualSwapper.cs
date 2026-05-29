using UnityEngine;

public class EnemyVisualSwapper : MonoBehaviour
{
    [SerializeField] private GameObject[] visualPrefabs;

    public GameObject[] VisualPrefabs { get => visualPrefabs; set => visualPrefabs = value; }

    private void Awake()
    {
        if (visualPrefabs == null || visualPrefabs.Length == 0) return;

        GameObject selectedPrefab = visualPrefabs[Random.Range(0, visualPrefabs.Length)];
        GameObject spawnedModel = Instantiate(selectedPrefab, transform);
        
        spawnedModel.transform.localPosition = Vector3.zero;
        
        spawnedModel.transform.localRotation = selectedPrefab.transform.localRotation;
        spawnedModel.transform.localScale = selectedPrefab.transform.localScale;
    }
}