using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.UIElements;

public class MusicPlayer : MonoBehaviour
{
    private MusicPlayer musicPlayer;
    private AudioClip[] currMusicTrack;
    public AudioClip[] musicTrack01;
    public AudioClip[] musicTrack02;
    private AudioSource[] musicSources;
    private AudioLowPassFilter[] lowFilters;
    [Tooltip("The volume for all instruments")]
    [Range(0.001f, 1.0f)] public float globalVolume;
    private float lastGlobalVolume;
    [Tooltip("The highest frequency that can be played by all instruments")]
    [Range(18, 5000)] public float globalCutoffFreq;
    [Range(0.001f, 0.20f)] public float fadeInSpd = 0.05f;
    [Range(0.001f, 0.20f)] public float fadeOutSpd = 0.025f;

    private GameObject tempInstance;
    private int counter = 0;
    private string tempName;
    private Type[] components = new Type[2];
    private bool[] instrumentsActive = new bool[23];

    private void Awake()
    {
        musicPlayer = GetComponent<MusicPlayer>();
        components[0] = typeof(AudioSource);
        components[1] = typeof(AudioLowPassFilter);
    }

    private void Start()
    {
        globalVolume = 1.0f;
        globalCutoffFreq = 500;
        if (musicTrack01.Length != 0 && musicTrack02.Length != 0)
        {
            SetUpMusicPLayer();
            SetTracksAndFilters();
        }
        else ErrorLog(0);
    }

    private void Update()
    {
        CheckGlobalVolumeSlider();
        TogleInstruments();
    }

    private void SetUpMusicPLayer()
    {
        int instrumentsCount = musicTrack01.Length + musicTrack02.Length;
        musicSources = new AudioSource[instrumentsCount];
        lowFilters = new AudioLowPassFilter[instrumentsCount];

        for (int i = 0; i < musicSources.Length; i++)
        {
            tempName = "Instrument" + counter.ToString();
            counter++;
            tempInstance = new GameObject(tempName, components);
            tempInstance.transform.SetParent(musicPlayer.transform);
            musicSources[i] = tempInstance.GetComponentInChildren<AudioSource>();
            lowFilters[i] = tempInstance.GetComponentInChildren<AudioLowPassFilter>();
        }
    }

    /// <summary> Set up and assign all audioclips to an AudioSource</summary>
    private void SetTracksAndFilters()
    {
        for (int i = 0; i < musicSources.Length; i++)
        {
            if (i < musicTrack01.Length)
            {
                musicSources[i].clip = musicTrack01[i];
                musicSources[i].loop = true;
                musicSources[i].volume = 0;
                musicSources[i].Play();
                lowFilters[i].cutoffFrequency = 500;
                lowFilters[i].enabled = false;
            }
            else
            {
                musicSources[i].clip = musicTrack02[i - musicTrack01.Length];
                musicSources[i].loop = true;
                musicSources[i].volume = 0;
                musicSources[i].Play();
                lowFilters[i].cutoffFrequency = 500;
                lowFilters[i].enabled = false;
            }
        }
    }

    /// <summary> Play all the instruments of a music track</summary>
    /// <param name="trackToPlay"> The track you wish to play, call from music player script</param>
    public void EnableMusicTrack(AudioClip[] trackToPlay)
    {
        if (trackToPlay == musicTrack01)
        {
            for (int i = 0; i < musicSources.Length; i++)
            {
                if (i < musicTrack01.Length) instrumentsActive[i] = true;
                else instrumentsActive[i] = false;
            }
        }
        if (trackToPlay == musicTrack02)
        {
            for (int i = 0; i < musicSources.Length; i++)
            {
                if (i < musicTrack01.Length) instrumentsActive[i] = false;
                else instrumentsActive[i] = true;
            }
        }
    }

    /// <summary> Play all the instruments of a music track</summary>
    /// <param name="trackToPlay">Accept either "1" or "2"</param>
    public void EnableMusicTrack(int trackToPlay)
    {
        if (trackToPlay == 1)
        {
            for (int i = 0; i < musicSources.Length; i++)
            {
                if (i < musicTrack01.Length) instrumentsActive[i] = true;
                else instrumentsActive[i] = false;
            }
        }
        if (trackToPlay == 2)
        {
            for (int i = 0; i < musicSources.Length; i++)
            {
                if (i < musicTrack01.Length) instrumentsActive[i] = false;
                else instrumentsActive[i] = true;
            }
        }
    }

    /// <summary> Raise the volume of all instruments of all tracks in the music player to 1</summary>
    public void PlayAll()
    {
        for (int i = 0; i < musicSources.Length; i++)
        {
            instrumentsActive[i] = true;
        }
    }

    /// <summary> Raise the volume of all instruments of all tracks in the music player to 0</summary>
    public void MuteAll()
    {
        for (int i = 0; i < musicSources.Length; i++)
        {
            instrumentsActive[i] = false;
        }
    }

    /// <summary> Raise the volume of all specified instruments. Each instrument is an integer, the first one being integer 0. Seperate all
    /// integers in the string by a coma, do not use spaces</summary>
    /// <param name="a"> A string of integers seperated by comas.</param>
    public void AddInstrument(string a)
    {
        string[] splittedParams = a.Split(',');
        foreach (string i in splittedParams)
        {
            string tempString = i;
            int tempInt = int.Parse(tempString);
            if (tempInt < musicSources.Length)
            {
                instrumentsActive[tempInt] = true;
            }
            else ErrorLog(1);
        }
    }

    /// <summary> Lower the volume of all specified instruments. Each instrument is an integer, the first one being integer 0. Seperate all
    /// integers in the string by a coma, do not use spaces</summary>
    /// <param name="a"> A string of integers seperated by comas.</param>
    public void RemoveInstrument(string a)
    {
        string[] splittedParams = a.Split(',');
        foreach (string i in splittedParams)
        {
            string tempString = i;
            int tempInt = int.Parse(tempString);
            if (tempInt < musicSources.Length)
            {
                instrumentsActive[tempInt] = false;
            }
            else ErrorLog(1);
        }
    }

    public void ToggleFilter()
    {
        for (int i = 0; i < musicSources.Length; i++)
        {
            lowFilters[i].enabled = !lowFilters[i].enabled;
            lowFilters[i].cutoffFrequency = globalCutoffFreq;
        }
    }

    private void CheckGlobalVolumeSlider()
    {
        if (globalVolume != lastGlobalVolume)
        {
            globalVolume = (float)Math.Round(globalVolume, 3);
            for (int i = 0; i < musicSources.Length; i++)
            {
                if (musicSources[i].volume != 0)
                {
                    musicSources[i].volume = globalVolume;
                }
            }
        }
        lastGlobalVolume = globalVolume;
    }

    public void TogleInstruments()
    {
        for (int i = 0; i < musicSources.Length; i++)
        {
            if (InputsManager.instance.fKeyArray[i])
            {
                instrumentsActive[i] = !instrumentsActive[i];
            }

            if (instrumentsActive[i] && musicSources[i].volume < globalVolume)
            {
                musicSources[i].volume += fadeInSpd;
            }
            if (!instrumentsActive[i] && musicSources[i].volume > 0)
            {
                musicSources[i].volume -= fadeOutSpd;
            }
        }
    }

    public void DisplayActive()
    {
        string tempString = "List of active instruments : ";
        bool temp = CheckIfEmpty();
        if (temp)
        {
            tempString += "Empty";
            Debug.Log(tempString);
        }
        else
        {
            for (int i = 0; i < musicSources.Length; i++)
            {
                if (instrumentsActive[i])
                {
                    tempString += "|" + i.ToString();
                }
            }
            Debug.Log(tempString);
        }
    }

    public bool CheckIfEmpty()
    {
        for (int i = 0; i < musicSources.Length; i++)
        {
            if (instrumentsActive[i])
            {
                return false;
            }
        }
        return true;
    }

    private void ErrorLog(int logToPrint)
    {
        switch (logToPrint)
        {
            case 0:
                Debug.LogError("Arrays sizes and clips to play are left undefined by the designer in the inspector", musicPlayer);
                break;
            case 1:
                Debug.LogError("You are trying to remove/add instrument(s) that do not exist in the music player. Instrument 12 relate to integer 11", musicPlayer);
                break;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MusicPlayer))]
public class MusicPlayerEditor : Editor
{
    MusicPlayer musicPlayer;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        musicPlayer = (MusicPlayer)target;
        if (GUILayout.Button("OVERDRIVE"))
        {
            musicPlayer.PlayAll();
        }
        if (GUILayout.Button("Play Track 01"))
        {
            musicPlayer.EnableMusicTrack(musicPlayer.musicTrack01);
        }
        if (GUILayout.Button("Play Track 02"))
        {
            musicPlayer.EnableMusicTrack(musicPlayer.musicTrack02);
        }
        if (GUILayout.Button("Mute all"))
        {
            musicPlayer.MuteAll();
        }
        if (GUILayout.Button("Toggle low pass filter"))
        {
            musicPlayer.ToggleFilter();
        }
        if (GUILayout.Button("Display actives instruments"))
        {
            musicPlayer.DisplayActive();
        }
    }
}
#endif
