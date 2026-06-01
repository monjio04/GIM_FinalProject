using UnityEngine;

public enum DialogueType
{
    Normal,     // 일반 대사
    Monologue,  // 독백 (이 경우 스피커 이름을 숨기거나 '나'로 고정할 수 있음)
    Radio,      // 무전 음성
    Narration   // 나레이션 (타이틀 씬 등)
}


[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public DialogueType dialogueType = DialogueType.Normal;
    
    [Tooltip("체크하면 대사가 나올 때 플레이어 움직임을 멈춥니다. (오프닝이나 독백 시 키보드 조작 방지용)")]
    public bool freezePlayer = true; 

    [Tooltip("이 대사(문장)가 화면에 몇 초 동안 머물다 넘어갈지 설정합니다.")]
    public float autoDisplayTime = 3.0f; // 기본 3초 뒤 자동 소멸

    public string speaker;

    [TextArea(2, 5)]
    public string[] lines;
}