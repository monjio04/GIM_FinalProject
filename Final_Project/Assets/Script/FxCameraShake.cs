using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public sealed class FxCameraShake : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode shakeKey = KeyCode.Alpha1;
    [SerializeField] private bool alsoAcceptNumpad1 = true;

    [Header("Shake")]
    [SerializeField] private float duration = 1f;
    [SerializeField] private float positionStrength = 0.25f;
    [SerializeField] private float rotationStrength = 2.5f;
    [SerializeField] private float frequency = 28f;
    [SerializeField] private AnimationCurve falloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private Vector3 previousPositionOffset;
    private Quaternion previousRotationOffset = Quaternion.identity;
    private float shakeTimeRemaining;
    private float shakeElapsed;
    private float seed;
    private bool wasShaking;

    private void Awake()
    {
        previousPositionOffset = Vector3.zero;
        previousRotationOffset = Quaternion.identity;
        seed = Random.value * 1000f;
    }

    private void OnDisable()
    {
        ResetCameraOffset();
    }

    private void Update()
    {
        if (WasShakeKeyPressed())
        {
            StartShake();
        }
    }

    private void LateUpdate()
    {
        UpdateShake();
    }

    public void StartShake()
    {
        RemoveCurrentShakeOffset();
        shakeTimeRemaining = Mathf.Max(0f, duration);
        shakeElapsed = 0f;
        seed = Random.value * 1000f;
    }

    private void UpdateShake()
    {
        if (shakeTimeRemaining <= 0f)
        {
            if (wasShaking)
            {
                RemoveCurrentShakeOffset();
                wasShaking = false;
            }

            return;
        }

        wasShaking = true;
        shakeElapsed += Time.deltaTime;
        shakeTimeRemaining = Mathf.Max(0f, shakeTimeRemaining - Time.deltaTime);

        float normalizedTime = duration > 0f ? Mathf.Clamp01(shakeElapsed / duration) : 1f;
        float strength = Mathf.Max(0f, falloff.Evaluate(normalizedTime));
        float time = Time.time * Mathf.Max(0.01f, frequency);

        Vector3 positionOffset = new Vector3(
            Noise(seed + 0.0f, time),
            Noise(seed + 8.3f, time),
            Noise(seed + 16.7f, time) * 0.35f) * positionStrength * strength;

        Vector3 rotationOffset = new Vector3(
            Noise(seed + 21.1f, time),
            Noise(seed + 32.9f, time),
            Noise(seed + 43.5f, time)) * rotationStrength * strength;

        ApplyShakeOffset(positionOffset, Quaternion.Euler(rotationOffset));
    }

    private void ResetCameraOffset()
    {
        RemoveCurrentShakeOffset();
    }

    private void ApplyShakeOffset(Vector3 positionOffset, Quaternion rotationOffset)
    {
        RemoveCurrentShakeOffset();
        transform.localPosition += positionOffset;
        transform.localRotation *= rotationOffset;
        previousPositionOffset = positionOffset;
        previousRotationOffset = rotationOffset;
    }

    private void RemoveCurrentShakeOffset()
    {
        transform.localPosition -= previousPositionOffset;
        transform.localRotation *= Quaternion.Inverse(previousRotationOffset);
        previousPositionOffset = Vector3.zero;
        previousRotationOffset = Quaternion.identity;
    }

    private bool WasShakeKeyPressed()
    {
        return WasKeyPressed(shakeKey) || (alsoAcceptNumpad1 && WasKeyPressed(KeyCode.Keypad1));
    }

    private static float Noise(float x, float y)
    {
        return Mathf.PerlinNoise(x, y) * 2f - 1f;
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
}
