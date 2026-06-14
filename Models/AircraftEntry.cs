namespace TrafficMatchBuilder.Models;

public sealed class AircraftEntry
{
    // Title is the value vPilot/MSFS must be able to load. It is exported as ModelName.
    public string Title { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string TypeCode { get; set; } = "";
    public string AirlineCode { get; set; } = "";
    public string CallsignPrefix { get; set; } = "";
    public string Registration { get; set; } = "";
    public string AddonFamily { get; set; } = "Traffic Library";
    public string SourceKind { get; set; } = "aircraft.cfg";
    public string ModelNameSource { get; set; } = "aircraft.cfg title";
    public string Confidence { get; set; } = "High";
    public string Warning { get; set; } = "";
    public string SourceFile { get; set; } = "";

    public string ExportCode => !string.IsNullOrWhiteSpace(CallsignPrefix) ? CallsignPrefix : AirlineCode;

    public bool HasMinimumData =>
        !string.IsNullOrWhiteSpace(Title) &&
        !string.IsNullOrWhiteSpace(TypeCode);

    public bool IsExportable =>
        HasMinimumData &&
        !string.Equals(Confidence, "Low", System.StringComparison.OrdinalIgnoreCase);

    public bool IsUsable => IsExportable;
}
