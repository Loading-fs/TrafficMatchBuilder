using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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

    private static readonly string[] KnownBasePackageHints =
    {
        "pmdg-aircraft-738", "pmdg-aircraft-737", "pmdg-aircraft-739", "pmdg-aircraft-77w",
        "fnx-aircraft-320", "fnx-aircraft-319", "fnx-aircraft-321", "fenix", "fnx"
    };

    private static readonly string[] KnownLiveryPackageHints =
    {
        "pmdg-aircraft-738-liveries", "pmdg-aircraft-737-liveries", "pmdg-aircraft-739-liveries", "pmdg-aircraft-77w-liveries",
        "liveries", "livery"
    };

    public ScanResult Scan(string rootFolder) => Scan(rootFolder, null);

    public ScanResult Scan(string rootFolder, string? baseAircraftFolder)
    {
        if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder))
            throw new DirectoryNotFoundException("The selected folder does not exist.");

        var result = new ScanResult();

        var discovery = DiscoverRelatedFolders(rootFolder, baseAircraftFolder);
        result.AutoDiscoveredBaseFolders.AddRange(discovery.BaseFolders);
        result.AutoDiscoveredLiveryFolders.AddRange(discovery.LiveryFolders);
        result.DiscoveryNotes.AddRange(discovery.Notes);

        var allAircraftCfgFiles = new List<string>();
        AddUniqueFiles(allAircraftCfgFiles, SafeEnumerateFiles(rootFolder, "aircraft.cfg"));

        if (!string.IsNullOrWhiteSpace(baseAircraftFolder) && Directory.Exists(baseAircraftFolder) && !PathsEqual(rootFolder, baseAircraftFolder))
            AddUniqueFiles(allAircraftCfgFiles, SafeEnumerateFiles(baseAircraftFolder, "aircraft.cfg"));

        foreach (var baseFolder in discovery.BaseFolders)
            AddUniqueFiles(allAircraftCfgFiles, SafeEnumerateFiles(baseFolder, "aircraft.cfg"));

        var cfgEntries = new List<AircraftEntry>();
        var usableAircraftCfgFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var cfgFile in allAircraftCfgFiles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var parsedEntries = ParseAircraftCfg(cfgFile).ToList();
            if (parsedEntries.Count > 0)
                usableAircraftCfgFiles.Add(cfgFile);

            foreach (var entry in parsedEntries)
            {
                cfgEntries.Add(entry);
                // Keep classic traffic libraries exportable, but avoid duplicating base-aircraft entries in the report when
                // they were only auto-discovered to resolve addon livery names.
                if (IsInside(cfgFile, rootFolder))
                    result.Entries.Add(entry);
            }
        }

        // Count only aircraft.cfg files that actually contain [FLTSIM.x] entries.
        // Fenix/PMDG packages may contain unrelated config files named aircraft.cfg.
        result.AircraftCfgFiles = usableAircraftCfgFiles.Count;

        var liveryRoots = new List<string>();
        AddUniqueFolders(liveryRoots, FindAddonLiveryRoots(rootFolder));
        foreach (var liveryFolder in discovery.LiveryFolders)
            AddUniqueFolders(liveryRoots, FindAddonLiveryRoots(liveryFolder));

        result.AddonLiveryFolders = liveryRoots.Distinct(StringComparer.OrdinalIgnoreCase).Count();

        foreach (var liveryRoot in liveryRoots.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var entry = ParseAddonLivery(liveryRoot, rootFolder, cfgEntries, allAircraftCfgFiles);
            if (entry is not null)
                result.Entries.Add(entry);
        }

        return result;
    }

    private static (List<string> BaseFolders, List<string> LiveryFolders, List<string> Notes) DiscoverRelatedFolders(string rootFolder, string? manualBaseFolder)
    {
        var baseFolders = new List<string>();
        var liveryFolders = new List<string>();
        var notes = new List<string>();

        if (!string.IsNullOrWhiteSpace(manualBaseFolder) && Directory.Exists(manualBaseFolder))
            AddUniqueFolders(baseFolders, new[] { manualBaseFolder });

        var broadScan = IsBroadScanRoot(rootFolder);
        var selectedPackageName = GetSelectedPackageName(rootFolder);
        var expectedBaseNames = GetExpectedBasePackageNames(selectedPackageName)
            .Concat(GetExpectedBasePackageNamesFromContent(rootFolder))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // If the selected folder itself contains addon livery metadata, treat only this package as a livery package.
        // Do not pull in every unrelated Fenix/PMDG livery package from Community unless the user selected Community/Packages.
        if (FindAddonLiveryRoots(rootFolder).Any())
            AddUniqueFolders(liveryFolders, new[] { rootFolder });

        var packageRoots = FindPackageRoots(rootFolder).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        foreach (var packageRoot in packageRoots)
        {
            if (!Directory.Exists(packageRoot))
                continue;

            if (broadScan)
            {
                foreach (var packageFolder in SafeEnumerateTopDirectories(packageRoot))
                {
                    var name = Path.GetFileName(packageFolder).ToLowerInvariant();
                    if (LooksLikeKnownLiveryPackage(name))
                        AddUniqueFolders(liveryFolders, new[] { packageFolder });
                    else if (LooksLikeKnownBasePackage(name))
                        AddUniqueFolders(baseFolders, new[] { packageFolder });
                }
            }
            else
            {
                foreach (var expectedBaseName in expectedBaseNames)
                {
                    var expected = Path.Combine(packageRoot, expectedBaseName);
                    if (Directory.Exists(expected))
                        AddUniqueFolders(baseFolders, new[] { expected });
                }
            }
        }

        // Also try the user's default MSFS package paths. For a narrow scan, only use the matching base packages.
        foreach (var defaultRoot in GetDefaultMsfsPackageRoots())
        {
            if (!Directory.Exists(defaultRoot))
                continue;

            if (broadScan)
            {
                foreach (var packageFolder in SafeEnumerateTopDirectories(defaultRoot))
                {
                    var name = Path.GetFileName(packageFolder).ToLowerInvariant();
                    if (LooksLikeKnownLiveryPackage(name))
                        AddUniqueFolders(liveryFolders, new[] { packageFolder });
                    else if (LooksLikeKnownBasePackage(name))
                        AddUniqueFolders(baseFolders, new[] { packageFolder });
                }
            }
            else
            {
                foreach (var expectedBaseName in expectedBaseNames)
                {
                    var expected = Path.Combine(defaultRoot, expectedBaseName);
                    if (Directory.Exists(expected))
                        AddUniqueFolders(baseFolders, new[] { expected });
                }
            }
        }

        // Put exact base packages before support/effect folders, so the report does not show zGFX/Fenix effects as the base aircraft.
        baseFolders = baseFolders
            .OrderByDescending(HasAircraftCfg)
            .ThenByDescending(folder => LooksLikeKnownBasePackage(Path.GetFileName(folder).ToLowerInvariant()))
            .ThenBy(folder => folder.Length)
            .ToList();

        if (baseFolders.Count > 0)
            notes.Add($"Auto-discovered base aircraft folders: {baseFolders.Count}");
        if (liveryFolders.Count > 0)
            notes.Add($"Auto-discovered addon livery folders: {liveryFolders.Count}");
        if (!broadScan && liveryFolders.Count > 0)
            notes.Add("Narrow addon scan: only the selected livery package and matching base aircraft packages were scanned.");

        return (baseFolders, liveryFolders, notes);
    }

    private static bool IsBroadScanRoot(string rootFolder)
    {
        var name = Path.GetFileName(rootFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        return name.Equals("Community", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Official", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Packages", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetSelectedPackageName(string rootFolder)
    {
        var selected = new DirectoryInfo(rootFolder);
        while (selected is not null)
        {
            var parentName = selected.Parent?.Name ?? "";
            if (parentName.Equals("Community", StringComparison.OrdinalIgnoreCase) ||
                parentName.Equals("Official", StringComparison.OrdinalIgnoreCase))
                return selected.Name.ToLowerInvariant();
            selected = selected.Parent;
        }

        return Path.GetFileName(rootFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)).ToLowerInvariant();
    }

    private static IEnumerable<string> GetExpectedBasePackageNamesFromContent(string selectedFolder)
    {
        foreach (var liveryRoot in FindAddonLiveryRoots(selectedFolder))
        {
            var cfgPath = Path.Combine(liveryRoot, "livery.cfg");
            var jsonPath = Path.Combine(liveryRoot, "livery.json");
            var text = "";

            try { if (File.Exists(cfgPath)) text += File.ReadAllText(cfgPath) + " "; } catch { }
            try { if (File.Exists(jsonPath)) text += File.ReadAllText(jsonPath) + " "; } catch { }

            text = text.ToLowerInvariant();
            if (text.Contains("pmdg-aircraft-738") || text.Contains("b738") || text.Contains("ng3")) yield return "pmdg-aircraft-738";
            if (text.Contains("pmdg-aircraft-737") || text.Contains("b737")) yield return "pmdg-aircraft-737";
            if (text.Contains("pmdg-aircraft-739") || text.Contains("b739")) yield return "pmdg-aircraft-739";
            if (text.Contains("pmdg-aircraft-77w") || text.Contains("b77w") || text.Contains("777-300er")) yield return "pmdg-aircraft-77w";
            if (text.Contains("fnx_") || text.Contains("fenix") || text.Contains("a320")) yield return "fnx-aircraft-320";
            if (text.Contains("a319")) yield return "fnx-aircraft-319-321";
            if (text.Contains("a321")) yield return "fnx-aircraft-319-321";
        }
    }

    private static bool HasAircraftCfg(string folder)
    {
        return SafeEnumerateFiles(folder, "aircraft.cfg").Any();
    }

    private static IEnumerable<string> FindPackageRoots(string selectedFolder)
    {
        var current = new DirectoryInfo(selectedFolder);
        while (current is not null)
        {
            if (current.Name.Equals("Community", StringComparison.OrdinalIgnoreCase) ||
                current.Name.Equals("Official", StringComparison.OrdinalIgnoreCase) ||
                current.Name.Equals("Packages", StringComparison.OrdinalIgnoreCase))
            {
                if (Directory.Exists(current.FullName))
                    yield return current.FullName;

                var community = Path.Combine(current.FullName, "Community");
                if (Directory.Exists(community))
                    yield return community;

                var official = Path.Combine(current.FullName, "Official");
                if (Directory.Exists(official))
                    yield return official;
            }
            current = current.Parent;
        }
    }

    private static IEnumerable<string> GetDefaultMsfsPackageRoots()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (!string.IsNullOrWhiteSpace(appData))
            yield return Path.Combine(appData, "Microsoft Flight Simulator 2024", "Packages", "Community");

        if (!string.IsNullOrWhiteSpace(localAppData))
        {
            yield return Path.Combine(localAppData, "Packages", "Microsoft.Limitless_8wekyb3d8bbwe", "LocalCache", "Packages", "Community");
            yield return Path.Combine(localAppData, "Packages", "Microsoft.FlightSimulator_8wekyb3d8bbwe", "LocalCache", "Packages", "Community");
        }
    }

    private static IEnumerable<string> GetExpectedBasePackageNames(string selectedName)
    {
        if (selectedName.Contains("pmdg-aircraft-738-liveries")) yield return "pmdg-aircraft-738";
        if (selectedName.Contains("pmdg-aircraft-737-liveries")) yield return "pmdg-aircraft-737";
        if (selectedName.Contains("pmdg-aircraft-739-liveries")) yield return "pmdg-aircraft-739";
        if (selectedName.Contains("pmdg-aircraft-77w-liveries")) yield return "pmdg-aircraft-77w";
    }

    private static bool LooksLikeKnownBasePackage(string name)
    {
        name = name.ToLowerInvariant();
        if (name.Contains("-liveries") || name.Contains("livery") || name.Contains("announcement"))
            return false;

        // Be strict here. Words like "Fenix" can appear in effect libraries and announcement packs.
        return name.Equals("pmdg-aircraft-738", StringComparison.OrdinalIgnoreCase)
            || name.Equals("pmdg-aircraft-737", StringComparison.OrdinalIgnoreCase)
            || name.Equals("pmdg-aircraft-739", StringComparison.OrdinalIgnoreCase)
            || name.Equals("pmdg-aircraft-77w", StringComparison.OrdinalIgnoreCase)
            || name.Equals("fnx-aircraft-320", StringComparison.OrdinalIgnoreCase)
            || name.Equals("fnx-aircraft-319", StringComparison.OrdinalIgnoreCase)
            || name.Equals("fnx-aircraft-321", StringComparison.OrdinalIgnoreCase)
            || name.Equals("fnx-aircraft-319-321", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeKnownLiveryPackage(string name)
    {
        name = name.ToLowerInvariant();
        if (LooksLikeKnownBasePackage(name))
            return false;
        if (name.Contains("pmdg") && name.Contains("liver"))
            return true;
        if (name.Contains("fnx-aircraft") && !LooksLikeKnownBasePackage(name))
            return true;
        if (name.Contains("fenix") && name.Contains("liver"))
            return true;
        return KnownLiveryPackageHints.Any(hint => name.Contains(hint, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<string> SafeEnumerateTopDirectories(string rootFolder)
    {
        try { return Directory.EnumerateDirectories(rootFolder).ToList(); }
        catch { return Array.Empty<string>(); }
    }

    private static void AddUniqueFiles(List<string> target, IEnumerable<string> source)
    {
        foreach (var item in source)
        {
            if (!target.Any(existing => PathsEqual(existing, item)))
                target.Add(item);
        }
    }

    private static void AddUniqueFolders(List<string> target, IEnumerable<string> source)
    {
        foreach (var item in source.Where(Directory.Exists))
        {
            if (!target.Any(existing => PathsEqual(existing, item)))
                target.Add(item);
        }
    }

    private static bool IsInside(string fileOrFolder, string rootFolder)
    {
        try
        {
            var full = Path.GetFullPath(fileOrFolder);
            var root = Path.GetFullPath(rootFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return full.StartsWith(root, StringComparison.OrdinalIgnoreCase) || PathsEqual(full, rootFolder);
        }
        catch { return false; }
    }

    private static IEnumerable<AircraftEntry> ParseAircraftCfg(string cfgFile)
    {
        string[] lines;

        try { lines = File.ReadAllLines(cfgFile); }
        catch { yield break; }

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
                    currentEntry = new AircraftEntry
                    {
                        SourceFile = cfgFile,
                        AddonFamily = "Traffic Library",
                        SourceKind = "aircraft.cfg",
                        ModelNameSource = "aircraft.cfg title",
                        Confidence = "High"
                    };
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
                    currentEntry.DisplayName = value;
                    break;
                case "ui_variation":
                    if (string.IsNullOrWhiteSpace(currentEntry.DisplayName))
                        currentEntry.DisplayName = value;
                    break;
                case "icao_type_designator":
                    currentEntry.TypeCode = value.ToUpperInvariant();
                    break;
                case "icao_airline":
                    currentEntry.AirlineCode = value.ToUpperInvariant();
                    break;
                case "atc_airline":
                    if (string.IsNullOrWhiteSpace(currentEntry.CallsignPrefix) && value.Length <= 4)
                        currentEntry.CallsignPrefix = value.ToUpperInvariant();
                    break;
                case "atc_id":
                    currentEntry.Registration = value.ToUpperInvariant();
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
            if (string.IsNullOrWhiteSpace(entry.DisplayName))
                entry.DisplayName = entry.Title;
            yield return entry;
        }
    }

    private static IEnumerable<string> FindAddonLiveryRoots(string rootFolder)
    {
        var candidates = SafeEnumerateFiles(rootFolder, "livery.json")
            .Concat(SafeEnumerateFiles(rootFolder, "livery.cfg"))
            .Select(Path.GetDirectoryName)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var dir in candidates)
        {
            var hasLiveryJson = File.Exists(Path.Combine(dir, "livery.json"));
            var hasLiveryCfg = File.Exists(Path.Combine(dir, "livery.cfg"));

            var hasModelLiveryXml = SafeEnumerateFiles(dir, "livery.xml")
                .Any(file => (Path.GetFileName(Path.GetDirectoryName(file)) ?? "").StartsWith("model.", StringComparison.OrdinalIgnoreCase));

            var hasTextureFolder = false;
            try
            {
                hasTextureFolder = Directory.EnumerateDirectories(dir)
                    .Any(sub => Path.GetFileName(sub).StartsWith("texture", StringComparison.OrdinalIgnoreCase));
            }
            catch { }

            var looksLikeKnownAddon = false;
            if (hasLiveryCfg)
            {
                try
                {
                    var text = File.ReadAllText(Path.Combine(dir, "livery.cfg"));
                    looksLikeKnownAddon = text.Contains("required_tags", StringComparison.OrdinalIgnoreCase)
                        || text.Contains("fnx_", StringComparison.OrdinalIgnoreCase)
                        || text.Contains("productPackage", StringComparison.OrdinalIgnoreCase)
                        || text.Contains("airlineIcao", StringComparison.OrdinalIgnoreCase);
                }
                catch { }
            }

            if ((hasLiveryJson || hasLiveryCfg) && (hasModelLiveryXml || hasTextureFolder || looksLikeKnownAddon))
                yield return dir;
        }
    }

    private static AircraftEntry? ParseAddonLivery(string liveryRoot, string scanRoot, List<AircraftEntry> cfgEntries, List<string> aircraftCfgFiles)
    {
        var cfgPath = Path.Combine(liveryRoot, "livery.cfg");
        var jsonPath = Path.Combine(liveryRoot, "livery.json");

        var cfg = File.Exists(cfgPath) ? ReadIniLike(cfgPath) : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var json = File.Exists(jsonPath) ? ReadFlatJson(jsonPath) : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var productPackage = First(json, "productPackage", "productId", cfg, "productPackage", "product_package");
        var displayName = First(json, "title", cfg, "title", "name", "ui_variation");
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = Path.GetFileName(liveryRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        var airlineCode = First(json, "airlineIcao", "icao", cfg, "icao_airline", "airlineIcao").ToUpperInvariant();
        var registration = First(json, "registration", "atcId", cfg, "atc_id", "registration").ToUpperInvariant();
        var requiredTags = First(cfg, "required_tags", json, "required_tags", "tags");
        var customModels = First(cfg, "fnx_custom_models", json, "fnx_custom_models");
        var typeCode = InferAddonType(productPackage, requiredTags + " " + customModels, liveryRoot, displayName);
        var addonFamily = InferAddonFamily(productPackage, requiredTags + " " + customModels, liveryRoot);

        var resolvedModelName = ResolveModelName(displayName, registration, productPackage, requiredTags, customModels, typeCode, addonFamily, cfgEntries, aircraftCfgFiles, scanRoot);

        var entry = new AircraftEntry
        {
            Title = resolvedModelName.ModelName,
            DisplayName = displayName,
            TypeCode = typeCode,
            AirlineCode = airlineCode,
            CallsignPrefix = airlineCode,
            Registration = registration,
            AddonFamily = addonFamily,
            SourceKind = "Addon Livery",
            ModelNameSource = resolvedModelName.Source,
            Confidence = resolvedModelName.Confidence,
            Warning = resolvedModelName.Warning,
            SourceFile = File.Exists(jsonPath) ? jsonPath : cfgPath
        };

        if (string.IsNullOrWhiteSpace(entry.Title) && !string.IsNullOrWhiteSpace(displayName))
        {
            entry.Title = displayName;
            entry.Confidence = "Low";
            entry.ModelNameSource = "livery metadata fallback";
            entry.Warning = "ModelName could not be confirmed from aircraft.cfg.";
        }

        return string.IsNullOrWhiteSpace(entry.Title) && string.IsNullOrWhiteSpace(entry.TypeCode) ? null : entry;
    }

    private static (string ModelName, string Confidence, string Source, string Warning) ResolveModelName(
        string displayName,
        string registration,
        string productPackage,
        string requiredTags,
        string customModels,
        string typeCode,
        string addonFamily,
        List<AircraftEntry> cfgEntries,
        List<string> aircraftCfgFiles,
        string scanRoot)
    {
        var tagText = $"{requiredTags} {customModels}";

        if (addonFamily.StartsWith("Fenix", StringComparison.OrdinalIgnoreCase))
        {
            var fenixResolved = ResolveFenixModelName(typeCode, tagText, cfgEntries);
            if (!string.IsNullOrWhiteSpace(fenixResolved.ModelName))
                return fenixResolved;
        }

        if (addonFamily.StartsWith("PMDG", StringComparison.OrdinalIgnoreCase))
        {
            var pmdgResolved = ResolvePmdgModelName(productPackage, typeCode, tagText, cfgEntries);
            if (!string.IsNullOrWhiteSpace(pmdgResolved.ModelName))
                return pmdgResolved;
        }

        var relatedCfgEntries = cfgEntries.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(productPackage))
        {
            var packageKey = productPackage.Replace("-liveries", "", StringComparison.OrdinalIgnoreCase);
            relatedCfgEntries = relatedCfgEntries.Where(e => e.SourceFile.Contains(packageKey, StringComparison.OrdinalIgnoreCase));
        }

        var related = relatedCfgEntries.ToList();
        if (related.Count == 0 && !string.IsNullOrWhiteSpace(typeCode))
            related = cfgEntries.Where(e => e.TypeCode.Equals(typeCode, StringComparison.OrdinalIgnoreCase)).ToList();

        var byRegistration = related.FirstOrDefault(e => !string.IsNullOrWhiteSpace(registration) &&
                                                         (e.Title.Contains(registration, StringComparison.OrdinalIgnoreCase) ||
                                                          e.DisplayName.Contains(registration, StringComparison.OrdinalIgnoreCase) ||
                                                          e.Registration.Equals(registration, StringComparison.OrdinalIgnoreCase)));
        if (byRegistration is not null)
            return (byRegistration.Title, "High", "matched aircraft.cfg title by registration", "");

        var normalizedDisplay = Normalize(displayName);
        var byDisplay = related.FirstOrDefault(e => !string.IsNullOrWhiteSpace(displayName) &&
                                                    (Normalize(e.Title).Contains(normalizedDisplay) || normalizedDisplay.Contains(Normalize(e.DisplayName))));
        if (byDisplay is not null)
            return (byDisplay.Title, "High", "matched aircraft.cfg title by livery name", "");

        var byType = related.FirstOrDefault(e => !string.IsNullOrWhiteSpace(typeCode) && e.TypeCode.Equals(typeCode, StringComparison.OrdinalIgnoreCase));
        if (byType is not null)
            return (byType.Title, "Medium", "matched aircraft.cfg title by aircraft type", "ModelName was matched by aircraft type only. Verify in vPilot.");

        if (!string.IsNullOrWhiteSpace(displayName))
            return (displayName, "Low", "livery metadata fallback", "ModelName could not be confirmed from aircraft.cfg.");

        return ("", "Low", "not found", "ModelName could not be determined.");
    }


    private static (string ModelName, string Confidence, string Source, string Warning) ResolveFenixModelName(
        string typeCode,
        string tagText,
        List<AircraftEntry> cfgEntries)
    {
        var tags = NormalizeTagText(tagText);

        var aircraft = typeCode.ToUpperInvariant() switch
        {
            "A319" => "FenixA319",
            "A321" => "FenixA321",
            _ => "FenixA320"
        };

        var engine = "";
        if (tags.Contains("IAE")) engine = "IAE";
        else if (tags.Contains("CFM")) engine = "CFM";

        var wing = "";
        if (tags.Contains("WF") || tags.Contains("WINGFENCE") || tags.Contains("WINGFENCES")) wing = "WF";
        else if (tags.Contains("SL") || tags.Contains("SHARKLET") || tags.Contains("SHARKLETS")) wing = "SL";

        if (string.IsNullOrWhiteSpace(engine) || string.IsNullOrWhiteSpace(wing))
            return ("", "Low", "Fenix required_tags", "Fenix engine or wing variant could not be determined from required_tags.");

        var expectedTitle = $"{aircraft} {engine} {wing}";
        var match = cfgEntries.FirstOrDefault(e => e.Title.Equals(expectedTitle, StringComparison.OrdinalIgnoreCase));
        if (match is not null)
            return (match.Title, "High", "matched Fenix required_tags to aircraft.cfg title", "");

        match = cfgEntries.FirstOrDefault(e =>
            e.Title.Contains(aircraft, StringComparison.OrdinalIgnoreCase) &&
            e.Title.Contains(engine, StringComparison.OrdinalIgnoreCase) &&
            e.Title.Contains(wing, StringComparison.OrdinalIgnoreCase));

        if (match is not null)
            return (match.Title, "High", "matched Fenix required_tags to aircraft.cfg title", "");

        // Fenix A320 titles are stable base model names. If the base folder is not readable but tags are complete,
        // export as Medium instead of falling back to the livery display name.
        return (expectedTitle, "Medium", "Fenix required_tags standard model name", "ModelName was derived from Fenix required_tags. Verify in vPilot.");
    }

    private static (string ModelName, string Confidence, string Source, string Warning) ResolvePmdgModelName(
        string productPackage,
        string typeCode,
        string tagText,
        List<AircraftEntry> cfgEntries)
    {
        var tags = NormalizeTagText($"{productPackage} {typeCode} {tagText}");
        var related = cfgEntries.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(productPackage))
        {
            var packageKey = productPackage.Replace("-liveries", "", StringComparison.OrdinalIgnoreCase);
            related = related.Where(e => e.SourceFile.Contains(packageKey, StringComparison.OrdinalIgnoreCase));
        }

        var relatedList = related.ToList();
        if (relatedList.Count == 0 && !string.IsNullOrWhiteSpace(typeCode))
            relatedList = cfgEntries.Where(e => e.TypeCode.Equals(typeCode, StringComparison.OrdinalIgnoreCase) || e.Title.Contains(typeCode, StringComparison.OrdinalIgnoreCase)).ToList();

        if (relatedList.Count == 0)
            return ("", "Low", "PMDG required_tags", "No PMDG base aircraft.cfg title was found.");

        var variantCandidates = relatedList.ToList();

        // PMDG 737 winglet / exterior tags. Keep this intentionally broad because PMDG title wording differs by version.
        if (tags.Contains("B738EXT") || tags.Contains("738EXT"))
            variantCandidates = variantCandidates.Where(e => TitleHasAny(e, "738", "737-800", "737 800", "B738")).ToList();
        if (tags.Contains("B737") || tags.Contains("737"))
            variantCandidates = variantCandidates.Where(e => TitleHasAny(e, "737", "B737")).ToList();
        if (tags.Contains("B739") || tags.Contains("739"))
            variantCandidates = variantCandidates.Where(e => TitleHasAny(e, "739", "737-900", "737 900", "B739")).ToList();

        if (tags.Contains("BW_L") || tags.Contains("BW_R") || tags.Contains("BLENDEDWINGLETS"))
        {
            var wingletMatches = variantCandidates.Where(e => TitleHasAny(e, "BW", "BLENDED", "WINGLETS", "WL")).ToList();
            if (wingletMatches.Count > 0)
                variantCandidates = wingletMatches;
        }

        if (tags.Contains("SSW") || tags.Contains("SPLIT") || tags.Contains("SCIMITAR"))
        {
            var scimitarMatches = variantCandidates.Where(e => TitleHasAny(e, "SSW", "SPLIT", "SCIMITAR")).ToList();
            if (scimitarMatches.Count > 0)
                variantCandidates = scimitarMatches;
        }

        // PMDG 777 currently has fewer useful livery tags; match the 77W/GE base model if present.
        if (tags.Contains("B77W") || tags.Contains("77W") || tags.Contains("777300ER"))
        {
            var tripleSevenMatches = variantCandidates.Where(e => TitleHasAny(e, "77W", "777-300ER", "777 300ER", "B77W")).ToList();
            if (tripleSevenMatches.Count > 0)
                variantCandidates = tripleSevenMatches;
        }

        if (tags.Contains("ENGINEGEW") || tags.Contains("GEW") || tags.Contains("GE90"))
        {
            var engineMatches = variantCandidates.Where(e => TitleHasAny(e, "GE", "GE90", "GEW")).ToList();
            if (engineMatches.Count > 0)
                variantCandidates = engineMatches;
        }

        var best = variantCandidates.FirstOrDefault();
        if (best is not null)
        {
            var confidence = variantCandidates.Count == 1 ? "High" : "Medium";
            var warning = confidence == "High" ? "" : "Multiple PMDG base titles matched. Verify in vPilot.";
            return (best.Title, confidence, "matched PMDG required_tags to aircraft.cfg title", warning);
        }

        var byType = relatedList.FirstOrDefault(e => !string.IsNullOrWhiteSpace(typeCode) && e.TypeCode.Equals(typeCode, StringComparison.OrdinalIgnoreCase));
        if (byType is not null)
            return (byType.Title, "Medium", "matched PMDG aircraft.cfg title by aircraft type", "PMDG ModelName was matched by aircraft type only. Verify in vPilot.");

        return ("", "Low", "PMDG required_tags", "PMDG variant could not be matched to a base aircraft.cfg title.");
    }

    private static HashSet<string> NormalizeTagText(string value)
    {
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in Regex.Matches(value ?? "", @"[A-Za-z0-9_\-]+"))
        {
            var token = match.Value.ToUpperInvariant().Replace("-", "");
            if (!string.IsNullOrWhiteSpace(token))
                tags.Add(token);
        }
        return tags;
    }

    private static bool TitleHasAny(AircraftEntry entry, params string[] needles)
    {
        var text = $"{entry.Title} {entry.DisplayName}";
        return needles.Any(needle => text.Contains(needle, StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";
        return Regex.Replace(value.ToLowerInvariant(), @"[^a-z0-9]+", "");
    }

    private static bool PathsEqual(string first, string second)
    {
        try
        {
            return string.Equals(
                Path.GetFullPath(first).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                Path.GetFullPath(second).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static Dictionary<string, string> ReadIniLike(string path)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var rawLine in File.ReadAllLines(path))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";") || line.StartsWith("#") || line.StartsWith("["))
                    continue;
                var idx = line.IndexOf('=');
                if (idx <= 0)
                    continue;
                values[line[..idx].Trim()] = CleanValue(line[(idx + 1)..].Trim());
            }
        }
        catch { }
        return values;
    }

    private static Dictionary<string, string> ReadFlatJson(string path)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                values[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString() ?? "",
                    JsonValueKind.Number => prop.Value.ToString(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => prop.Value.ToString()
                };
            }
        }
        catch { }
        return values;
    }

    private static IEnumerable<string> SafeEnumerateFiles(string rootFolder, string pattern)
    {
        var pending = new Stack<string>();
        if (!string.IsNullOrWhiteSpace(rootFolder) && Directory.Exists(rootFolder))
            pending.Push(rootFolder);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            IEnumerable<string> files = Array.Empty<string>();
            IEnumerable<string> dirs = Array.Empty<string>();

            try { files = Directory.EnumerateFiles(current, pattern, SearchOption.TopDirectoryOnly); } catch { }
            foreach (var file in files)
                yield return file;

            try { dirs = Directory.EnumerateDirectories(current); } catch { }
            foreach (var dir in dirs)
                pending.Push(dir);
        }
    }

    private static string First(Dictionary<string, string> first, params string[] keys)
    {
        foreach (var key in keys)
            if (first.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                return value.Trim();
        return "";
    }

    private static string First(Dictionary<string, string> first, string key1, string key2, Dictionary<string, string> second, params string[] secondKeys)
    {
        var value = First(first, key1, key2);
        return !string.IsNullOrWhiteSpace(value) ? value : First(second, secondKeys);
    }

    private static string First(Dictionary<string, string> first, string key1, string key2, string key3, Dictionary<string, string> second, params string[] secondKeys)
    {
        var value = First(first, key1, key2, key3);
        return !string.IsNullOrWhiteSpace(value) ? value : First(second, secondKeys);
    }

    private static string First(Dictionary<string, string> first, string key1, Dictionary<string, string> second, params string[] secondKeys)
    {
        var value = First(first, key1);
        return !string.IsNullOrWhiteSpace(value) ? value : First(second, secondKeys);
    }

    private static string InferAddonFamily(string productPackage, string requiredTags, string path)
    {
        var text = $"{productPackage} {requiredTags} {path}".ToLowerInvariant();
        if (text.Contains("pmdg-aircraft-738") || text.Contains("b738") || text.Contains("ng3")) return "PMDG 737";
        if (text.Contains("pmdg-aircraft-77w") || text.Contains("b77w") || text.Contains("777")) return "PMDG 777";
        if (text.Contains("fenix") || text.Contains("fnx") || text.Contains("a320")) return "Fenix A320";
        return "Addon Livery";
    }

    private static string InferAddonType(string productPackage, string requiredTags, string path, string title)
    {
        var text = $"{productPackage} {requiredTags} {path} {title}".ToLowerInvariant();
        if (text.Contains("pmdg-aircraft-738") || text.Contains("b738")) return "B738";
        if (text.Contains("pmdg-aircraft-737") || text.Contains("b737")) return "B737";
        if (text.Contains("pmdg-aircraft-739") || text.Contains("b739")) return "B739";
        if (text.Contains("pmdg-aircraft-77w") || text.Contains("b77w") || text.Contains("777-300er")) return "B77W";
        if (text.Contains("fenix") || text.Contains("fnx") || text.Contains("a320")) return "A320";
        return InferTypeCode(title, path);
    }

    private static string InferTypeCode(string title, string cfgFile)
    {
        var combined = $"{title} {cfgFile.Replace(Path.DirectorySeparatorChar, ' ')}";
        var normalized = combined.ToUpperInvariant().Replace("_", " ").Replace("-", " ");

        if (normalized.Contains("FENIXA320") || normalized.Contains("A320")) return "A320";
        if (normalized.Contains("FENIXA319") || normalized.Contains("A319")) return "A319";
        if (normalized.Contains("FENIXA321") || normalized.Contains("A321")) return "A321";

        if (normalized.Contains("737 800") || normalized.Contains("B738") || normalized.Contains("738")) return "B738";
        if (normalized.Contains("737 700") || normalized.Contains("B737")) return "B737";
        if (normalized.Contains("737 900") || normalized.Contains("B739") || normalized.Contains("739")) return "B739";
        if (normalized.Contains("777 300ER") || normalized.Contains("777 300 ER") || normalized.Contains("B77W") || normalized.Contains("77W")) return "B77W";

        var match = KnownTypeRegex.Match(combined);
        if (!match.Success) return "";
        var type = match.Value.ToUpperInvariant();
        return type switch { "B737" => "B738", _ => type };
    }

    private static string CleanValue(string value)
    {
        var commentIndex = value.IndexOf(';');
        if (commentIndex >= 0)
            value = value[..commentIndex];
        return value.Trim().Trim('"');
    }
}
