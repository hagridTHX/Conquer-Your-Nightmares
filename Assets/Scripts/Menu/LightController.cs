using UnityEngine;

public class LightControl : MonoBehaviour
{
    [SerializeField] private float timeToLightning = 3f;
    [SerializeField] GameObject LightningEffect;
    void Update()
    {
        timeToLightning -= Time.deltaTime;
        if(timeToLightning <= 0)
        {
            LightningEffect.SetActive(true);
            Invoke("LightningEnd", 0.5f);
            timeToLightning = Random.Range(5f, 15f);
        }
    }
    void LightningEnd()
    {
        LightningEffect.SetActive(false);
    }
}
