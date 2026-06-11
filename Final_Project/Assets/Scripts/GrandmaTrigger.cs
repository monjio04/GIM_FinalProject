using UnityEngine;

public class GrandmaTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 덫을 밟은 게 플레이어(Player 태그)라면
        if (other.CompareTag("Player"))
        {
            // ★ 핵심: ProjectManager에게 밖으로 나왔다고 진짜로 보고합니다!
            if (ProjectManager.Instance != null)
            {
                ProjectManager.Instance.OnPlayerExitBuilding();
            }

            // 이벤트가 두 번 터지지 않도록 덫을 파괴(비활성화)합니다.
            gameObject.SetActive(false);
        }
    }
}