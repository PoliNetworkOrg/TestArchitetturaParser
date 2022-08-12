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

    public Dictionary<int, QuestionObject> Questions = new();

    public TestObject(int anno)
    {
        _anno = anno;
    }

    public void Add(QuestionObject questionObject)
    {
        var questionObjectNumber = questionObject.Number;
        if (questionObjectNumber == null)
            return;
        Questions[questionObjectNumber.Value] = questionObject;
    }
}