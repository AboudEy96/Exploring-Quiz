using System.Collections;
using UnityEngine;


public class IntroTimeLineEvent : MonoBehaviour
{
    public GameObject IntroPanel;


    IEnumerator Start()
    {
        IntroPanel = this.gameObject;
        yield return new WaitForSeconds(3);
        DisableIntroPanel();
    }

    public void DisableIntroPanel()
    {
        IntroPanel.SetActive(false);
    }
}