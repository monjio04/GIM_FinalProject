using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;
using UnityEngine.VFX;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public sealed class FxHotkeyPlayer : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode playKey = KeyCode.Alpha1;
    [SerializeField] private bool alsoAcceptNumpad1 = true;

    [Header("FX")]
    [SerializeField] private VisualEffect[] visualEffects = new VisualEffect[2];
    [Tooltip("Playback speed for each VFX slot. 1 is normal, 2 is double speed, 0.5 is half speed.")]
    [SerializeField] private float[] visualEffectPlayRates = { 1f, 1f };
    [Tooltip("Optional extra event names sent to each VFX slot after Play(). Leave empty for normal VFX Graphs, or set the custom event name used by that graph.")]
    [SerializeField] private string[] visualEffectEventNames = { "", "" };
    [SerializeField] private AlembicCue[] alembicCues = { new AlembicCue(), new AlembicCue(), new AlembicCue() };
    [SerializeField] private ParticleSystem[] particleSystems;
    [SerializeField] private AudioCue[] audioCues = { new AudioCue(), new AudioCue(), new AudioCue() };
    [SerializeField] private FxCameraShake[] cameraShakes = new FxCameraShake[1];

    [Header("Playback")]
    [SerializeField] private bool activateObjectsBeforePlay = true;
    [SerializeField] private bool restartIfAlreadyPlaying = true;
    [SerializeField] private float alembicSpeed = 1f;
    [SerializeField] private float alembicDurationFallback = 3f;
    [SerializeField] private float alembicTotalFrames = 120f;
    [SerializeField] private bool logPlaybackWarnings = true;

    private readonly List<RuntimeAlembicPlayer> runningAlembics = new List<RuntimeAlembicPlayer>();
    private readonly List<RuntimeAudioCue> pendingAudioCues = new List<RuntimeAudioCue>();

    private void Update()
    {
        if (WasPlayKeyPressed())
        {
            PlayAll();
        }

        TickAlembics();
        TickAudioCues();
    }

    public void PlayAll()
    {
        PlayVisualEffects();
        PlayParticleSystems();
        PlayAudioCues();
        PlayAlembics();
        PlayCameraShakes();
    }

    private void PlayVisualEffects()
    {
        if (visualEffects == null)
        {
            return;
        }

        for (int i = 0; i < visualEffects.Length; i++)
        {
            VisualEffect visualEffect = visualEffects[i];
            if (visualEffect == null)
            {
                LogWarning("Visual Effects slot " + i + " is empty.");
                continue;
            }

            ActivateIfNeeded(visualEffect.gameObject);
            if (!visualEffect.gameObject.activeInHierarchy)
            {
                LogWarning(visualEffect.name + " is not active in hierarchy. Check if one of its parents is disabled.");
                continue;
            }

            if (visualEffect.visualEffectAsset == null)
            {
                LogWarning(visualEffect.name + " has no Visual Effect Asset assigned.");
                continue;
            }

            if (restartIfAlreadyPlaying)
            {
                visualEffect.Stop();
                visualEffect.Reinit();
            }

            visualEffect.playRate = GetVisualEffectPlayRate(i);
            visualEffect.Play();

            string eventName = GetVisualEffectEventName(i);
            if (!string.IsNullOrEmpty(eventName))
            {
                visualEffect.SendEvent(eventName);
            }
        }
    }

    private float GetVisualEffectPlayRate(int index)
    {
        if (visualEffectPlayRates == null || index >= visualEffectPlayRates.Length)
        {
            return 1f;
        }

        return Mathf.Max(0f, visualEffectPlayRates[index]);
    }

    private string GetVisualEffectEventName(int index)
    {
        if (visualEffectEventNames == null || index >= visualEffectEventNames.Length)
        {
            return "";
        }

        return visualEffectEventNames[index];
    }

    private void PlayParticleSystems()
    {
        if (particleSystems == null)
        {
            return;
        }

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            if (particleSystem == null)
            {
                continue;
            }

            ActivateIfNeeded(particleSystem.gameObject);
            if (restartIfAlreadyPlaying)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            particleSystem.Play(true);
        }
    }

    private void PlayAudioCues()
    {
        if (audioCues == null)
        {
            return;
        }

        if (restartIfAlreadyPlaying)
        {
            pendingAudioCues.Clear();
        }

        foreach (AudioCue audioCue in audioCues)
        {
            if (audioCue == null)
            {
                continue;
            }

            AudioSource audioSource = audioCue.Source;
            if (audioSource == null)
            {
                continue;
            }

            ActivateIfNeeded(audioSource.gameObject);

            if (restartIfAlreadyPlaying)
            {
                audioSource.Stop();
            }

            audioSource.volume = audioCue.Volume;
            audioSource.pitch = audioCue.Pitch;

            if (audioCue.DelaySeconds <= 0f)
            {
                audioSource.Play();
            }
            else
            {
                pendingAudioCues.Add(new RuntimeAudioCue(audioSource, audioCue.DelaySeconds));
            }
        }
    }

    private void TickAudioCues()
    {
        if (pendingAudioCues.Count == 0)
        {
            return;
        }

        for (int i = pendingAudioCues.Count - 1; i >= 0; i--)
        {
            RuntimeAudioCue cue = pendingAudioCues[i];
            if (!cue.Tick(Time.deltaTime))
            {
                continue;
            }

            cue.Play();
            pendingAudioCues.RemoveAt(i);
        }
    }

    private void PlayCameraShakes()
    {
        if (cameraShakes == null)
        {
            return;
        }

        foreach (FxCameraShake cameraShake in cameraShakes)
        {
            if (cameraShake == null)
            {
                continue;
            }

            cameraShake.StartShake();
        }
    }

    private void PlayAlembics()
    {
        if (alembicCues == null)
        {
            return;
        }

        if (restartIfAlreadyPlaying)
        {
            runningAlembics.Clear();
        }

        foreach (AlembicCue cue in alembicCues)
        {
            if (cue == null)
            {
                continue;
            }

            AlembicStreamPlayer streamPlayer = cue.StreamPlayer;
            if (streamPlayer == null)
            {
                continue;
            }

            ActivateIfNeeded(streamPlayer.gameObject);

            RuntimeAlembicPlayer player = new RuntimeAlembicPlayer(
                streamPlayer,
                cue,
                Mathf.Max(1f, alembicTotalFrames),
                Mathf.Max(0.01f, alembicDurationFallback));

            player.SetTime(player.StartTime);
            runningAlembics.Add(player);
        }
    }

    private void TickAlembics()
    {
        if (runningAlembics.Count == 0)
        {
            return;
        }

        float speed = Mathf.Max(0f, alembicSpeed);
        for (int i = runningAlembics.Count - 1; i >= 0; i--)
        {
            RuntimeAlembicPlayer player = runningAlembics[i];

            if (player.TickDelay(Time.deltaTime))
            {
                continue;
            }

            float nextTime = player.CurrentTime + Time.deltaTime * speed * player.Speed;

            if (nextTime >= player.EndTime)
            {
                player.SetTime(player.EndTime);
                runningAlembics.RemoveAt(i);
                continue;
            }

            player.SetTime(nextTime);
        }
    }

    private void ActivateIfNeeded(GameObject target)
    {
        if (activateObjectsBeforePlay && target != null && !target.activeSelf)
        {
            target.SetActive(true);
        }
    }

    private void LogWarning(string message)
    {
        if (logPlaybackWarnings)
        {
            Debug.LogWarning("[FxHotkeyPlayer] " + message, this);
        }
    }

    private bool WasPlayKeyPressed()
    {
        return WasKeyPressed(playKey) || (alsoAcceptNumpad1 && WasKeyPressed(KeyCode.Keypad1));
    }

    private static bool WasKeyPressed(KeyCode key)
    {
#if ENABLE_INPUT_SYSTEM
        Key control = ToInputSystemKey(key);
        return control != Key.None && Keyboard.current != null && Keyboard.current[control].wasPressedThisFrame;
#else
        return Input.GetKeyDown(key);
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private static Key ToInputSystemKey(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Alpha1: return Key.Digit1;
            case KeyCode.Keypad1: return Key.Numpad1;
            default: return Key.None;
        }
    }
#endif

    [System.Serializable]
    private sealed class AlembicCue
    {
        [Tooltip("Assign an AlembicStreamPlayer component here.")]
        [SerializeField] private AlembicStreamPlayer streamPlayer;
        [Tooltip("Frame inside the ABC to start from. 0 starts from the beginning, 30 starts from frame 30.")]
        [SerializeField] private float startFrame;
        [Tooltip("Delay in seconds after the hotkey before this ABC starts advancing.")]
        [SerializeField] private float startDelaySeconds;
        [Tooltip("Total frame count for this ABC. 0 uses the global Alembic Total Frames value.")]
        [SerializeField] private float totalFramesOverride;
        [Tooltip("Playback speed for only this ABC. 1 is normal speed.")]
        [SerializeField] private float speed = 1f;

        public AlembicStreamPlayer StreamPlayer => streamPlayer;
        public float StartFrame => Mathf.Max(0f, startFrame);
        public float StartDelaySeconds => Mathf.Max(0f, startDelaySeconds);
        public float Speed => Mathf.Max(0f, speed);

        public float GetTotalFrames(float globalTotalFrames)
        {
            return totalFramesOverride > 0f ? totalFramesOverride : globalTotalFrames;
        }
    }

    [System.Serializable]
    private sealed class AudioCue
    {
        [SerializeField] private AudioSource source;
        [Tooltip("Delay in seconds after the hotkey before this audio starts.")]
        [SerializeField] private float delaySeconds;
        [Range(0f, 1f)]
        [SerializeField] private float volume = 1f;
        [SerializeField] private float pitch = 1f;

        public AudioSource Source => source;
        public float DelaySeconds => Mathf.Max(0f, delaySeconds);
        public float Volume => Mathf.Clamp01(volume);
        public float Pitch => Mathf.Max(0.01f, pitch);
    }

    private sealed class RuntimeAudioCue
    {
        private readonly AudioSource source;
        private float delayRemaining;

        public RuntimeAudioCue(AudioSource source, float delaySeconds)
        {
            this.source = source;
            delayRemaining = Mathf.Max(0f, delaySeconds);
        }

        public bool Tick(float deltaTime)
        {
            delayRemaining = Mathf.Max(0f, delayRemaining - deltaTime);
            return delayRemaining <= 0f;
        }

        public void Play()
        {
            if (source != null)
            {
                source.Play();
            }
        }
    }

    private sealed class RuntimeAlembicPlayer
    {
        private readonly AlembicStreamPlayer streamPlayer;
        private float delayRemaining;

        public float StartTime { get; }
        public float EndTime { get; }
        public float Speed { get; }
        public float CurrentTime { get; private set; }

        public RuntimeAlembicPlayer(AlembicStreamPlayer streamPlayer, AlembicCue cue, float globalTotalFrames, float fallbackDuration)
        {
            this.streamPlayer = streamPlayer;
            float duration = streamPlayer.Duration;
            if (duration <= 0f)
            {
                duration = Mathf.Max(0f, streamPlayer.EndTime - streamPlayer.StartTime);
            }

            EndTime = duration > 0f ? duration : fallbackDuration;
            float totalFrames = Mathf.Max(1f, cue.GetTotalFrames(globalTotalFrames));
            StartTime = Mathf.Clamp(cue.StartFrame / totalFrames * EndTime, 0f, EndTime);
            Speed = cue.Speed;
            delayRemaining = cue.StartDelaySeconds;
            CurrentTime = StartTime;
        }

        public bool TickDelay(float deltaTime)
        {
            if (delayRemaining <= 0f)
            {
                return false;
            }

            delayRemaining = Mathf.Max(0f, delayRemaining - deltaTime);
            return delayRemaining > 0f;
        }

        public void SetTime(float time)
        {
            CurrentTime = Mathf.Clamp(time, 0f, EndTime);
            streamPlayer.UpdateImmediately(CurrentTime);
        }
    }
}
