using UnityEngine;

public class LightControl : MonoBehaviour
{
    [SerializeField] GameObject LightningEffect;
    [SerializeField] GameObject RoomLight;

    [SerializeField] private float timeToLightning = 3f;
    [SerializeField] private float timeToRoomLightGlich = 0.5f;
    [SerializeField] private float timeRoomLightGlich = 0.3f;
    [SerializeField] private int timeRoomLightGlichDuration = 300;


    private float RoomLightRanomNumer;

    
    private void Update()
    {
        LightningControll();
        RoomLightControll();
    }

    private void LightningControll()
    {
        timeToLightning -= Time.deltaTime;
        if (timeToLightning <= 0)
        {
            LightningEffect.SetActive(true);
            Invoke("LightningEnd", 0.5f);
            timeToLightning = Random.Range(5f, 15f);
        }
    }

    private void LightningEnd()
    {
        LightningEffect.SetActive(false);
    }

    private void RoomLightControll()
    {
        timeToRoomLightGlich -= Time.deltaTime;
        if (timeToRoomLightGlich <= 0)
        {
            RoomLightRanomNumer = Random.Range(0, 10) + 1;
            if (RoomLightRanomNumer <= 5)
            {
            RoomLightGlich();
            timeToRoomLightGlich = Random.Range(3f, 5f);
            }

            else if (RoomLightRanomNumer > 5)
            {
                RoomLightGlich();
                Invoke("RoomLightGlich", timeRoomLightGlich);
                timeToRoomLightGlich = Random.Range(3f, 5f);
            }

        }
    }

    private async void RoomLightGlich()
    {
        RoomLight.SetActive(false);
        await System.Threading.Tasks.Task.Delay(timeRoomLightGlichDuration);
        RoomLight.SetActive(true);
    }
   
  
}
