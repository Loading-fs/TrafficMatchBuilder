using System.Collections.Generic;
using System.Linq;

namespace TrafficMatchBuilder.Models;

public sealed class ScanResult
{
    public List<AircraftEntry> Entries { get; } = new();

    public int AircraftCfgFiles { get; set; }

    public int ModelsFound => Entries.Count;

    public int WithTypeCode => Entries.Count(entry => !string.IsNullOrWhiteSpace(entry.TypeCode));

    public int UsableRules => Entries.Count(entry => entry.IsUsable);
}
