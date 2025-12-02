using UnityEngine;

public class OptionsTabs : MonoBehaviour
{
    public GameObject controlsPage;
    public GameObject volumePage;
    public GameObject supportPage;

    public void ShowControls()
    {
        controlsPage.SetActive(true);
        volumePage.SetActive(false);
        supportPage.SetActive(false);
    }

    public void ShowVolume()
    {
        controlsPage.SetActive(false);
        volumePage.SetActive(true);
        supportPage.SetActive(false);
    }

    public void ShowSupport()
    {
        controlsPage.SetActive(false);
        volumePage.SetActive(false);
        supportPage.SetActive(true);
    }
}
