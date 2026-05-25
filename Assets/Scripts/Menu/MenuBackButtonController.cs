using UnityEngine;

public class MenuBackButtonController : MonoBehaviour
{
    [SerializeField] GameObject CameraMain;
    [SerializeField] GameObject CameraBed;
    [SerializeField] GameObject CameraBedTransition;
    [SerializeField] GameObject CameraItems;
    [SerializeField] GameObject CameraSettings;
    [SerializeField] GameObject MenuBackButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        if (CameraMain.activeSelf == false && CameraBed.activeSelf == false && MenuBackButton.activeSelf == false)
        {
            MenuBackButton.SetActive(true);
        }
        else if ((CameraMain.activeSelf == true || CameraBed.activeSelf == true) && MenuBackButton.activeSelf == true)
        {
            MenuBackButton.SetActive(false);
        }
    }

    public void BackToMainMenu()
    {
        Debug.Log("Back to main menu");
        CameraMain.SetActive(true);
        CameraBed.SetActive(false);
        CameraBedTransition.SetActive(false);
        CameraItems.SetActive(false);
        CameraSettings.SetActive(false);
        MenuBackButton.SetActive(false);
    }
}
