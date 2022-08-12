#region

using Newtonsoft.Json;

#endregion

namespace TestArchitettura.Object;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class QuestionObject
{
    private string? _answerCorrect;
    private List<LineObject>? _questionText;
    public Dictionary<string, AnswerObject>? Answers = new();
    public int? Number;


    public void SetQuestion(List<LineObject> question)
    {
        _questionText = question;
    }

    public void SetAnswers(Dictionary<string, AnswerObject>? answerObjects)
    {
        Answers = answerObjects;
        _answerCorrect = "A";

        var dictionary = Answers;
        dictionary?[_answerCorrect].SetAsCorrect();
    }

    public void SetNumber(int? numberQuestion)
    {
        Number = numberQuestion;
    }
}