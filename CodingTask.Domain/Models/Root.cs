using System.Text.Json.Serialization;

namespace CodingTask.Domain.Models
{
    public class Root
    {
        [JsonPropertyName("projects")]
        public List<Project> Projects { get; set; } = new List<Project>();
    }
}
