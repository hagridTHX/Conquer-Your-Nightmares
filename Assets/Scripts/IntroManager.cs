using System.Collections;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    [Header("Intro Settings")]
    public bool playIntro = true;
    public float introDuration = 4f;

    [Header("UI References")]
    public GameObject introCanvas;

    void Start()
    {
        if (!playIntro)
        {
            introCanvas.SetActive(false);
            Time.timeScale = 1f;
            return;
        }

        Time.timeScale = 0f;
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        yield return new WaitForSecondsRealtime(introDuration);

        introCanvas.SetActive(false);
        Time.timeScale = 1f;
    }
}