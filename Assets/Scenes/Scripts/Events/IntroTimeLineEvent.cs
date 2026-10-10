using System.Collections;
using UnityEngine;


public class IntroTimeLineEvent : MonoBehaviour
{
    public GameObject IntroPanel;


    IEnumerator Start()
    {
        IntroPanel = this.gameObject;
        yield return new WaitForSeconds(10);
        DisableIntroPanel();
    }

    public void DisableIntroPanel()
    {
        IntroPanel.SetActive(false);
    }
}