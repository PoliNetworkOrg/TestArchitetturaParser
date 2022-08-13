#region

using TestArchitettura.Object;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

#endregion

namespace TestArchitettura.Parser;

public static class MainParser
{
    private static TestObject GetTestObject(string pathPdf, int anno)
    {
        var document = PdfDocument.Open(pathPdf);
        var result = new TestObject(anno);

        for (var i = 0; i < document.NumberOfPages; i++)
        {
            var page = document.GetPage(i + 1);
            ParsePage(result, page, i + 1);
        }

        return result;
    }

    private static void ParsePage(TestObject result, Page page, int pageId)
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

        var lettersAtSameLeftDistance = new Dictionary<double, List<Letter>?>();
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
            if (lettersAtLeft != null && lettersAtLeft.Count != 0) break;

            leftsExcluded.Add(leftMin.Value);
        }

        var images = page.GetImages().ToList();
        var markedContents = page.GetMarkedContents().ToList();

        if (markedContents.Count > 0) Console.WriteLine(markedContents.Count);

        for (var i = 0; i < lettersAtLeft.Count; i++)
        {
            var questionObject =
                GetQuestion(lettersAtSameHeight, lettersAtLeft, i, images);

            questionObject.PageId = pageId;

            result.Add(questionObject);
        }
    }

    private static QuestionObject GetQuestion(Dictionary<double, List<Letter>> lettersAtSameHeight,
        IReadOnlyList<Letter> lettersAtLeft, int i, IEnumerable<IPdfImage> pdfImages)
    {
        var questionResult = new QuestionObject();
        var letterStart = lettersAtLeft[i];
        var letterEnd = i < lettersAtLeft.Count - 1 ? lettersAtLeft[i + 1] : null;
        var lettersAtSameHeightFiltered = lettersAtSameHeight
            .Where(l => IsIncluded(l.Key, letterStart, letterEnd)).ToList();
        var lines = lettersAtSameHeightFiltered
            .Select(l => GetLine(l.Value))
            .ToList();

        var list = lines.Where(x => !string.IsNullOrEmpty(x.Text?.Trim())).ToList();

        var imagesFiltered = pdfImages.Where(x => IsIncluded(x.Bounds.Top, letterStart, letterEnd)).ToList();
        AddAnswersAndQuestion(list, questionResult, imagesFiltered);

        return questionResult;
    }

    private static bool IsIncluded(double l, Letter letterStart, Letter? letterEnd)
    {
        return letterEnd == null
            ? l <= letterStart.Location.Y
            : l > letterEnd.Location.Y && l <= letterStart.Location.Y;
    }

    private static void AddAnswersAndQuestion(
        IReadOnlyList<LineObject> list,
        QuestionObject questionResult,
        IReadOnlyCollection<IPdfImage> imagesFiltered)
    {
        var numberQuestion = GetNumberQuestion(list);
        if (numberQuestion == null)
            return;

        var startAnswers = list.FirstOrDefault(x => x is { Text: { } } && x.Text.StartsWith("A)"), null);
        if (startAnswers != null && string.IsNullOrEmpty(startAnswers.Text))
            return;

        if (startAnswers != null)
        {
            var indexStartAnswers = Trova(list, startAnswers);
            if (indexStartAnswers < 0 || indexStartAnswers >= list.Count)
                return;

            var question = new List<LineObject>();
            for (var i = 0; i < indexStartAnswers; i++) question.Add(list[i]);

            var answers = new List<LineObject>();
            for (var i = indexStartAnswers; i < list.Count; i++) answers.Add(list[i]);

            question = FilterQuestion(question);
            questionResult.SetQuestion(question);

            var answerObjects = GetAnswers(answers, imagesFiltered);
            try
            {
                questionResult.SetAnswers(answerObjects);
            }
            catch
            {
                // ignored
            }
        }

        questionResult.SetNumber(numberQuestion);
    }

    private static int Trova(IReadOnlyList<LineObject> list, LineObject startAnswers)
    {
        for (var i = 0; i < list.Count; i++)
            if (list[i].Text == startAnswers.Text)
                return i;

        return -1;
    }


    private static List<LineObject> FilterQuestion(List<LineObject> question)
    {
        var indexList = question.Where(x => x.Text != null && x.Text.Contains(". ")).ToList();
        if (indexList.Count == 0)
            return question;

        var index = question.IndexOf(indexList.First());
        if (index < 0 || index >= question.Count)
            return question;

        var result = new List<LineObject>();
        for (var i = index; i < question.Count; i++) result.Add(question[i]);

        return result;
    }

    private static int? GetNumberQuestion(IEnumerable<LineObject> list)
    {
        var first = list.First().Text?.Trim();
        var x = first?.Split('.');
        try
        {
            if (x != null)
                return Convert.ToInt32(x[0]);
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static Dictionary<string, AnswerObject> GetAnswers(
        IReadOnlyList<LineObject> answers,
        IReadOnlyCollection<IPdfImage> imagesFiltered
    )
    {
        var result = new Dictionary<string, AnswerObject>();
        string? lastDone = null;

        for (var i = 0; i < answers.Count; i++)
        {
            var x = answers[i];
            var trim = x.Text?.Trim();
            if (trim == null)
                continue;

            var index = trim.IndexOf(")", StringComparison.Ordinal);
            if (index < 0 || (index >= trim.Length && index < 3))
            {
                if (lastDone != null)
                    result[lastDone].ExpandAnswer(trim);
            }
            else
            {
                var charId = trim[..index].Trim();
                var answer = trim[(index + 1)..].Trim();
                result[charId] = new AnswerObject();
                result[charId].SetAnswerText(answer);
                result[charId].SetAnswerId(charId);
                lastDone = charId;
            }


            var pdfImages = imagesFiltered.Where(image => ImageIsInAnswer(answers, i, image.Bounds.Top))
                .ToList();

            if (lastDone == null)
                continue;

            var answerObject = result[lastDone];
            foreach (var image in pdfImages) answerObject.AddImage(image);
        }

        return result;
    }

    private static bool ImageIsInAnswer(IReadOnlyList<LineObject> answers, int i, double imageY)
    {
        var x = answers[i];

        var answersCountPrevious = answers.Count - 1;
        if (i == answersCountPrevious) return x.Y >= imageY;

        var x2 = answers[i + 1];


        var imageIsInAnswer = x.Y >= imageY && x2.Y <= imageY;
        return imageIsInAnswer;
    }


    private static LineObject GetLine(IEnumerable<Letter> lValue)
    {
        var enumerable = lValue.ToList();
        var s = enumerable.Aggregate("", (current, x) => current + x.Value);

        var text = s.Trim();

        var result = new LineObject
        {
            Text = text,
            Y = enumerable.Any() ? enumerable.First().Location.Y : null
        };

        return result;
    }

    public static async Task<Dictionary<int, TestObject>> GetResults()
    {
        var testObjects = new Dictionary<int, TestObject>();
        for (var i = 2007; i <= DateTime.Now.Year; i++)
            try
            {
                Console.WriteLine("Starting " + i);

                var path = "pdf/p" + i + ".pdf";

                if (File.Exists(path) == false)
                {
                    var url = "https://accessoprogrammato.miur.it/compiti/CompitoArchitettura" + i + ".pdf";
                    await DownloadFile(url, path);
                }

                var r = GetTestObject(path, i);
                testObjects[i] = r;
            }
            catch (Exception e)
            {
                Console.WriteLine("Fallito " + i + " " + e.Message);
            }

        return testObjects;
    }

    private static async Task DownloadFile(string url, string path)
    {
        var client = new HttpClient();
        var streamContent = new StreamContent(new MemoryStream());
        var newResponse = await client.PostAsync(url, streamContent);

        var content = newResponse.Content;
        // actually a System.Net.Http.StreamContent instance but you do not need to cast as the actual type does not matter in this case

        await using var file = File.Create(path);
        // create a new file to write to
        var contentStream = await content.ReadAsStreamAsync(); // get the actual content stream
        await contentStream.CopyToAsync(file); // copy that stream to the file stream
    }
}