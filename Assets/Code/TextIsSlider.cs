using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class TextIsSlider : MonoBehaviour
{
    private Slider slider;
    private TextMeshProUGUI text;

    private void Start()
    {
        slider = GetComponentInParent<Slider>();
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        string s = slider.value.ToString();
        text.text = s;
    }
}
