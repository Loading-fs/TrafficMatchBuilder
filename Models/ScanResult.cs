using System.Collections.Generic;
using System.Linq;

namespace TrafficMatchBuilder.Models;

public sealed class ScanResult
{
    public List<AircraftEntry> Entries { get; } = new();

    public int AircraftCfgFiles { get; set; }
    public int AddonLiveryFolders { get; set; }

    public List<string> AutoDiscoveredBaseFolders { get; } = new();
    public List<string> AutoDiscoveredLiveryFolders { get; } = new();
    public List<string> DiscoveryNotes { get; } = new();

    public int ModelsFound => Entries.Count;

    public int WithTypeCode => Entries.Count(entry => !string.IsNullOrWhiteSpace(entry.TypeCode));

    public int UsableRules => Entries.Count(entry => entry.IsExportable);

    public int LowConfidenceRules => Entries.Count(entry => string.Equals(entry.Confidence, "Low", System.StringComparison.OrdinalIgnoreCase));

    public bool NeedsBaseAircraftFolder => AddonLiveryFolders > 0 && LowConfidenceRules > 0 && UsableRules == 0;
}
