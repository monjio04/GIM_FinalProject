using UnityEngine;

public class InsideExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (ProjectManager.Instance != null)
            {
                ProjectManager.Instance.OnReachInsideExit();
            }
            gameObject.SetActive(false); // 한 번 밟으면 꺼짐
        }
    }
}