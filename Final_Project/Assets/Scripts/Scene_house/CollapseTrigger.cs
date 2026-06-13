using UnityEngine;

public class CollapseTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (ProjectManager.Instance != null) ProjectManager.Instance.OnReachCollapsePoint();
            gameObject.SetActive(false);
        }
    }
}