using Newtonsoft.Json;

namespace OracRTD
{
    [JsonObject(MemberSerialization.OptIn)]
    class NameValue
    {
        public NameValue(string name, double value)
        {
            this.name = name;
            this.value = value;
        }

        [JsonProperty]
        public string name { get; set; }
        [JsonProperty]
        public double value{ get; set; }
    }
}
