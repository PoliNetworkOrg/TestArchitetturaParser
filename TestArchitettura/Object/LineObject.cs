using Newtonsoft.Json;

namespace TestArchitettura.Object;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class LineObject
{
    public string? Text;
    public double? Y;
}