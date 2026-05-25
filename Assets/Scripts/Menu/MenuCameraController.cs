using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
public class EventClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject CameraMain;
    [SerializeField] GameObject CameraBed;
    [SerializeField] GameObject CameraBedTransition;
    [SerializeField] GameObject CameraItems;
    [SerializeField] GameObject CameraSettings;
    [SerializeField] GameObject MenuBackButton;
    [SerializeField] TextMeshProUGUI MenuText;

    [SerializeField] private float transitionDuration = 2f;
    
    private string pointedOn;


    public void OnPointerDown(PointerEventData eventData)
    { 
        
    }
    public void OnPointerUp(PointerEventData eventData)
    {

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        ObjectClick();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(ChceckIfCameraMainActive())
        {
            GetComponent<Outline>().enabled = true;
            ChangeText();
        }
        pointedOn = eventData.pointerCurrentRaycast.gameObject.tag;


    }


    public async void OnPointerExit(PointerEventData eventData)
    {
        if(ChceckIfCameraMainActive())
        {
            GetComponent<Outline>().OutlineColor = Color.gold;
            GetComponent<Outline>().enabled = false;
            MenuText.text = "MENU";
        }
    }


    private void LoadScene()
    {
        SceneManager.LoadScene("SampleScene");
    }

 private void ChangeText()
    {

        if (pointedOn == "MenuBed")
        {
            MenuText.text = "PLAY";
        }
        else if (pointedOn == "MenuChest")
        {
            MenuText.text = "ITEMS";
        }
        else if (pointedOn == "MenuPicture")
        {
            MenuText.text = "SETTINGS";
        }
    }

    private async void ObjectClick()
    {
        if (pointedOn == "MenuBed" && CameraItems.activeSelf == false && CameraSettings.activeSelf == false)
        {
            GetComponent<Outline>().OutlineColor = new Color(92, 84, 0);
            CameraBedTransition.SetActive(true);
            await System.Threading.Tasks.Task.Delay(1000);
            CameraBed.SetActive(true);
            await System.Threading.Tasks.Task.Delay(1000);
            CameraBedTransition.SetActive(false);
            Invoke("LoadScene", transitionDuration);
            CameraMain.SetActive(false);
            MenuText.text = "STARTING GAME...";
        
        }
        else if (pointedOn == "MenuChest" && CameraBed.activeSelf == false && CameraSettings.activeSelf == false)
        {
            CameraItems.SetActive(true);
            CameraMain.SetActive(false);
        }
         else if (pointedOn == "MenuPicture" && CameraBed.activeSelf == false && CameraItems.activeSelf == false)
        {
            CameraSettings.SetActive(true);
            CameraMain.SetActive(false);
        }
        TurnOffOutline();

    }

    private void TurnOffOutline()
    {
        GameObject.Find("Bed").GetComponent<Outline>().enabled = false;
        GameObject.Find("Chest").GetComponent<Outline>().enabled = false;   
        GameObject.Find("Picture").GetComponent<Outline>().enabled = false;
    }
    private bool ChceckIfCameraMainActive()
    {
        if(CameraMain.activeSelf == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    
}

