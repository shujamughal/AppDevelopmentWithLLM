using System;
using System.Linq;
using SharpToken;

class Program
{
    static void Main()
    {
        var tokenizer = GptEncoding.GetEncoding("cl100k_base");

        // User's query
        string userQuery = "How can I reset my password?";

        // Database of pre-existing questions
        string[] questions = {
            "How do I change my password?",
            "How do I create an account?",
            "How do I reset my forgotten password?"
        };

        // Tokenize the user query
        var userTokens = tokenizer.Encode(userQuery);

        Console.WriteLine($"User Query Tokens: {string.Join(", ", userTokens)}");

        int bestMatchIndex = -1;
        double highestSimilarity = -1;

        for (int i = 0; i < questions.Length; i++)
        {
            var questionTokens = tokenizer.Encode(questions[i]);
            double similarity = ComputeCosineSimilarity(userTokens.ToArray(), questionTokens.ToArray());

            Console.WriteLine($"Question: {questions[i]}");
            Console.WriteLine($"Tokens: {string.Join(", ", questionTokens)}");
            Console.WriteLine($"Similarity: {similarity}");
            Console.WriteLine("----------------------------------");

            if (similarity > highestSimilarity)
            {
                highestSimilarity = similarity;
                bestMatchIndex = i;
            }
        }

        Console.WriteLine($"Best match: {questions[bestMatchIndex]}");
    }

    static double ComputeCosineSimilarity(int[] vec1, int[] vec2)
    {
        // Convert token ID arrays into frequency vectors
        var allTokens = vec1.Concat(vec2).Distinct().ToArray();
        double[] vec1Freq = allTokens.Select(t => (double)vec1.Count(x => x == t)).ToArray();
        double[] vec2Freq = allTokens.Select(t => (double)vec2.Count(x => x == t)).ToArray();

        // Compute dot product
        double dotProduct = vec1Freq.Zip(vec2Freq, (x, y) => x * y).Sum();

        // Compute magnitudes
        double magnitude1 = Math.Sqrt(vec1Freq.Sum(x => x * x));
        double magnitude2 = Math.Sqrt(vec2Freq.Sum(x => x * x));

        // Handle division by zero case
        return (magnitude1 * magnitude2 == 0) ? 0 : dotProduct / (magnitude1 * magnitude2);
    }

}
