using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;



/// <summary>
/// Handles music beat synchronization, background pulsing, and bot spawning.
/// </summary>
public class BeatConductor : MonoBehaviour
{
    [Header("Audio & Beat Map")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private string beatMapFile = "song_beats.json";
    [SerializeField] private BotSpawner botSpawner;

    [Header("UI & Visuals")]
    [SerializeField] private Image backgroundPanel;  // assign your UI Panel
    [SerializeField] private float pulseStrength = 0.6f;
    [SerializeField] private float decaySpeed = 6f;
    [SerializeField] private bool enableBackgroundFlash = false;

    private BeatMap beatMap;
    private int nextBeatIndex = 0;

    private double dspStartTime;

    private float baseAlpha;
    private float targetAlpha;

    public static event Action<int, float> OnBeatGlobal;

    private void Start()
    {
        LoadBeatMap();

        // Schedule audio precisely
        dspStartTime = AudioSettings.dspTime + 0.1;
        musicSource.PlayScheduled(dspStartTime);

        baseAlpha = backgroundPanel.color.a;
        targetAlpha = baseAlpha;
    }

    private void Update()
    {
        if (beatMap == null || nextBeatIndex >= beatMap.Beats.Length)
            return;

        // DSP-accurate song time
        double songTime = AudioSettings.dspTime - dspStartTime;

        if (songTime >= beatMap.Beats[nextBeatIndex])
        {
            OnBeat(nextBeatIndex, (float)songTime);
            nextBeatIndex++;
        }

        Color c = backgroundPanel.color;
        if (enableBackgroundFlash)
        {
            c.a = Mathf.Lerp(c.a, baseAlpha, Time.deltaTime * decaySpeed);
        }
        else
        {
            c.a = baseAlpha;
        }
        backgroundPanel.color = c;
    }

    /// <summary>
    /// Loads the beat map from a JSON file.
    /// </summary>
    private void LoadBeatMap()
    {
        string path = Path.Combine(Application.streamingAssetsPath, beatMapFile);
        if (!File.Exists(path))
        {
            Debug.LogError($"Beat map file not found: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        beatMap = JsonUtility.FromJson<BeatMap>(json);
    }

    /// <summary>
    /// Determines if a bot should spawn on this beat index.
    /// </summary>
    private bool ShouldSpawn(int index)
    {
        int beat = index % 4;
        int bar = index / 4;
        // Spawn every 4 bars, on the first beat of each bar (beat 0)
        return bar % 4 == 0 && beat == 0;
    }

    /// <summary>
    /// Called on each beat. Triggers global event and visual pulse.
    /// </summary>
    private void OnBeat(int index, float time)
    {
        // TEMP: Disable bot spawning on beat for single-bot test
        // if (ShouldSpawn(index))
        // {
        //     botSpawner.SpawnBot();
        // }

        OnBeatGlobal?.Invoke(index, time);

        // Instant flash on beat
        if (enableBackgroundFlash)
        {
            targetAlpha = baseAlpha + pulseStrength;
            Color c = backgroundPanel.color;
            c.a = targetAlpha;
            backgroundPanel.color = c;
        }
    }
}
