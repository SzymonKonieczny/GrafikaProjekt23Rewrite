using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartUI : MonoBehaviour
{
    public void OnMainMenuButton()
    {
        SceneManager.LoadScene("Main Menu");

    }
}
