#region

using Newtonsoft.Json;
using TestArchitettura.Parser;

#endregion

Console.WriteLine("Hello, World!");

if (!Directory.Exists("pdf")) Directory.CreateDirectory("pdf");


var results = MainParser.GetResults();



var stringOut = JsonConvert.SerializeObject(results);
File.WriteAllText("pdf/out.json", stringOut);
File.WriteAllText("./../../../out.json", stringOut);

Console.WriteLine("Done");