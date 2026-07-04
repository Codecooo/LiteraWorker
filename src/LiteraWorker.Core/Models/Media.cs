namespace LiteraWorker.Core.Models;

public record Media(string RawName, string DisplayName, double WidthMm, double HeightMm);

public sealed class MediaComparer : IComparer<Media>
{
    public int Compare(Media? x, Media? y)
    {
        if (x is null) return 1;
        if (y is null) return -1;

        // If the name is A4 put them first to make sure A4 is always the default option.
        if (x.RawName.ToLower().Contains("a4") && !y.RawName.ToLower().Contains("a4"))
            return -1;
        if (!x.RawName.ToLower().Contains("a4") && y.RawName.ToLower().Contains("a4"))
            return 1;

        return x.DisplayName.CompareTo(y.DisplayName);
    }
}

