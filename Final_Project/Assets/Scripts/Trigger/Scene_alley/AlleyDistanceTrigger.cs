using UnityEngine;

public class AlleyDistanceTrigger : MonoBehaviour
{
    public enum TriggerType { Monologue50m, Monologue100m, Monologue150m }
    
    [Header("이 박스가 터뜨릴 대사 종류를 골라주세요")]
    public TriggerType triggerType;

    private bool hasTriggered = false; 

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player") && AlleySceneController.Instance != null)
        {
            hasTriggered = true; 

            // 박스를 밟는 정확한 프레임에 기획한 대사를 매니저에게 요청합니다.
            switch (triggerType)
            {
                case TriggerType.Monologue50m:
                    AlleySceneController.Instance.Trigger50mMonologue();
                    break;
                case TriggerType.Monologue100m:
                    AlleySceneController.Instance.Trigger100mMonologue();
                    break;
                case TriggerType.Monologue150m:
                    AlleySceneController.Instance.Trigger150mMonologue();
                    break;
            }
        }
    }
}