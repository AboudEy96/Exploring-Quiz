using System;
using UnityEngine;

public enum CubeState
{
    Locked,
    Unlocked,
    Solved
}

public class QuizCube : MonoBehaviour, IInteractable
{
    [SerializeField] private QuestionData question;
    [SerializeField] private QuizUI quizUI;

    [SerializeField] private CubeState startState =
        CubeState.Locked;

    public CubeState State { get; private set; }

    public bool CanInteract =>
        State == CubeState.Unlocked;

    public event Action<QuizCube> OnSolved;

    private void Awake()
    {
        State = startState;
    }

    public void Unlock()
    {
        if (State != CubeState.Locked)
            return;

        State = CubeState.Unlocked;

        Debug.Log(
            $"{gameObject.name} → UNLOCKED"
        );
    }

    public void Interact()
    {
        if (!CanInteract)
            return;

        if (quizUI == null)
        {
            Debug.LogError(
                $"{gameObject.name}: QuizUI is missing!"
            );

            return;
        }

        quizUI.Show(
            question,
            HandleAnswer
        );
    }

    private void HandleAnswer(bool correct)
    {
        if (!correct)
            return;

        State = CubeState.Solved;

        Debug.Log(
            $"{gameObject.name} SOLVED"
        );

        OnSolved?.Invoke(this);
    }

    public void ResetCube()
    {
        State = CubeState.Locked;

        Debug.Log(
            $"{gameObject.name} RESET"
        );
    }
}