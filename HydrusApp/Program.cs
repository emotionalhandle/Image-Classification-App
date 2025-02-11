using System;
using System.Threading.Tasks;
using HydrusApp.Models;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            string pagePath = "top page middle final page";
            string fileHash = "d576f9bb83d47cb7500a0076c913bab411cdec2fd6fb26b4972c617c0b157205";

            // Get all pages and find the target page
            var pages = await HydrusUtils.GetPages();
            Console.WriteLine($"Found {pages.Count} pages total");

            // Get the page key for the specified path
            Console.WriteLine($"Looking for page with path/key: {pagePath}");
            var pageKey = await HydrusUtils.GetPageKeyByPath(pagePath);
            Console.WriteLine($"Found page key: {pageKey}");

            // Add the file to the page
            Console.WriteLine($"Adding file {fileHash} to page {pageKey}...");
            await HydrusUtils.AddFileToPage(pageKey, fileHash);
            Console.WriteLine("File added successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}




