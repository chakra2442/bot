using System;
using System.Collections.Generic;
using System.Configuration;
using Azure;
using Azure.AI.TextAnalytics;

namespace R3DD17
{
    class AzureNLP
    {
        public ParsedText Parse(string payloadText)
        {
            try
            {
                var endpoint = ConfigurationManager.AppSettings["TextminingEndpoint"];
                var credentials = ConfigurationManager.AppSettings["TextminingCredentials"];
                var client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(credentials));
                var result = new ParsedText();

                result.KeyPhrases = KeyPhraseExtractionExample(client, payloadText);
                result.Sentiment = SentimentAnalysisExample(client, payloadText);
                result.NamedEntities = EntityRecognitionExample(client, payloadText);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{payloadText} : {ex.Message}");
                return null;
            }
        }
        static Sentiment SentimentAnalysisExample(TextAnalyticsClient client, string inputText)
        {
            DocumentSentiment documentSentiment = client.AnalyzeSentiment(inputText);
            var sentiment = documentSentiment.Sentiment.ToString();
            var score = 0.0;
            switch (documentSentiment.Sentiment)
            {
                case TextSentiment.Positive:
                    score = documentSentiment.ConfidenceScores.Positive;
                    break;
                case TextSentiment.Negative:
                    score = documentSentiment.ConfidenceScores.Negative;
                    break;
                case TextSentiment.Neutral:
                case TextSentiment.Mixed:
                    score = documentSentiment.ConfidenceScores.Neutral;
                    break;
            }

            return new Sentiment() { Value = sentiment, ConfidenceScore = score };
        }

        static List<NamedEntity> EntityRecognitionExample(TextAnalyticsClient client, string inputText)
        {
            var result = new List<NamedEntity>();
            var response = client.RecognizeEntities(inputText);
            foreach (var entity in response.Value)
            {
                result.Add(
                    new NamedEntity()
                    {
                        Value = entity.Text,
                        Category = entity.Category.ToString(),
                        SubCategory = entity.SubCategory,
                        ConfidenceScore = entity.ConfidenceScore
                    });
            }

            return result;
        }


        static List<string> KeyPhraseExtractionExample(TextAnalyticsClient client, string inputText)
        {
            var result = new List<string>();
            var response = client.ExtractKeyPhrases(inputText);
            result.AddRange(response.Value);
            return result;
        }
    }
}
