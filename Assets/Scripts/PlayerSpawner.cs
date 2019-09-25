using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    string appName = "SteeringAI";

    public static PlayerSpawner instance;

    public bool play = true;
    public bool testBrain = true;

    public GameObject player;

    public int populationCount;
    public PlayerController[] population;
    public int populationAlive;
    public float mutationRate = .15f;
    int generation = 1;
    float globalBestScore = 0;
    float globalFastest = 0;
    float globalMostLaps = 0;
    NeuralNetwork globalBestBrain;
    FitnessComparer fc;

    public string writeFile;
    public string readFile;
    public TextAsset premadeBrain;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Start();
    }

    private void Start()
    {
        // actors ignore each other
        Physics2D.IgnoreLayerCollision(10, 10, true);
        ImportPremadeBrain();

        // reset scores
        generation = 0;
        globalBestScore = 0;
        globalFastest = 0;
        globalMostLaps = 0;
        globalBestBrain = null;
        population = null;

        if (play)
        {
            Instantiate(player);
        }
        else
        {
            if (testBrain)
            {
                NeuralNetwork testb = JsonUtility.FromJson<NeuralNetwork>(ReadBrain(readFile));
                StartCoroutine(AssignBrain(Instantiate(player).GetComponent<PlayerController>(), testb));
            }
            else
            {
                fc = new FitnessComparer();
                population = new PlayerController[populationCount];

                for (int j = 0; j < populationCount; j++)
                {
                    GameObject p = Instantiate(player);
                    population[j] = p.GetComponent<PlayerController>();
                }
                populationAlive = populationCount;
            }
        }
    }

    // if premade brain is not in record, add it
    void ImportPremadeBrain()
    {
        print(Application.persistentDataPath + "/Resources/" + appName);
        if (!Directory.Exists(Application.persistentDataPath + "/Resources/" + appName))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Resources/" + appName);
        }

        if (!GetAllReadableFiles().Contains(premadeBrain.name))
        {
            string path = Application.persistentDataPath + "/Resources/" + appName + "/" + premadeBrain.name + ".txt";
            StreamWriter sw = new StreamWriter(path, false);
            sw.Write(premadeBrain.text);
            sw.Close();
            PlayerPrefs.SetString(appName + premadeBrain.name, premadeBrain.text);
            UIManager.instance.UpdateReadDropdown();
        }
    }

    System.Collections.IEnumerator AssignBrain(PlayerController pc, NeuralNetwork nn)
    {
        yield return new WaitForEndOfFrame();
        pc.brain = nn;
        UIManager.instance.statusTitle.text = "Testing " + readFile;
        UIManager.instance.statusText.text = "Highest achieved in training: " + ReadScore(readFile);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            FastForward();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            NormalSpeed();
        }

        if (play || testBrain || population == null)
        {
            return;
        }

        if (populationAlive == 0)
        {
            Repopulate();
            generation++;
            UIManager.instance.statusText.text = ("Generation: " + generation + " \nBest: " + globalBestScore + " \nFastest Speed: " + (int)globalFastest + " \nFarthest Lap: " + globalMostLaps + "\nPopulation: " + populationCount + "\nMutation: " + mutationRate);
        }
    }

    public void FastForward()
    {
        Time.timeScale = 20;
        UIManager.instance.statusText.text = "Now x20 speed";
    }

    public void NormalSpeed()
    {
        Time.timeScale = 1;
        UIManager.instance.statusText.text = "Now x1 speed";
    }

    public void SpeedUp()
    {
        Time.timeScale += .5f;
        UIManager.instance.statusText.text = "Speeding up to x" + Time.timeScale + " speed";
    }

    public void SlowDown()
    {
        Time.timeScale -= .25f;
        UIManager.instance.statusText.text = "Slowing down to x" + Time.timeScale + " speed";
        if (Time.timeScale <= 0)
        {
            Time.timeScale = .25f;
            UIManager.instance.statusText.text = "Cannot go any slower! Min is x.25 speed";
        }
    }

    void Repopulate()
    {
        Array.Sort(population, fc);
        // records highest score
        // auto best if has higher lap count than last best brain
        if (population[0].lap > globalMostLaps)
        {
            globalMostLaps = population[0].lap;
            globalFastest = population[0].averageVelocity;
            globalBestScore = population[0].score;
            globalBestBrain = population[0].brain.Copy();
        }
        else
        {
            // if course complete, care more about speed than score
            if (population[0].lap == 3)
            {
                foreach (PlayerController pc in population)
                {
                    if (pc.lap == 3 && pc.averageVelocity > globalFastest)
                    {
                        globalMostLaps = pc.lap;
                        globalFastest = pc.averageVelocity;
                        globalBestScore = pc.score;
                        globalBestBrain = pc.brain.Copy();
                    }
                }
            }
            // otherwise care more about score
            else
            {
                if (population[0].score > globalBestScore)
                {
                    globalMostLaps = population[0].lap;
                    globalFastest = population[0].averageVelocity;
                    globalBestScore = population[0].score;
                    globalBestBrain = population[0].brain.Copy();
                }
            }
        }


        NeuralNetwork[] newPopulation = new NeuralNetwork[populationCount];
        for (int j = 0; j < populationCount; j += 2)
        {
            // creates 2 babies based on 2 genes
            NeuralNetwork parentA = PickOne(population).brain;
            NeuralNetwork parentB = PickOne(population).brain;
            NeuralNetwork[] babies = parentA.CrossOver(parentB);
            NeuralNetwork babyBrain1 = babies[0];
            NeuralNetwork babyBrain2 = babies[1];
            babyBrain1.Mutate(mutationRate);
            babyBrain2.Mutate(mutationRate);
            newPopulation[j] = babyBrain1;
            newPopulation[j + 1] = babyBrain2;
        }

        for (int j = 0; j < populationCount; j++)
        {
            population[j].Reset();
            population[j].brain = newPopulation[j];
        }

        populationAlive = populationCount;

        // saves info every new generation
        SaveBrain();
    }

    PlayerController PickOne(PlayerController[] pool)
    {
        // gets sum to normalize score
        float sum = 0;
        foreach (PlayerController pc in pool)
        {
            sum += pc.score;
        }
        // creates array of normalized score
        float[] normalizedScore = new float[pool.Length];
        for (int j = 0; j < pool.Length; j++)
        {
            normalizedScore[j] = pool[j].score / sum;
        }
        // gets random number
        float rand = UnityEngine.Random.value;
        int index = 0;
        // sees where it drops
        while (rand > 0)
        {
            rand -= normalizedScore[index];
            index++;
        }
        index--;
        // returns pc
        return pool[index];
    }

    public void SetMutation(float f)
    {
        mutationRate = f;
    }

    public void SetPopulation(float f)
    {
        // only even populations
        populationCount = 50 + 2 * (int)(50 * f);
        // if changing population while training, restart training
        if (!play && !testBrain)
        {
            UIManager.instance.train.onClick.Invoke();
        }
    }

    // write to file(for visibility) and player pref
    // only read from player pref
    public void SaveBrain()
    {
        // if not training
        if (population == null)
        {
            return;
        }

        // save highest score if still running
        Array.Sort(population, fc);
        // records highest score
        if (population[0].score > globalBestScore)
        {
            globalBestScore = population[0].score;
            globalBestBrain = population[0].brain.Copy();
        }

        float score = globalBestScore;
        string s = JsonUtility.ToJson(globalBestBrain);
        string path = Application.persistentDataPath + "/Resources/" + appName + "/" + writeFile + ".txt";
        PrepareWriteFile();

        // file already exists with a score
        try
        {
            float previousScore = ReadScore(writeFile);

            // only write if score is better
            if (score > previousScore)
            {
                // Rewrite brain
                StreamWriter writer = new StreamWriter(path, false);
                writer.WriteLine(score);
                writer.WriteLine(s);
                writer.Close();

                // rewrite record
                PlayerPrefs.SetString(appName + writeFile, score + "\n" + s);
            }
        }
        // file is in wrong format
        catch
        {
            // Rewrite brain
            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(score);
            writer.WriteLine(s);
            writer.Close();

            // rewrite record
            PlayerPrefs.SetString(appName + writeFile, score + "\n" + s);
        }
    }

    // reads the brain string from a file path
    string ReadBrain(string path)
    {
        // retreives text from asset
        String res = "";
        if (PlayerPrefs.HasKey(appName + path))
        {
            res = PlayerPrefs.GetString(appName + path);
        }
        // parces the JSON 
        res = res.Substring(res.IndexOf("{"));

        return res;
    }

    // returns score from a file path
    float ReadScore(string path)
    {
        // retreives text from asset
        String res = "";
        if (PlayerPrefs.HasKey(appName + path))
        {
            res = PlayerPrefs.GetString(appName + path);
        }
        Debug.Log("In Read Score" + "\nRes: " + res + "\nScore: " + res.IndexOf("{"));
        // parces the score 
        res = res.Substring(0, res.IndexOf("{")).Trim();
        float previousScore = float.Parse(res);
        return previousScore;
    }

    // gets all txt file names in Resources
    public List<string> GetAllReadableFiles()
    {
        string filePath = Application.persistentDataPath + "/Resources/" + appName;
        DirectoryInfo dir = new DirectoryInfo(filePath);
        FileInfo[] info = dir.GetFiles("*.txt");
        string[] res = new string[info.Length];
        for (int j = 0; j < info.Length; j++)
        {
            res[j] = Path.GetFileNameWithoutExtension(info[j].ToString());
        }
        return new List<string>(res);
    }

    // returns if the file to be written previously existed
    public bool PrepareWriteFile()
    {
        string path = Application.persistentDataPath + "/Resources/" + appName + "/" + writeFile + ".txt";
        // if not in filesystem
        if (!GetAllReadableFiles().Contains(writeFile))
        {
            StreamWriter sw = new StreamWriter(path, false);
            // if in record, update file system
            if (PlayerPrefs.HasKey(appName + writeFile))
            {
                Debug.Log("File does not exist, PP does exist");
                sw.Write(PlayerPrefs.GetString(appName + writeFile));
                sw.Close();
                UIManager.instance.UpdateReadDropdown();
                return true;
            }
            else // if not in record, create empty record and file
            {
                Debug.Log("File does not exist, PP does not exist");
                sw.Write("");
                sw.Close();
                PlayerPrefs.SetString(appName + writeFile, "");
                UIManager.instance.UpdateReadDropdown();
                return false;
            }
        }
        // if in filesystem
        else
        {
            // if in record, do nothing
            if (PlayerPrefs.HasKey(appName + writeFile))
            {
                Debug.Log("File does exist, PP does exist");
                return true;
            }
            // if not in record, commit to record
            else
            {
                PlayerPrefs.SetString(appName + writeFile, "");
                return true;
            }
        }
    }
}

public class FitnessComparer : IComparer<PlayerController>
{
    public int Compare(PlayerController x, PlayerController y)
    {
        return (int)(y.score * 100 - x.score * 100);
    }
}
