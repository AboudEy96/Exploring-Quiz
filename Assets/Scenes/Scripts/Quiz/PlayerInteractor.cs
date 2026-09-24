using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private QuizUI quizUI;
    [SerializeField] private GameObject interactPrompt; // اختياري: نص "اضغط E"
    [SerializeField] private float interactRadius = 1.5f;
    [SerializeField] private LayerMask interactMask = ~0;
    private readonly List<IInteractable> nearby = new List<IInteractable>();

    private void OnEnable()
    {
        quizUI.Opened += LockPlayer;
        quizUI.Closed += UnlockPlayer;
    }

    private void OnDisable()
    {
        quizUI.Opened -= LockPlayer;
        quizUI.Closed -= UnlockPlayer;
    }

    private void Update()
    {
        IInteractable target = GetTarget();
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
              Debug.Log("E pressed | nearby=" + nearby.Count + " | target=" + (target != null));

        if (interactPrompt != null)
            interactPrompt.SetActive(target != null);

        if (target == null) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            target.Interact();
    }

    private IInteractable GetTarget()
    {
        if (quizUI.IsOpen) return null;

        Collider[] hits = Physics.OverlapSphere(
            transform.position, interactRadius, interactMask, QueryTriggerInteraction.Collide);

        foreach (Collider c in hits)
        {
            IInteractable i = c.GetComponentInParent<IInteractable>();
            if (i != null && i.CanInteract) return i;
        }
        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponentInParent<IInteractable>();
        Debug.Log("Trigger enter: " + other.name + " | interactable=" + (interactable != null));

        if (interactable != null && !nearby.Contains(interactable))
            nearby.Add(interactable);
        
        
    }
    

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponentInParent<IInteractable>();
        if (interactable != null)
            nearby.Remove(interactable);
    }

    private void LockPlayer()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void UnlockPlayer()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }
}