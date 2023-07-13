using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    public TMPro.TMP_Text Text;

    public void OnMainMenuButton()
    {
        SceneManager.LoadScene("Main Menu");

    }
    public void setText(string s)
    {
        Text.text = s;
    }
}
