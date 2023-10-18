// TODO this is garbage
// the packages are not compatible, so I have to reimplement theese data classes

using System.Text.Json.Serialization;

public class ExpansionData {
    [JsonPropertyName("name")]
    public string Name { get; set;}
}