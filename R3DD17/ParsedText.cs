using System.Collections.Generic;
using Newtonsoft.Json;

namespace R3DD17
{
    public class ParsedText
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public Sentiment Sentiment { get; set; }
        public List<NamedEntity> NamedEntities { get; set; }
        public List<string> KeyPhrases { get; set; }
    }
    public class NamedEntity
    {
        public string Value { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public double ConfidenceScore { get; set; }
    }

    public class Sentiment
    {
        public string Value { get; set; }
        public double ConfidenceScore { get; set; }
    }
}
