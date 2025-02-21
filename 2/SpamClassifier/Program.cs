using System;
using Microsoft.ML;
using Microsoft.ML.Data;

class Program
{
    static void Main()
    {
        var mlContext = new MLContext();

        // Load data
        var data = mlContext.Data.LoadFromTextFile<SpamInput>("spam-data.tsv", separatorChar: '\t', hasHeader: true);

        // Define training pipeline
        var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(SpamInput.Text))
            .Append(mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(SpamInput.Label)))
            .Append(mlContext.Transforms.Concatenate("Features", "Features"))
            .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        // Train model
        var model = pipeline.Fit(data);

        // Test model
        var predictionEngine = mlContext.Model.CreatePredictionEngine<SpamInput, SpamPrediction>(model);

        // Predict a sample message
        var result = predictionEngine.Predict(new SpamInput { Text = "You won a free car! Click the link now!" });

        Console.WriteLine($"Prediction: {(result.PredictedLabel == "spam" ? "Spam" : "Not Spam")}");
    }
}

// Data model for input
public class SpamInput
{
    [LoadColumn(0)]
    public string Text { get; set; }

    [LoadColumn(1)]
    public string Label { get; set; }
}

// Data model for output prediction
public class SpamPrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedLabel { get; set; }
}
