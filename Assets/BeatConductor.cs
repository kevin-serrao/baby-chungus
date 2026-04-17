using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;

public class BeatConductor : MonoBehaviour
{
    public AudioSource musicSource;
    public string beatMapFile = "song_beats.json";
    public BotSpawner botSpawner;

    public Image backgroundPanel;  // assign your UI Panel
    public float pulseStrength = 0.6f;
    public float decaySpeed = 6f;

    BeatMap beatMap;
    int nextBeatIndex = 0;

    double dspStartTime;

    float baseAlpha;
    float targetAlpha;

    public static event Action<int, float> OnBeatGlobal;

    void Start()
    {
        LoadBeatMap();

        // Schedule audio precisely
        dspStartTime = AudioSettings.dspTime + 0.1;
        musicSource.PlayScheduled(dspStartTime);

        baseAlpha = backgroundPanel.color.a;
        targetAlpha = baseAlpha;
    }

    void Update()
    {
        if (beatMap == null || nextBeatIndex >= beatMap.beats.Length)
            return;

        // DSP-accurate song time
        double songTime =
            AudioSettings.dspTime - dspStartTime;

        if (songTime >= beatMap.beats[nextBeatIndex])
        {
            OnBeat(nextBeatIndex, (float)songTime);
            nextBeatIndex++;
        }

        Color c = backgroundPanel.color;
        c.a = Mathf.Lerp(c.a, baseAlpha, Time.deltaTime * decaySpeed);
        backgroundPanel.color = c;
    }

    void LoadBeatMap()
    {
        Debug.Log(Path.Combine(
            Application.streamingAssetsPath,
            beatMapFile
        ));
        string path = Path.Combine(
            Application.streamingAssetsPath,
            beatMapFile
        );

        string json = File.ReadAllText(path);
        Debug.Log(json);
        beatMap = JsonUtility.FromJson<BeatMap>(json);
    }
    // 9 is first spawn
    // spawn on every 8 until 
    Boolean shouldSpawn(int index)
    {
        int beat = index % 4;
        int bar = index / 4;
        if (bar < 2)
        {
            return false;
        }
        if ( bar < 8)
        {
            return bar % 2 == 0 && beat == 1;
        }
        return bar < 33 && beat < 1 ? beat == 3 : true;
    }

    void OnBeat(int index, float time)
    {
        // Spawn on beat
        if (shouldSpawn(index))
        {
            botSpawner.spawnBot();
        }

        OnBeatGlobal?.Invoke(index, time);

        // Instant flash on beat
        Color c = backgroundPanel.color;
        c.a = baseAlpha + pulseStrength;
        backgroundPanel.color = c;
    }
}
