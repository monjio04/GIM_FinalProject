using UnityEngine;

public class GrandmaTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            ProjectManager.Instance.OnReachGrandmaTrigger(); // 첫 대사 시작
            gameObject.SetActive(false); // 다시 안 뜨게
        }
    }
}