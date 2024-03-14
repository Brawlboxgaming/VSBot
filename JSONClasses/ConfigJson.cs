using Newtonsoft.Json;

namespace VPBot.Class
{
    public class ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }
    }
}