using Newtonsoft.Json;
using UglyToad.PdfPig.Content;

namespace TestArchitettura.Object;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class AnswerObject
{
    private string? _answerId;
    private bool _isCorrect = false;
    private string? _textAnswer;
    private List<string> _imagesPng64 = new List<string>();

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

    public void AddImage(IPdfImage image)
    {
        var imageString = Util.ImageUtil.GetPng64String(image);
        if (imageString != null) 
            this._imagesPng64.Add(imageString);
    }
}