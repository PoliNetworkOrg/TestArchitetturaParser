using Newtonsoft.Json;

namespace TestArchitettura.Object;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class QuestionObject
{
    public int? Number;
    private List<string>? _questionText;
    public Dictionary<string, AnswerObject>? Answers = new Dictionary<string, AnswerObject>();
    private string? _answerCorrect;


    public void SetQuestion(List<string> question)
    {
        this._questionText = question;
    }

    public void SetAnswers(Dictionary<string, AnswerObject>? answerObjects)
    {
        this.Answers = answerObjects;
        this._answerCorrect = "A";

        var dictionary = this.Answers;
        dictionary?[this._answerCorrect].SetAsCorrect();
    }
    
    public void SetNumber(int? numberQuestion)
    {
        this.Number = numberQuestion;
    }

}
