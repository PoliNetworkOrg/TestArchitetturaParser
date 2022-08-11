using TestArchitettura.Object;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace TestArchitettura.Parser;



public static class MainParser
{
    public static TestObject GetTestObject(string pathPdf, int anno)
    {
        var document = PdfDocument.Open(pathPdf);
        var result =  new TestObject(anno);

        for (var i = 0; i < document.NumberOfPages; i++)
        {
            var page = document.GetPage(i + 1);
            ParsePage(result, page);
        }

        return result;
    }

    private static void ParsePage(TestObject result, Page page)
    {
        var lettersAtSameHeight = new Dictionary<double, List<Letter>>();
        foreach (var letter in page.Letters)
        {
            if (letter == null)
                continue;

            var height = letter.Location.Y;
            if (lettersAtSameHeight.ContainsKey(height) == false)
                lettersAtSameHeight[height] = new List<Letter>();

            lettersAtSameHeight[height].Add(letter);
        }

        var lettersAtSameLeftDistance =  new Dictionary<double, List<Letter>?>();
        foreach (var letter in page.Letters)
        {
            if (letter == null)
                continue;

            var left = letter.Location.X;
            if (lettersAtSameLeftDistance.ContainsKey(left) == false)
                lettersAtSameLeftDistance[left] = new List<Letter>();
            lettersAtSameLeftDistance[left] ??= new List<Letter>();
            
            lettersAtSameLeftDistance[left]?.Add(letter);
        }

        List<Letter>? lettersAtLeft = null;
        var leftsExcluded = new List<double>();
        while (true)
        {
            var enumerable = lettersAtSameLeftDistance.Keys.ToList()
                .Where(x => !leftsExcluded.Contains(x));
            var doubles = enumerable.ToList();
            if (!doubles.Any())
                return;
            double? leftMin = doubles.Min();
            var letters = lettersAtSameLeftDistance[leftMin.Value];
            if (letters != null)
                lettersAtLeft = (List<Letter>?)letters.Where(x => !string.IsNullOrEmpty(x.Value.Trim())).ToList();
            if (lettersAtLeft != null && lettersAtLeft.Count != 0)
            {
                break;
            }
            else
            {
                leftsExcluded.Add(leftMin.Value);
            }
        }

        for (var i = 0; i < lettersAtLeft.Count; i++)
        {
            var questionObject =
                GetQuestion(lettersAtSameHeight, lettersAtLeft, i);

            result.Add(questionObject);
        }
        
    }

    private static QuestionObject GetQuestion(Dictionary<double,
        List<Letter>> lettersAtSameHeight,
        IReadOnlyList<Letter> lettersAtLeft, int i)
    {
        var questionResult = new QuestionObject();
        var letterStart = lettersAtLeft[i];
        var letterEnd = i < lettersAtLeft.Count - 1 ? lettersAtLeft[i + 1] : null;
        var lettersAtSameHeightFiltered = lettersAtSameHeight
            .Where(l => letterEnd == null
                ? l.Key <= letterStart.Location.Y
                : (l.Key > letterEnd.Location.Y) && l.Key <= letterStart.Location.Y)
            .ToList();
        var lines = lettersAtSameHeightFiltered
            .Select(l => GetLine(l.Value))
            .ToList();

        var list = lines.Where(x => !string.IsNullOrEmpty(x.Trim())).ToList();

        AddAnswersAndQuestion(list, questionResult);
        
        return questionResult;
    }

    private static void AddAnswersAndQuestion(List<string> list, QuestionObject questionResult)
    {
        var numberQuestion = GetNumberQuestion(list);
        if (numberQuestion == null)
            return;
        
        
        var startAnswers = list.FirstOrDefault(x => x.StartsWith("A)"), string.Empty);
        if (string.IsNullOrEmpty(startAnswers))
            return;
        
        var indexStartAnswers = list.IndexOf(startAnswers);
        if (indexStartAnswers < 0 || indexStartAnswers >= list.Count)
            return;

        var question = new List<string>();
        for (var i = 0; i < indexStartAnswers; i++)
        {
            question.Add(list[i]);
        }

        var answers = new List<string>();
        for (var i = indexStartAnswers; i < list.Count; i++)
        {
            answers.Add(list[i]);
        }

        question = FilterQuestion(question);
        questionResult.SetQuestion(question);

        ;
        var answerObjects = GetAnswers(answers);
        ;
        try
        {
            questionResult.SetAnswers(answerObjects);
        }
        catch
        {
            ;
        }

        ;
        questionResult.SetNumber(numberQuestion);
    }

    private static List<string> FilterQuestion(List<string> question)
    {
        var indexList = question.Where(x => x.Contains(". ")).ToList();
        if (indexList.Count == 0)
            return question;

        var index = question.IndexOf(indexList.First());
        if (index < 0 || index >= question.Count)
            return question;

        var result = new List<string>();
        for (var i = index; i < question.Count; i++)
        {
            result.Add(question[i]);
        }

        return result;
    }

    private static int? GetNumberQuestion(List<string> list)
    {
        ;
        string first = list.First().Trim();
        var x = first.Split('.');
        try
        {
            return Convert.ToInt32(x[0]);
        }
        catch
        {
            ;
        }

        return null;
    }

    private static Dictionary<string, AnswerObject> GetAnswers(List<string> answers)
    {
        ;

        var result = new Dictionary<string, AnswerObject>();
        string? lastDone = null;
        
        foreach (var x in answers)
        {
            var trim = x.Trim();
            var index = trim.IndexOf(")", StringComparison.Ordinal);
            if (index < 0 || index >= trim.Length && index < 3)
            {
                if (lastDone != null) 
                    result[lastDone].ExpandAnswer(trim);
            }
            else
            {
                var charId = trim[0..index].Trim();
                var answer = trim[(index + 1)..].Trim();
                result[charId] = new AnswerObject();
                result[charId].SetAnswerText(answer);
                result[charId].SetAnswerId(charId);
                lastDone = charId;
            }
        }
        return result;
    }

    private static string GetLine(IEnumerable<Letter> lValue)
    {
        var s = lValue.Aggregate("", (current, x) => current + x.Value);

        return s.Trim();
    }
}