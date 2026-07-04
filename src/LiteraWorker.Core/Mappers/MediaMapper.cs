using System.Drawing.Printing;
using System.Text.Json;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Mappers;

public sealed class MediaMapper()
{
    public static List<Media>? MapMediaInJson(List<string> printerMedia)
    {
        using var stream = new FileStream("paper_sizes.json", FileMode.Open);
        List<Media>? knownMedias = JsonSerializer.Deserialize(stream, JsonContext.Default.ListMedia);
        var uniqueMedia = new HashSet<Media>();

        if (knownMedias is null) return null;

        foreach (var rawMedia in printerMedia)
        {
            var match = knownMedias.FirstOrDefault(m => m.RawName == rawMedia);

            if (match is not null)
            {
                uniqueMedia.Add(match);
                continue;
            }

            if (!rawMedia.StartsWith("custom")) continue;

            // if the media is custom (common in CUPS) try to extract the name and the height/width.

            try
            {
                var parts = rawMedia.Split('_'); // Split the raw name from underscore, example: "custom_197x243" 
                if (parts.Length < 2) continue;

                // Pick the size part from the split i.e the second element, example: "197x243"
                // and split them by x again to get the width/height 
                var sizePart = parts[1];
                var size = sizePart.Split('x');

                if (size.Length != 2) continue;

                static string Clean(string input)
                {
                    return new string(input.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
                }

                var widthStr = Clean(size[0]);
                var heightStr = Clean(size[1]);

                if (double.TryParse(widthStr, out double width) &&
                    double.TryParse(heightStr, out double height))
                {
                    uniqueMedia.Add(new Media
                    (
                        RawName: rawMedia,
                        DisplayName: $"Custom {width}x{height}",
                        WidthMm: width,
                        HeightMm: height
                    ));
                }
            }
            catch
            {
                continue;
            }
        }

        var list = uniqueMedia.ToList();
        list.Sort(new MediaComparer());

        return list;
    }
}

public static class MediaExtensions
{
    public static int MmToHundredthInch(double mm)
    => (int)Math.Round(mm / 25.4 * 100);

    public static List<Media> ToMediaList(this ICollection<Media> media)
    {
        var clientMedia = new List<Media>();
        foreach (var item in media)
        {
            var mediaItem = new Media
            (
                RawName: item.RawName,
                DisplayName: item.DisplayName,
                WidthMm: (double)item.WidthMm,
                HeightMm: (double)item.HeightMm
            );

            clientMedia.Add(mediaItem);
        }

        return clientMedia;
    }
}
