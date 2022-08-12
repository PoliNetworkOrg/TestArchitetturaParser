using Newtonsoft.Json;

namespace TestArchitettura.Object;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class QuestionObject
{
    public int? Number;
    private List<LineObject>? _questionText;
    public Dictionary<string, AnswerObject>? Answers = new();
    private string? _answerCorrect;


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
