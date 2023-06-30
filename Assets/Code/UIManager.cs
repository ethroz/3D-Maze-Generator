using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private Player player;
    private MazeGenerator mg;
    private Slider[] sliders;
    private string[] sliderNames = new string[] { "SizeX", "SizeY", "SizeZ", "Loops" };
    private GameObject[] menus;
    private GameObject quitButton;
    private TMP_InputField sensitivityField;
    private string[] menuNames = new string[] { "Pause Menu", "ScoreText" };
    private UIParticleSystem[] particleSystems;

    void Start()
    {
        player = Camera.main.GetComponent<Player>();
        mg = GameObject.FindGameObjectWithTag("GameController").GetComponent<MazeGenerator>();
        GameObject[] g = GameObject.FindGameObjectsWithTag("Slider");
        sliders = new Slider[sliderNames.Length];
        for (int i = 0; i < g.Length; i++)
        {
            for (int j = 0; j < sliderNames.Length; j++)
            {
                if (sliderNames[j] == g[i].name)
                {
                    sliders[j] = g[i].GetComponent<Slider>();
                    break;
                }
            }
        }
        sliders[0].value = mg.Size.x;
        sliders[1].value = mg.Size.y;
        sliders[2].value = mg.Size.z;
        sliders[3].value = mg.Loops;
        sliders[0].onValueChanged.AddListener(delegate { SizeXChanged(); });
        sliders[1].onValueChanged.AddListener(delegate { SizeYChanged(); });
        sliders[2].onValueChanged.AddListener(delegate { SizeZChanged(); });
        sliders[3].onValueChanged.AddListener(delegate { LoopsChanged(); });
        SizeXChanged();
        SizeYChanged();
        SizeZChanged();
        LoopsChanged();

        g = GameObject.FindGameObjectsWithTag("Menu");
        menus = new GameObject[menuNames.Length];
        for (int i = 0; i < g.Length; i++)
        {
            for (int j = 0; j < menuNames.Length; j++)
            {
                if (menuNames[j] == g[i].name)
                {
                    menus[j] = g[i];
                    break;
                }
            }
        }
        foreach (GameObject m in menus)
        {
            m.SetActive(false);
        }

        quitButton = GameObject.FindGameObjectWithTag("Button");
        quitButton.SetActive(false);

        sensitivityField = GameObject.FindGameObjectWithTag("InputForm").GetComponent<TMP_InputField>();
        sensitivityField.text = (player.Sensitivity * 100.0f).ToString();
        sensitivityField.onValueChanged.AddListener(delegate { SensitivityChanged(); });
        SensitivityChanged();
        sensitivityField.gameObject.SetActive(false);

        g = GameObject.FindGameObjectsWithTag("ParticleSystem");
        particleSystems = new UIParticleSystem[g.Length];
        for (int i = 0; i < g.Length; i++)
        {
            particleSystems[i] = g[i].GetComponent<UIParticleSystem>();
        }

        mg.GenerateMaze();
    }

    public void TogglePause()
    {
        menus[0].SetActive(!menus[0].activeSelf);
        quitButton.SetActive(!quitButton.activeSelf);
        sensitivityField.gameObject.SetActive(!sensitivityField.gameObject.activeSelf);
    }

    public void ToggleScoreText()
    {
        menus[1].SetActive(!menus[1].activeSelf);
    }

    public bool IsScoreToggled()
    {
        return menus[1].activeSelf;
    }

    public void ChangeScoreText(int score)
    {
        menus[1].GetComponent<Text>().text = "Your Time: " + score + "s";
    }

    public void ChangeScoreText(string text)
    {
        menus[1].GetComponent<Text>().text = text;
    }

    public void PlayParticles()
    {
        foreach (UIParticleSystem p in particleSystems)
        {
            p.Play();
        }
    }

    public float ParticleDuration()
    {
        return particleSystems[0].Duration + particleSystems[0].Lifetime;
    }

    private void SizeXChanged()
    {
        mg.Size.x = (int)sliders[0].value;
    }

    private void SizeYChanged()
    {
        mg.Size.y = (int)sliders[1].value;
    }

    private void SizeZChanged()
    {
        mg.Size.z = (int)sliders[2].value;
    }

    private void LoopsChanged()
    {
        mg.Loops = (int)sliders[3].value;
    }

    private void SensitivityChanged()
    {
        if (float.TryParse(sensitivityField.text, out float f))
            player.Sensitivity = f / 100.0f;
    }
}
