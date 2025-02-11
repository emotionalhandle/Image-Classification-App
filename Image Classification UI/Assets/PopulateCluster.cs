using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class PopulateCluster : MonoBehaviour
{
    public Image referenceImage1;
    public Image referenceImage2;
    public Image referenceImage3;
    public Image referenceImage4;
    public RectTransform recommendedContent;
    public GameObject imagePrefab; // Prefab for the image to spawn

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadImagesFromCSV(@"C:\Users\Squishy\Documents\image_classification\classification_results.csv");
    }

    private void LoadImagesFromCSV(string csvFilePath)
    {
        try
        {
            var lines = File.ReadAllLines(csvFilePath);
            if (lines.Length > 0)
            {
                // Parse the first line to get source and predicted class
                var firstLineParts = ParseCsvLine(lines[1]);
                if (firstLineParts.Length >= 4) // Ensure there are at least 4 fields
                {
                    string source = firstLineParts[1]; // Set source from the second column
                    string predictedClass = firstLineParts[2]; // Set predicted class from the third column

                    // Find the corresponding .txt file
                    string txtFilePath = Path.Combine(@"C:\Users\Squishy\Documents\image_classification\Reference Clusters", $"{predictedClass}.txt");
                    LoadImagesFromFile(txtFilePath);

                    // Spawn images for every matching line in the CSV
                    for (int i = 1; i < lines.Length; i++) // Start from 1 to skip the header
                    {
                        var lineParts = ParseCsvLine(lines[i]);
                        if (lineParts.Length >= 4)
                        {
                            string lineSource = lineParts[1]; // Source from the second column
                            string linePredictedClass = lineParts[2]; // Predicted class from the third column
                            string imagePath = lineParts[0]; // Assuming the image path is in the first column

                            if (lineSource == source && linePredictedClass == predictedClass)
                            {
                                SpawnImage(imagePath);
                            }
                        }
                    }
                }
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error reading file: {ex.Message}");
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
                inQuotes = !inQuotes; // Toggle the inQuotes flag
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField.Trim()); // Add the current field to the result
                currentField = ""; // Reset for the next field
            }
            else
            {
                currentField += c; // Append the character to the current field
            }
        }

        // Add the last field
        if (currentField.Length > 0)
        {
            result.Add(currentField.Trim());
        }

        return result.ToArray();
    }

    private void LoadImagesFromFile(string filePath)
    {
        try
        {
            // Read all lines from the file
            var lines = File.ReadAllLines(filePath);

            // Load images from the first four lines
            if (lines.Length >= 4)
            {
                LoadImage(lines[0], referenceImage1);
                LoadImage(lines[1], referenceImage2);
                LoadImage(lines[2], referenceImage3);
                LoadImage(lines[3], referenceImage4);
            }
            else
            {
                Debug.LogWarning("Not enough image paths in the file.");
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error reading file: {ex.Message}");
        }
    }

    private void LoadImage(string path, Image imageComponent)
    {
        if (File.Exists(path))
        {
            // Load the image from the file
            byte[] imageData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData); // Automatically resizes the texture dimensions

            // Set filter mode and other settings for better quality
            texture.filterMode = FilterMode.Bilinear;
            texture.anisoLevel = 8;
            texture.Apply();

            imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // Get the GridLayout component from the parent
            GridLayoutGroup gridLayout = imageComponent.transform.parent.parent.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
            {
                // Use the cell size from the GridLayout
                float maxSize = gridLayout.cellSize.x; // Assuming square cells, you can also use gridLayout.cellSize.y
                Debug.Log(maxSize);
                // Calculate the aspect ratio and set the RectTransform size
                float aspectRatio = (float)texture.width / texture.height;
                RectTransform rectTransform = imageComponent.GetComponent<RectTransform>();
                
                // Calculate new dimensions while maintaining aspect ratio
                if (aspectRatio > 1) // Landscape
                {
                    rectTransform.sizeDelta = new Vector2(maxSize, maxSize / aspectRatio);
                }
                else // Portrait or square
                {
                    rectTransform.sizeDelta = new Vector2(maxSize * aspectRatio, maxSize);
                }
            }
            else
            {
                Debug.LogError("GridLayoutGroup component not found on the parent.");
            }
        }
        else
        {
            Debug.LogError($"Image file not found: {path}");
        }
    }

    private void SpawnImage(string imagePath)
    {
        if (File.Exists(imagePath))
        {
            // Instantiate a new image object from the prefab (this will be controlled by GridLayout)
            GameObject gridCell = Instantiate(imagePrefab, recommendedContent);
            
            // Create a child GameObject for the actual image
            GameObject imageObject = new GameObject("Image");
            imageObject.transform.SetParent(gridCell.transform, false);
            
            // Add Image component to the child
            Image imageComponent = imageObject.AddComponent<Image>();
            RectTransform rectTransform = imageComponent.GetComponent<RectTransform>();
            
            // Set the anchor to center
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            
            // Load the image and adjust size
            LoadImage(imagePath, imageComponent);
        }
        else
        {
            Debug.LogError($"Image file not found: {imagePath}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
