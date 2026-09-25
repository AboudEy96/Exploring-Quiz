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

    [Header("Timing")]
    [SerializeField] private float closeDelay = 1f;
    [SerializeField] private float loseDelay = 1.5f;

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
        if (IsOpen)
            return;

        IsOpen = true;

        panel.SetActive(true);

        feedbackText.text = "";
        questionText.text = question.questionText;

        SetupButtons(question, onAnswered);

        Opened?.Invoke();
    }

    private void SetupButtons(
        QuestionData question,
        Action<bool> onAnswered)
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            Button btn = answerButtons[i];

            btn.onClick.RemoveAllListeners();

            bool hasAnswer = i < question.answers.Length;

            btn.gameObject.SetActive(hasAnswer);

            if (!hasAnswer)
                continue;

            btn.interactable = true;

            TMP_Text buttonText =
                btn.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
                buttonText.text = question.answers[i];

            int index = i;

            btn.onClick.AddListener(() =>
            {
                OnAnswerClicked(
                    index,
                    question,
                    onAnswered
                );
            });
        }
    }

    private void OnAnswerClicked(
        int index,
        QuestionData question,
        Action<bool> onAnswered)
    {
        if (!IsOpen)
            return;

        bool correct = index == question.correctIndex;

        Debug.Log(
            $"Answer: {index} | Correct: {correct}"
        );
        
        onAnswered?.Invoke(correct);
        Answered?.Invoke(correct);

        if (correct)
        {
            feedbackText.text = "ﻞﻄﺒﻟﺍ ﺎﻬﻳﺍ ﺖﻨﺴﺣﺍ!";

            SetButtonsInteractable(false);

            StartCoroutine(CloseAfterDelay());
        }
        else
        {
            
            answerButtons[index].interactable = false;
        }
    }
    
    public void ShowLose()
    {
        if (!IsOpen)
            return;

        StopAllCoroutines();
        

        SetButtonsInteractable(false);

        StartCoroutine(LoseSequence());
    }

    private IEnumerator LoseSequence()
    {
        yield return new WaitForSecondsRealtime(loseDelay);

        Close();
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSecondsRealtime(closeDelay);

        Close();
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        StopAllCoroutines();

        panel.SetActive(false);

        IsOpen = false;

        feedbackText.text = "";

        Closed?.Invoke();
    }

    private void SetButtonsInteractable(bool value)
    {
        foreach (Button button in answerButtons)
        {
            button.interactable = value;
        }
    }
}