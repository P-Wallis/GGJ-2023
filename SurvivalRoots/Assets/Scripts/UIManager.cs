using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        instructions.SetActive(instructionsShown);
        start.SetActive(true);
        end.SetActive(false);

        muteButton.onClick.AddListener(ToggleMute);
    }

    [HideInInspector] public bool instructionsShown = true;

    public ResourcesMeter waterMeter;
    public ResourcesMeter mineralMeter;
    public Button endTurnButton;
    public Button resetButton;
    public DayVisualizer dayVisualizer;
    public TextMeshProUGUI powerLevel;
    public UIRiseAndFade waterUpdate;
    public UIRiseAndFade mineralUpdate;
    public UIRiseAndFade noWaterAlert;
    public GameObject instructions;
    public GameObject start;
    public GameObject end;
    public TextMeshProUGUI endTitle;
    public TextMeshProUGUI endDescription;
    public Button muteButton;
    public GameObject muted, unmuted;

    bool isMuted = false;
    void ToggleMute()
    {
        isMuted = !isMuted;
        muted.SetActive(isMuted);
        unmuted.SetActive(!isMuted);
    }

    private void Update()
    {
        if(instructionsShown && Input.GetMouseButtonDown(0))
        {
            instructionsShown = false;
            instructions.SetActive(instructionsShown);
        }
    }

    public void Play()
    {
        start.SetActive(false);
    }

    public void Win()
    {
        end.SetActive(true);
        endTitle.text = "You Win!";
    }

    public void Lose()
    {
        end.SetActive(true);
        endTitle.text = "You Lose.";
        endDescription.gameObject.SetActive(false);
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
