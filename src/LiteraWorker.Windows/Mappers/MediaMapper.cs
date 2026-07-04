using System.Drawing.Printing;
using System.Text.Json;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Mappers;
using LiteraWorker.Core.Models;

namespace LiteraWorker.Windows.Mappers;

public static class MediaMapper
{
    private const double SizeToleranceMm = 1.5; // printers lie

    public static List<Media> MapMediaInJson(List<string> papersId)
    {
        using var stream = new FileStream("paper_sizes.json", FileMode.Open);
        var knownMedias = JsonSerializer.Deserialize(stream, JsonContext.Default.DictionaryStringMedia);

        var mappedMedia = new List<Media>();

        foreach (var id in papersId)
        {
            if (string.IsNullOrEmpty(id)) continue;

            if (knownMedias!.TryGetValue(id, out var media))
            {
                mappedMedia.Add(media);
            }

            // just skip if there is no match in the dictionary
            continue; 
        }

        return mappedMedia;
    }

    public static PaperSize MapToPaperSize(
        Media media,
        PrinterSettings printerSettings)
    {
        // Try exact PaperKind match by known sizes
        var byKind = TryMatchByPaperKind(media, printerSettings);
        if (byKind != null)
            return byKind;

        // Try dimension-based match
        var bySize = TryMatchByDimensions(media, printerSettings);
        if (bySize != null)
            return bySize;

        // Try name-based heuristics
        var byName = TryMatchByName(media, printerSettings);
        if (byName != null)
            return byName;

        // Fallback: custom size 
        return CreateCustomPaperSize(media);
    }

    private static PaperSize? TryMatchByPaperKind(
    Media media,
    PrinterSettings printerSettings)
    {
        var knownKind = MediaToPaperKind(media);
        if (knownKind == null)
            return null;

        return printerSettings.PaperSizes
            .Cast<PaperSize>()
            .FirstOrDefault(p => p.Kind == knownKind);
    }

    private static PaperKind? MediaToPaperKind(Media media)
    {
        var name = media.RawName.ToLowerInvariant();

        return name switch
        {
            var n when n.Contains("a4") => PaperKind.A4,
            var n when n.Contains("a3") => PaperKind.A3,
            var n when n.Contains("a5") => PaperKind.A5,
            var n when n.Contains("letter") => PaperKind.Letter,
            var n when n.Contains("legal") => PaperKind.Legal,
            _ => null
        };
    }

    private static PaperSize? TryMatchByDimensions(
    Media media,
    PrinterSettings printerSettings)
    {
        foreach (PaperSize ps in printerSettings.PaperSizes)
        {
            var widthMm = ps.Width * 25.4 / 100;
            var heightMm = ps.Height * 25.4 / 100;

            if (IsMatch(media, widthMm, heightMm))
                return ps;

            // orientation-agnostic match
            if (IsMatch(media, heightMm, widthMm))
                return ps;
        }

        return null;
    }

    private static bool IsMatch(Media media, double w, double h)
    {
        return Math.Abs(media.WidthMm - w) <= SizeToleranceMm
            && Math.Abs(media.HeightMm - h) <= SizeToleranceMm;
    }

    private static PaperSize? TryMatchByName(
    Media media,
    PrinterSettings printerSettings)
    {
        var mediaName = media.RawName.ToLowerInvariant();

        return printerSettings.PaperSizes
            .Cast<PaperSize>()
            .FirstOrDefault(ps =>
                ps.PaperName.ToLowerInvariant().Contains(mediaName) ||
                mediaName.Contains(ps.PaperName.ToLowerInvariant()));
    }

    private static PaperSize CreateCustomPaperSize(Media media)
    {
        return new PaperSize(
            media.DisplayName,
            MediaExtensions.MmToHundredthInch(media.WidthMm),
            MediaExtensions.MmToHundredthInch(media.HeightMm)
        );
    }
}