/// Designed by Félix Desrosiers-Dorval
/// Last modification date : 2019-07-01
/// Last feature added : LowPassFilter slider now updating when valus is changed
/// https://github.com/SquareUnit/Code-Storage

// Script requirements : System made for looping music tracks seamlessly into each other. At least two audioclips, 
// one in each of the two tracks a required. All audioslips must be of the same length. Audioclip length should aim 
// to have at maximum 2 decimals that round perfectly to avoid float imprecision.

using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

public class MusicPlayer : MonoBehaviour
{
    private MusicPlayer musicPlayer;
    private AudioSource[] audioSources;
    private AudioClip[] currMusicTrack;
    public AudioClip[] musicTrack01;
    public AudioClip[] musicTrack02;
    private float[] clipsMaxVolume;
    private bool[] activeClip;
    private AudioLowPassFilter[] lowFilters;
    [Tooltip("The volume for all instruments")]
    [Range(0.001f, 1.0f)] public float globalVolume;
    private float lastGlobalVolume;
    [Tooltip("The highest frequency that can be played by all instruments")]
    [Range(18, 5000)] public float globalCutoffFreq;
    private float lastGlobalCutoffFreq;
    [Range(0.001f, 0.20f)] public float fadeInSpd = 0.05f;
    [Range(0.001f, 0.20f)] public float fadeOutSpd = 0.025f;
    private bool fading;

    private GameObject tempObject;
    private int counter = 0;
    private string tempName;
    private Type[] components = new Type[2];

    public bool debugOn;

    private void Awake()
    {
        musicPlayer = GetComponent<MusicPlayer>();
        components[0] = typeof(AudioSource);
        components[1] = typeof(AudioLowPassFilter);
        globalVolume = 1.0f;
        globalCutoffFreq = 500;
    }

    private void Start()
    {
        if (musicTrack01.Length != 0 && musicTrack02.Length != 0)
        {
            SetupAudioSources();
        }
        else if (debugOn) ErrorLog(0);
    }

    private void Update()
    {
        UpdateGlobalVolumeInspector();
        UpdateGlobalFrequencyInspector();
        ToggleClipActivity();
        SetClipsVolume();
    }

    private void FixedUpdate()
    {
        if (!CheckIfAnyClipPlaying())
        {
            if (AllClipsInactive())
            {
                if(debugOn) Debug.Log("none playing & none Active");
            }
            else
            {
                PlayActiveClips();
            }
        }
        else
        {
            int firstClipFound = FirstActiveClip();
            // Here I am checking if the playing audioClip end before the current frame end.
            if (audioSources[firstClipFound].time + Time.fixedDeltaTime >= audioSources[firstClipFound].clip.length)
            {
                float delay = audioSources[firstClipFound].time + Time.fixedDeltaTime - audioSources[firstClipFound].clip.length;
                StartCoroutine(WaitForLoopEnd(delay));
            }
        }
    }

    public IEnumerator WaitForLoopEnd(float delay)
    {
        PlayActiveClips();
        fading = true;
        Invoke("StopFading", 1.0f);
        Debug.Log("WaitForLoopEnd coroutine has finished");
        yield return new WaitForSeconds(delay);
    }

    private void StopFading()
    {
        fading = false;
    }

    /// <summary> Initial setup for the music player. Run once at start. </summary>
    private void SetupAudioSources()
    {
        int clipCount = musicTrack01.Length + musicTrack02.Length;
        audioSources = new AudioSource[clipCount];
        lowFilters = new AudioLowPassFilter[clipCount];
        clipsMaxVolume = new float[clipCount];
        activeClip = new bool[clipCount];

        for (int i = 0; i < audioSources.Length; i++)
        {
            tempName = "Instrument" + counter.ToString();
            counter++;
            tempObject = new GameObject(tempName, components);
            tempObject.transform.SetParent(musicPlayer.transform);
            audioSources[i] = tempObject.GetComponentInChildren<AudioSource>();
            lowFilters[i] = tempObject.GetComponentInChildren<AudioLowPassFilter>();

            audioSources[i].playOnAwake = false;
            audioSources[i].volume = 0.0f;
            lowFilters[i].cutoffFrequency = 500;
            lowFilters[i].enabled = false;
            clipsMaxVolume[i] = 1.0f;

            SetupTracks(i);
        }
    }

    /// <summary> Set up and assign all audioclips to an AudioSource</summary>
    private void SetupTracks(int i)
    {
        if (i < musicTrack01.Length)
        {
            audioSources[i].clip = musicTrack01[i];
        }
        else
        {
            audioSources[i].clip = musicTrack02[i - musicTrack01.Length];
        }
    }

    /// <summary> Play all active instruments </summary>
    private void PlayActiveClips()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            // If the clip is active or inactive but still needing a fade out
            if (activeClip[i] || (!activeClip[i] && audioSources[i].volume >= 0.001f))
            {
                if (audioSources[i].volume <= 0.0f) audioSources[i].volume = 0.001f;
                audioSources[i].Play();
                fading = true;
                Invoke("StopFading", 1.0f);
            }
        }
    }

    /// <summary> Play all the instruments of a music track</summary>
    /// <param name="trackToPlay"> The track you wish to play, call from music player script</param>
    public void PlayMusicTrack(AudioClip[] trackToPlay)
    {
        if (trackToPlay == musicTrack01)
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                if (i < musicTrack01.Length) activeClip[i] = true;
                else activeClip[i] = false;
            }
        }
        if (trackToPlay == musicTrack02)
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                if (i < musicTrack01.Length) activeClip[i] = false;
                else activeClip[i] = true;
            }
        }
        if (!CheckIfAnyClipPlaying()) PlayActiveClips();
    }

    /// <summary> Play all the instruments of a music track</summary>
    /// <param name="trackToPlay">Accept either "1" or "2"</param>
    public void EnableMusicTrack(int trackToPlay)
    {
        if (trackToPlay == 1)
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                if (i < musicTrack01.Length) activeClip[i] = true;
                else activeClip[i] = false;
            }
        }
        if (trackToPlay == 2)
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                if (i < musicTrack01.Length) activeClip[i] = false;
                else activeClip[i] = true;
            }
        }
        if (!CheckIfAnyClipPlaying()) PlayActiveClips();
    }

    /// <summary> Raise the volume of all instruments of all tracks in the music player to 1</summary>
    public void PlayAll()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            activeClip[i] = true;
        }
    }

    /// <summary> Raise the volume of all instruments of all tracks in the music player to 0</summary>
    public void MuteAll()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            activeClip[i] = false;
            fading = true;
            Invoke("StopFading", 1.0f);
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
            if (tempInt < audioSources.Length)
            {
                activeClip[tempInt] = true;
            }
            else if (debugOn) ErrorLog(1);
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
            if (tempInt < audioSources.Length)
            {
                activeClip[tempInt] = false;
            }
            else if (debugOn) ErrorLog(1);
        }
    }

    /// <summary> Toggle on or off low pass filters for all tracks, active or not</summary>
    public void ToggleLowPassFilters()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            lowFilters[i].enabled = !lowFilters[i].enabled;
            lowFilters[i].cutoffFrequency = globalCutoffFreq;
        }
    }

    /// <summary> Unity events ready : Allow designers to change instruments volume individually. Will be overwritten if global volume value is changed.</summary>
    public void SetInstrumentVolume(int clip, float volume)
    {
        audioSources[clip].volume = clipsMaxVolume[clip] = volume;
    }

    /// <summary> Allow designers to be able to add/remove an instrument with fkeys. fkeys and shift+fkeys can toggle the 24 first clips </summary>
    public void ToggleClipActivity()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (InputsManager.instance.fKeyArray[i])
            {
                activeClip[i] = !activeClip[i];
            }
        }
    }

    /// <summary> Fade audioclips in or out depending if there are set to active or not. Also stop the audioSource if it's volume fall to 0"/> </summary>
    public void SetClipsVolume()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (fading)
            {
                if (activeClip[i])
                {
                    audioSources[i].volume += fadeInSpd;
                    if (audioSources[i].volume > clipsMaxVolume[i]) audioSources[i].volume = clipsMaxVolume[i];
                    else if (audioSources[i].volume > globalVolume) audioSources[i].volume = globalVolume;
                    else if (audioSources[i].volume > 1.0f) audioSources[i].volume = 1.0f;
                }
                else // Clip is inactive
                {
                    audioSources[i].volume -= fadeOutSpd;
                    if (audioSources[i].volume < 0.0f) audioSources[i].volume = 0.0f;
                }
            }

            if(audioSources[i].volume == 0)
            {
                audioSources[i].Stop();
            }
        }
    }

    /// <summary> Return true if at leat one audioSource is playing an audioclip </summary>
    public bool CheckIfAnyClipPlaying()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i].isPlaying) return true;
        }
        return false;
    }

    /// <summary> Display in the console all instrument currently set active </summary>
    public void PrintClip()
    {
        bool temp;
        string activeClips = " <color=orange>List of active instruments : </color>";
        string playingClips = "                          <color=orange>List of playing clips : </color>";
        temp = AllClipsInactive();
        if (temp)
        {
            activeClips += "Empty";
        }
        else
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                if (activeClip[i])
                {
                    activeClips += "|" + i.ToString();
                }
            }
        }
        temp = AllClipsNotPlaying();
        if (temp)
        {
            playingClips += "Empty";
        }
        else
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                if (audioSources[i].isPlaying)
                {
                    playingClips += "|" + i.ToString();
                }
            }
        }

        Debug.Log(activeClips + "\n" + playingClips);
    }

    /// <summary> Returns true if no active clip is found </summary>
    public bool AllClipsInactive()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (activeClip[i])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary> Returns true if no audiosource is currently playing a clip</summary>
    public bool AllClipsNotPlaying()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i].isPlaying)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary> Find the first active instruments in the playing track.</summary>
    /// <returns> The integer corresponding to the audioSource found that has an active instrument </returns>
    private int FirstActiveClip()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (activeClip[i])
            {
                return i;
            }
        }
        return 0;
    }

    /// <summary> Check if volume slider what used, if so update globalVolume. Will override individual instruments volumes </summary>
    private void UpdateGlobalVolumeInspector()
    {
        if (globalVolume != lastGlobalVolume)
        {
            globalVolume = (float)Math.Round(globalVolume, 3);
            for (int i = 0; i < audioSources.Length; i++)
            {
                if (audioSources[i].volume != 0)
                {
                    audioSources[i].volume = globalVolume;
                }
            }
        }
        lastGlobalVolume = globalVolume;
    }

    /// <summary> Check if volume slider what used, if so update globalVolume. Will override individual instruments volumes </summary>
    private void UpdateGlobalFrequencyInspector()
    {
        if (globalCutoffFreq != lastGlobalCutoffFreq)
        {
            for (int i = 0; i < audioSources.Length; i++)
            {
                if (audioSources[i].volume != 0)
                {
                    lowFilters[i].cutoffFrequency = globalCutoffFreq;
                }
            }
        }
        lastGlobalCutoffFreq = globalCutoffFreq;
    }

    /// <summary> Used to keep the code more clean by moving att redundant and long debug log to the end of the file </summary>
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
            musicPlayer.PlayMusicTrack(musicPlayer.musicTrack01);
        }
        if (GUILayout.Button("Play Track 02"))
        {
            musicPlayer.PlayMusicTrack(musicPlayer.musicTrack02);
        }
        if (GUILayout.Button("Mute all"))
        {
            musicPlayer.MuteAll();
        }
        if (GUILayout.Button("Toggle low pass filter"))
        {
            musicPlayer.ToggleLowPassFilters();
        }
        if (GUILayout.Button("Display actives instruments"))
        {
            musicPlayer.PrintClip();
        }
    }
}
#endif

// double clipDuration = (double)musicTrack01[0].samples / musicTrack01[0].frequency;
