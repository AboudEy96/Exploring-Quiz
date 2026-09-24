using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button[] answerButtons;
    [SerializeField] private float closeDelay = 1f;

    public bool IsOpen { get; private set; }

    public event Action Opened;
    public event Action Closed;

    public event Action<bool> Answered;

    private void Start()
    {
        panel.SetActive(false);
    }

    public void Show(QuestionData question, Action<bool> onAnswered)
    {
        if (IsOpen) return;

        IsOpen = true;

        panel.SetActive(true);

        feedbackText.text = "";
        questionText.text = question.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            Button btn = answerButtons[i];
            btn.onClick.RemoveAllListeners();

            bool hasAnswer = i < question.answers.Length;

            btn.gameObject.SetActive(hasAnswer);

            if (!hasAnswer)
                continue;

            btn.interactable = true;

            TMP_Text buttonText = btn.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
                buttonText.text = question.answers[i];

            int index = i;

            btn.onClick.AddListener(() =>
                OnAnswerClicked(index, question, onAnswered)
            );
        }

        Opened?.Invoke();
    }

    private void OnAnswerClicked(
        int index,
        QuestionData question,
        Action<bool> onAnswered)
    {
        Debug.Log("Answer clicked: " + index);

        bool correct = index == question.correctIndex;

        // أخبر QuizCube
        onAnswered?.Invoke(correct);

        // أخبر QuizProgressionManager
        Answered?.Invoke(correct);

        if (correct)
        {
            feedbackText.text = "إجابة صحيحة!";

            SetButtonsInteractable(false);

            StartCoroutine(CloseAfterDelay());
        }
        else
        {
            feedbackText.text = "إجابة خاطئة، جرب مرة ثانية";

            // فقط الجواب الغلط يتعطل
            answerButtons[index].interactable = false;
        }
    }

    public void Close()
    {
        Debug.Log("QUIZ CLOSE CALLED");

        if (!IsOpen)
            return;

        StopAllCoroutines();

        panel.SetActive(false);

        IsOpen = false;

        Closed?.Invoke();
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSecondsRealtime(closeDelay);

        Close();
    }

    private void SetButtonsInteractable(bool value)
    {
        foreach (Button button in answerButtons)
        {
            button.interactable = value;
        }
    }
}