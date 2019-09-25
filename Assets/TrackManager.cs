using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    public static TrackManager instance;

    public GameObject tracks;

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

    public void SetTrackActive(string trackName)
    {
        foreach (Transform track in tracks.transform)
        {
            if (trackName.Equals(track.name))
            {
                track.gameObject.SetActive(true);
            }
            else
            {
                track.gameObject.SetActive(false);
            }
        }
    }

    public List<string> GetAllTrackNames()
    {
        List<string> res = new List<string>();
        foreach (Transform track in tracks.transform)
        {
            res.Add(track.name);
        }
        return res;
    }
}
