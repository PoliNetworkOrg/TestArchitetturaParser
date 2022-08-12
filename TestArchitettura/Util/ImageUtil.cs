using UglyToad.PdfPig.Content;

namespace TestArchitettura.Util;

public static class ImageUtil
{
    public static string? GetPng64String(IPdfImage image)
    {
        return image.TryGetPng(out var png) ? Convert.ToBase64String(png) : null;
    }
}