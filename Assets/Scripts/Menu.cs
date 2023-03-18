using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField]
    private GameObject menu;

    [SerializeField]
    private GameObject leaderboard;
    private bool leadActive;

    [SerializeField]
    private TextMeshProUGUI fullscreen;

    [SerializeField]
    private Slider sfxSlider;
    [SerializeField]
    private Slider musicSlider;

    [SerializeField]
    private List<AudioSource> sfx = new();
    [SerializeField]
    private List<AudioSource> music = new();

    private float prevTime;


    List<int> widths = new List<int>() {1024, 1152, 1280, 1366, 1600, 1920, 2560, 3840};
    List<int> heights = new List<int>() {576, 648, 720, 768, 900, 1080, 1440, 2160};


    private void Start()
    {
        menu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!menu.activeInHierarchy)
            {
                //get state of leaderboard
                if (leaderboard.activeInHierarchy)
                {
                    leadActive = true;
                    leaderboard.SetActive(false);
                }
                else
                {
                    leadActive = false;
                }

                prevTime = Time.timeScale;
                menu.SetActive(true);
                Time.timeScale = 0f;
            }
            else if (menu.activeInHierarchy)
            {
                menu.SetActive(false);
                Time.timeScale = prevTime;

                if (leadActive)
                {
                    leaderboard.SetActive(true);
                }
            }
        }

        foreach (AudioSource source in sfx)
        {
            source.volume = sfxSlider.value;
        }
        foreach (AudioSource source in music)
        {
            source.volume = musicSlider.value;
        }
    }

    public void SetResolution(int index)
    {
        bool fullscreen = Screen.fullScreen;
        int width = widths[index];
        int height = heights[index];
        Screen.SetResolution(width, height, fullscreen);
    }

    public void Fullscreen()
    {
        if (!Screen.fullScreen)
        {
            Screen.fullScreen = true;
            fullscreen.text = "fullscreen";
        }
        else
        {
            Screen.fullScreen = false;
            fullscreen.text = "not fullscreen";
        }
    }

    public void Quit()
    {
        Application.Quit();
        print("application quit");
    }
}
