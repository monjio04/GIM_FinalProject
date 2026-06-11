using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TalkManager : MonoBehaviour
{
    public static TalkManager Instance;

    public GameObject talkUI;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
 
    [Header("타자기 설정")]
    [Tooltip("글자가 찍히는 속도입니다. 수치가 작을수록 빨라집니다.")]
    public float typeSpeed = 0.08f; 

    private DialogueData currentDialogue;
    private int currentIndex;
    private bool isTalking;
    private bool isSequenceActive = false; // 시퀀스 실행 여부 확인용 플래그
    private Action onDialogueEndCallback;

    public bool IsTalking => isTalking;
    public bool ShouldFreezePlayer => isTalking && currentDialogue != null && currentDialogue.freezePlayer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartDialogue(DialogueData dialogue, Action onEndCallback = null)
    {
        // 시퀀스 실행 중이 아닐 때만 기존 코루틴을 중단 (대화 끊김 방지)
        if (!isSequenceActive)
        {
            StopAllCoroutines();
        }

        currentDialogue = dialogue;
        currentIndex = 0;
        onDialogueEndCallback = onEndCallback;

        isTalking = true; 

        talkUI.SetActive(true);
        talkUI.transform.SetAsLastSibling(); 

        if (dialogue.dialogueType == DialogueType.Monologue || dialogue.dialogueType == DialogueType.Narration)
        {
            speakerText.text = "";
        }
        else
        {
            speakerText.text = $"[{dialogue.speaker}]"; 
        }

        StartCoroutine(DisplayDialogueRoutine());
    }

    IEnumerator DisplayDialogueRoutine()
    {
        while (currentIndex < currentDialogue.lines.Length)
        {
            string line = currentDialogue.lines[currentIndex];

            if (line.Contains("//"))
            {
                string[] parts = line.Split(new string[] { "//" }, StringSplitOptions.None);
                for (int i = 0; i < parts.Length; i++)
                {
                    talkUI.SetActive(true);
                    yield return StartCoroutine(TypeTextRoutine(parts[i].Trim()));
                    yield return new WaitForSeconds(currentDialogue.autoDisplayTime);

                    if (i < parts.Length - 1)
                    {
                        talkUI.SetActive(false);
                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }
            else
            {
                yield return StartCoroutine(TypeTextRoutine(line));
                yield return new WaitForSeconds(currentDialogue.autoDisplayTime);
            }
            currentIndex++;
        }
        EndDialogue();
    }

    IEnumerator TypeTextRoutine(string line)
    {
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0; 
        int totalCharacters = line.Length;

        for (int i = 0; i <= totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i; 
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    void EndDialogue()
    {
        isTalking = false;
        
        // 시퀀스 진행 중이 아니면 UI를 끔
        if (!isSequenceActive)
        {
            talkUI.SetActive(false);
        }

        if (onDialogueEndCallback != null)
        {
            onDialogueEndCallback.Invoke();
            onDialogueEndCallback = null;
        }
    }

    public void StartDialogueSequence(DialogueData[] dialogues, Action onEndCallback = null)
    {
        isSequenceActive = true; 
        StartCoroutine(DialogueSequenceRoutine(dialogues, onEndCallback));
    }

    IEnumerator DialogueSequenceRoutine(DialogueData[] dialogues, Action onEndCallback)
    {
        foreach (var d in dialogues)
        {
            if (d == null) continue;

            bool isDone = false;
            StartDialogue(d, () => { isDone = true; });
            yield return new WaitUntil(() => isDone);
            yield return new WaitForSeconds(0.2f);
        }
        
        isSequenceActive = false; // 시퀀스 종료
        talkUI.SetActive(false);  // UI 확실히 끄기
        onEndCallback?.Invoke();
    }
}