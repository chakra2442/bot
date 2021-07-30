using System;
using VaderSharp;

namespace R3DD17
{
    public class VaderSentiment
    {
        private SentimentIntensityAnalyzer analyzer;

        public VaderSentiment()
        {
            this.analyzer = new SentimentIntensityAnalyzer();
        }
        public ParsedText Parse(string payloadText)
        {
            try
            {
                var result = new ParsedText();
                result.KeyPhrases = null;
                result.NamedEntities = null;
                
                var polarity = this.analyzer.PolarityScores(payloadText);
                if (polarity.Compound > 0.05)
                {
                    result.Sentiment = new Sentiment() { Value = "Positive", ConfidenceScore = polarity.Positive };
                }
                else if (polarity.Compound < -0.05)
                {
                    result.Sentiment = new Sentiment() { Value = "Negetive", ConfidenceScore = polarity.Negative };
                }
                else
                {
                    result.Sentiment = new Sentiment() { Value = "Neutral", ConfidenceScore = polarity.Neutral };
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{payloadText} : {ex.Message}");
                return null;
            }
        }
    }
}
