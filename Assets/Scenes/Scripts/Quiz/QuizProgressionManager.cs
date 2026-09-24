using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuizProgressionManager : MonoBehaviour
{
    [Serializable]
    private class Step
    {
        public QuizCube cube;
        public GameObject barrier;
    }

    [SerializeField] private List<Step> steps = new List<Step>();

    [Header("References")]
    [SerializeField] private QuizUI quizUI;

    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Rules")]
    [SerializeField] private int maxWrongAnswers = 3;

    private int currentScore;
    private int wrongAnswers;

    public event Action OnAllSolved;

    private void OnEnable()
    {
        if (quizUI != null)
            quizUI.Answered += HandleAnswer;
    }

    private void OnDisable()
    {
        if (quizUI != null)
            quizUI.Answered -= HandleAnswer;
    }

    private void Start()
    {
        for (int i = 0; i < steps.Count; i++)
        {
            int index = i;

            if (steps[i].cube != null)
            {
                steps[i].cube.OnSolved += _ => Advance(index);
            }
        }

        ResetQuiz();
    }

    private void HandleAnswer(bool correct)
    {
        if (correct)
            return;

        RegisterWrongAnswer();
    }

    private void RegisterWrongAnswer()
    {
        wrongAnswers++;

        Debug.Log(
            $"Wrong answer: {wrongAnswers}/{maxWrongAnswers}"
        );

        if (wrongAnswers >= maxWrongAnswers)
        {
            Debug.Log("3 wrong answers QUIZ RESET");

            ResetQuiz();
        }
    }

    private void Advance(int index)
    {
        currentScore++;

        UpdateScoreUI();

        Step step = steps[index];

        if (step.barrier != null)
            step.barrier.SetActive(false);

        int next = index + 1;

        if (next < steps.Count)
        {

            steps[next].cube.Unlock();

            Debug.Log(
                $"Next cube unlocked: {steps[next].cube.name}"
            );
        }
        else
        {
            FinishQuiz();
        }
    }

    private void FinishQuiz()
    {
        Debug.Log(
            $"QUIZ FINISHED! Score: {currentScore}/{steps.Count}"
        );

        if (scoreText != null)
        {
            scoreText.text =
                $"Score: {currentScore} / {steps.Count}";
        }

        OnAllSolved?.Invoke();
    }

    private void ResetQuiz()
    {
        currentScore = 0;
        wrongAnswers = 0;
        
        for (int i = 0; i < steps.Count; i++)
        {
            if (steps[i].cube != null)
            {
                steps[i].cube.ResetCube();
            }
            
            if (steps[i].barrier != null)
            {
                steps[i].barrier.SetActive(true);
            }
        }
        
        if (steps.Count > 0 && steps[0].cube != null)
        {
            steps[0].cube.Unlock();
        }

        UpdateScoreUI();

        Debug.Log("Quiz reset");
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text =
                $"Score: {currentScore} / {steps.Count}";
        }
    }
}