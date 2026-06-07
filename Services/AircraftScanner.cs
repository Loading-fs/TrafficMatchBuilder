using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TrafficMatchBuilder.Models;

namespace TrafficMatchBuilder.Services;

public sealed class AircraftScanner
{
    private static readonly Regex FltsimSectionRegex =
        new(@"^\s*\[fltsim(?:\.\d+)?\]\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex GeneralSectionRegex =
        new(@"^\s*\[general\]\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex KnownTypeRegex =
        new(@"\b(A20N|A21N|A318|A319|A320|A321|A332|A333|A339|A343|A346|A359|A35K|A388|B736|B737|B738|B739|B38M|B39M|B752|B753|B763|B764|B772|B77L|B77W|B788|B789|B78X|C172|C208|DA40|DA42|DH8D|E170|E175|E190|E195|CRJ7|CRJ9|AT72)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public ScanResult Scan(string rootFolder)
    {
        if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder))
            throw new DirectoryNotFoundException("The selected folder does not exist.");

        var result = new ScanResult();

        var aircraftCfgFiles = Directory
            .EnumerateFiles(rootFolder, "aircraft.cfg", SearchOption.AllDirectories)
            .ToList();

        result.AircraftCfgFiles = aircraftCfgFiles.Count;

        foreach (var cfgFile in aircraftCfgFiles)
        {
            foreach (var entry in ParseAircraftCfg(cfgFile))
                result.Entries.Add(entry);
        }

        return result;
    }

    private static IEnumerable<AircraftEntry> ParseAircraftCfg(string cfgFile)
    {
        string[] lines;

        try
        {
            lines = File.ReadAllLines(cfgFile);
        }
        catch
        {
            yield break;
        }

        var entries = new List<AircraftEntry>();
        AircraftEntry? currentEntry = null;
        var inGeneralSection = false;
        var generalTypeCode = "";

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                continue;

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                if (currentEntry is not null && !string.IsNullOrWhiteSpace(currentEntry.Title))
                    entries.Add(currentEntry);

                currentEntry = null;
                inGeneralSection = GeneralSectionRegex.IsMatch(line);

                if (FltsimSectionRegex.IsMatch(line))
                {
                    inGeneralSection = false;
                    currentEntry = new AircraftEntry { SourceFile = cfgFile };
                }

                continue;
            }

            var separatorIndex = line.IndexOf('=');

            if (separatorIndex <= 0)
                continue;

            var key = line[..separatorIndex].Trim().ToLowerInvariant();
            var value = CleanValue(line[(separatorIndex + 1)..].Trim());

            if (inGeneralSection)
            {
                if (key == "icao_type_designator" && !string.IsNullOrWhiteSpace(value))
                    generalTypeCode = value.ToUpperInvariant();

                continue;
            }

            if (currentEntry is null)
                continue;

            switch (key)
            {
                case "title":
                    currentEntry.Title = value;
                    break;

                case "icao_type_designator":
                    currentEntry.TypeCode = value.ToUpperInvariant();
                    break;

                case "icao_airline":
                    currentEntry.AirlineCode = value.ToUpperInvariant();
                    break;
            }
        }

        if (currentEntry is not null && !string.IsNullOrWhiteSpace(currentEntry.Title))
            entries.Add(currentEntry);

        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.TypeCode))
                entry.TypeCode = generalTypeCode;

            if (string.IsNullOrWhiteSpace(entry.TypeCode))
                entry.TypeCode = InferTypeCode(entry.Title, cfgFile);

            yield return entry;
        }
    }

    private static string InferTypeCode(string title, string cfgFile)
    {
        var combined = $"{title} {cfgFile.Replace(Path.DirectorySeparatorChar, ' ')}";
        var match = KnownTypeRegex.Match(combined);

        if (!match.Success)
            return "";

        var type = match.Value.ToUpperInvariant();

        return type switch
        {
            "B737" => "B738",
            _ => type
        };
    }

    private static string CleanValue(string value)
    {
        var commentIndex = value.IndexOf(';');

        if (commentIndex >= 0)
            value = value[..commentIndex];

        return value.Trim().Trim('"');
    }
}
