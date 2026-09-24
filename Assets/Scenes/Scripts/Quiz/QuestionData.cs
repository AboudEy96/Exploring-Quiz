using UnityEngine;

[CreateAssetMenu(fileName = "NewQuestion", menuName = "Quiz/Question")]
public class QuestionData : ScriptableObject
{
    [TextArea(2, 5)] public string questionText;
    public string[] answers = new string[4];
    [Range(0, 3)] public int correctIndex;
}
