using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public Button play;
    public Button train;
    public Button test;
    public InputField write;
    public Dropdown read;
    public Text statusTitle;
    public Text statusText;
    public Dropdown trackSelect;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    private void Start()
    {
        play.onClick.AddListener(delegate
        {
            PlayerSpawner.instance.SaveBrain();
            PlayerSpawner.instance.play = true;
            SceneManager.LoadScene(0);
            statusTitle.text = "Playing";
            statusText.text = "Play around to see what the cars are trying to learn.";
        });

        train.onClick.AddListener(delegate
        {
            PlayerSpawner.instance.SaveBrain();
            PlayerSpawner.instance.play = false;
            PlayerSpawner.instance.testBrain = false;
            SceneManager.LoadScene(0);
            statusTitle.text = "Training";
            statusText.text = "Generation: 0 \nBest: 0 \nFastest Speed: 0 \nFarthest Lap:0 \nPopulation: " + PlayerSpawner.instance.populationCount + "\nMutation: " + PlayerSpawner.instance.mutationRate;
        });

        test.onClick.AddListener(delegate
        {
            if (PlayerSpawner.instance.GetAllReadableFiles().Count == 0)
            {
                statusText.text = "No trained cars to test!";
            }
            else
            {
                PlayerSpawner.instance.SaveBrain();
                PlayerSpawner.instance.play = false;
                PlayerSpawner.instance.testBrain = true;
                SceneManager.LoadScene(0);
                statusTitle.text = "Testing " + PlayerSpawner.instance.readFile;
                statusText.text = "";
            }
        });

        write.onEndEdit.AddListener(delegate
        {
            PlayerSpawner.instance.writeFile = write.text;
        });

        UpdateReadDropdown();
        PlayerSpawner.instance.readFile = read.options[0].text;
        read.onValueChanged.AddListener(delegate
        {
            PlayerSpawner.instance.readFile = read.options[read.value].text;
        });

        UpdateTrackDropdown();
        trackSelect.onValueChanged.AddListener(delegate
        {
            TrackManager.instance.SetTrackActive(trackSelect.options[trackSelect.value].text);
            SceneManager.LoadScene(0);
        });
    }

    public void UpdateReadDropdown()
    {
        read.ClearOptions();
        read.AddOptions(PlayerSpawner.instance.GetAllReadableFiles());
    }

    void UpdateTrackDropdown()
    {
        trackSelect.ClearOptions();
        trackSelect.AddOptions(TrackManager.instance.GetAllTrackNames());
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            statusText.text = Application.persistentDataPath + "/Resources";
        }
    }
}
