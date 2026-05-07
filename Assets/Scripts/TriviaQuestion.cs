using UnityEngine;

/// <summary>
/// Serializable trivia question data model.
/// Loaded from questions.json via JsonUtility.
/// </summary>
[System.Serializable]
public class TriviaQuestion
{
    public string question;
    public string[] answers;
    public int correctIndex;
    public string category;
    public int difficulty;
}

/// <summary>
/// Wrapper for JSON deserialization of the question array.
/// </summary>
[System.Serializable]
public class QuestionList
{
    public TriviaQuestion[] questions;
}
