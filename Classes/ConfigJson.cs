using Newtonsoft.Json;

namespace VPBot.Classes
{
    public class ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }
    }
}