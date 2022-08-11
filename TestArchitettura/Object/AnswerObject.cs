using Newtonsoft.Json;

namespace TestArchitettura.Object;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class AnswerObject
{
    private string? _answerId;
    private bool _isCorrect = false;
    private string? _textAnswer;

    public void SetAsCorrect()
    {
        this._isCorrect = true;
    }

    public void SetAnswerText(string answer)
    {
        this._textAnswer = answer;
    }

    public void SetAnswerId(string? charId)
    {
        this._answerId = charId;
    }

    public void ExpandAnswer(string s)
    {
        var answer = this._textAnswer;
        if (answer != null) 
            this._textAnswer += " " + s.Trim();
    }
}