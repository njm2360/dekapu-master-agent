public class InstanceInfo
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? DisplayName { get; set; }

    public string DisplayLabel =>
    string.IsNullOrWhiteSpace(DisplayName)
        ? Name
        : DisplayName;
}
