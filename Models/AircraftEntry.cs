namespace TrafficMatchBuilder.Models;

public sealed class AircraftEntry
{
    public string Title { get; set; } = "";
    public string TypeCode { get; set; } = "";
    public string AirlineCode { get; set; } = "";
    public string SourceFile { get; set; } = "";

    public bool IsUsable =>
        !string.IsNullOrWhiteSpace(Title) &&
        !string.IsNullOrWhiteSpace(TypeCode);
}
