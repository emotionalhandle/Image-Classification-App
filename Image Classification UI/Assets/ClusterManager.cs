using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ClusterManager : MonoBehaviour
{
    public GameObject clusterPrefab;
    public RectTransform clusterContainer; // The parent object where clusters will be instantiated

    public void ProcessSource(string source)
    {
        string csvPath = @"C:\Users\Squishy\Documents\image_classification\classification_results.csv";
        var lines = File.ReadAllLines(csvPath);
        
        // Skip header line and parse the CSV
        var entries = lines.Skip(1)
            .Select(line => ParseCsvLine(line))
            .Where(parts => parts.Length >= 3) // Ensure we have enough columns
            .ToList();

        // Get all unique predicted classes for this source
        var predictedClasses = entries
            .Where(parts => parts[1] == source) // Match source
            .Select(parts => parts[2]) // Get predicted class
            .Distinct()
            .ToList();

        // Create only the first cluster
        if (predictedClasses.Count > 0)
        {
            var predClass = predictedClasses[0];
            
            // Instantiate the cluster prefab
            GameObject clusterObj = Instantiate(clusterPrefab, clusterContainer);
            PopulateCluster cluster = clusterObj.GetComponent<PopulateCluster>();
            
            // Initialize the cluster with its source and predicted class
            cluster.Initialize(source, predClass);

            // Add all matching images to this cluster
            var matchingEntries = entries
                .Where(parts => parts[1] == source && parts[2] == predClass)
                .Select(parts => parts[0]); // Get image path

            foreach (var imagePath in matchingEntries)
            {
                cluster.AddImage(imagePath);
            }

            Debug.Log($"Created cluster for predicted class: {predClass}");
        }
    }

    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField.Trim());
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }

        if (currentField.Length > 0)
        {
            result.Add(currentField.Trim());
        }

        return result.ToArray();
    }

    void Start()
    {
        string csvPath = @"C:\Users\Squishy\Documents\image_classification\classification_results.csv";
        
        try
        {
            var lines = File.ReadAllLines(csvPath);
            if (lines.Length > 1) // Ensure we have at least one data line after header
            {
                // Skip header and get first data line
                var firstLine = ParseCsvLine(lines[1]);
                if (firstLine.Length >= 2)
                {
                    string firstSource = firstLine[1]; // Source is in second column
                    ProcessSource(firstSource);
                }
                else
                {
                    Debug.LogError("CSV line does not contain enough columns");
                }
            }
            else
            {
                Debug.LogError("CSV file is empty or contains only header");
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error reading CSV file: {ex.Message}");
        }
    }
} 