// See https://aka.ms/new-console-template for more information

using System.Net;
using TestArchitettura.Object;

Console.WriteLine("Hello, World!");

if (!Directory.Exists("pdf"))
{
    Directory.CreateDirectory("pdf");
}

var results = new Dictionary<int, TestObject>();

for (int i = 2007; i <= 2019; i++)
{
    Console.WriteLine("Starting "+ i.ToString());
    
    var path = "pdf/p" + i.ToString() + ".pdf";

    if (File.Exists(path) == false)
    {
        using var client = new WebClient();
        var url = "https://accessoprogrammato.miur.it/compiti/CompitoArchitettura" + i.ToString() + ".pdf";
        client.DownloadFile(url, path);
    }

    var r = TestArchitettura.Parser.MainParser.GetTestObject(path, i);
    results[i] = r;
}

var stringOut = Newtonsoft.Json.JsonConvert.SerializeObject(results);
File.WriteAllText("pdf/out.json",stringOut);
File.WriteAllText("./../../../out.json",stringOut);

Console.WriteLine("Done");