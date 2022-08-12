#region

using Newtonsoft.Json;
using TestArchitettura.Util;
using UglyToad.PdfPig.Content;

#endregion

namespace TestArchitettura.Object;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class AnswerObject
{
    private string? _answerId;
    private List<string> _imagesPng64 = new();
    private bool _isCorrect;
    private string? _textAnswer;

    public void SetAsCorrect()
    {
        _isCorrect = true;
    }

    public void SetAnswerText(string answer)
    {
        _textAnswer = answer;
    }

    public void SetAnswerId(string? charId)
    {
        _answerId = charId;
    }

    public void ExpandAnswer(string s)
    {
        var answer = _textAnswer;
        if (answer != null)
            _textAnswer += " " + s.Trim();
    }

    public void AddImage(IPdfImage image)
    {
        var imageString = ImageUtil.GetPng64String(image);
        if (imageString != null)
            _imagesPng64.Add(imageString);
    }
}