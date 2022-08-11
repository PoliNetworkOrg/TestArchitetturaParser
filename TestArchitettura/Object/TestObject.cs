using Newtonsoft.Json;

namespace TestArchitettura.Object;

/// <summary>
/// Un intero test
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class TestObject
{
    private int _anno;

    public Dictionary<int, QuestionObject> Questions = new Dictionary<int, QuestionObject>();

    public TestObject(int anno)
    {
        this._anno = anno;
    }

    public void Add(QuestionObject questionObject)
    {
        var questionObjectNumber = questionObject.Number;
        if (questionObjectNumber == null)
            return;
        this.Questions[questionObjectNumber.Value] = questionObject;
    }
}