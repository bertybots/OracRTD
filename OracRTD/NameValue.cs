using Newtonsoft.Json;

namespace OracRTD
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NameValue
    {
        public NameValue(string name, double value, string username, double excelDate)
        {
            this.name = name;
            this.value = value;
            this.username = username;
            this.excelDate = excelDate;
        }

        [JsonProperty]
        public string name { get; set; }
        [JsonProperty]
        public double value{ get; set; }
        [JsonProperty]
        public string username { get; set; }
        [JsonProperty]
        public double excelDate { get; set; }
    }
}
