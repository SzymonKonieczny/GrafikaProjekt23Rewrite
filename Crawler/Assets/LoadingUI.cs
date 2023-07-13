using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    public Slider slider;

    public TMPro.TMP_Text Text;
    public void SetText(string s)
    {
        Text.text = s;
    }
    public void SetSliderCompletion(float f)
    {
        slider.interactable = true;
        slider.value = f;
        slider.interactable = false;

    }
}
