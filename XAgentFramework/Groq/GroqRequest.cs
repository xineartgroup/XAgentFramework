using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace XAgentFramework.Groq
{
    public class GroqRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<GroqMessage> Messages { get; set; } = [];
    }
}
