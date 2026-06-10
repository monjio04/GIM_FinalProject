using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    private Light myLight;
    public float minIntensity = 1.0f;
    public float maxIntensity = 3.0f;
    public float speed = 10f;

    void Start() => myLight = GetComponent<Light>();

    void Update()
    {
        // 펄린 노이즈를 이용해 자연스러운 불꽃 깜빡임 연출
        myLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PerlinNoise(Time.time * speed, 0.0f));
    }
}