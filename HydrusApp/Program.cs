using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        try 
        {
            var hydrus = new HydrusUtils("5a880bb8e976458d386516747c4cb070be8da0464789d1415b1c87c76660648d");
            var fileHash = "d576f9bb83d47cb7500a0076c913bab411cdec2fd6fb26b4972c617c0b157205";
            
            Console.WriteLine("Getting pages from Hydrus...");
            var pages = await hydrus.GetPages();
            Console.WriteLine($"Found {pages.Count} pages in total");
            
            string targetPageKey = null;
            
            foreach (var page in pages)
            {
                Console.WriteLine($"Checking page: {page.Name} (Key: {page.PageKey})");
                if (page.Name?.ToLower() == "testpage")
                {
                    Console.WriteLine($"Found testpage:");
                    Console.WriteLine($"  Name: {page.Name}");
                    Console.WriteLine($"  PageKey: {page.PageKey}");
                    Console.WriteLine($"  IsMediaPage: {page.IsMediaPage}");
                    Console.WriteLine($"  PageType: {page.PageType}");
                    Console.WriteLine($"  PageState: {page.PageState}");
                    targetPageKey = page.PageKey;
                    break;
                }
            }
            
            if (targetPageKey == null)
            {
                Console.WriteLine("Could not find testpage! Available pages:");
                foreach (var page in pages)
                {
                    if (page.Name?.Contains("test", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        Console.WriteLine($"- {page.Name} (Key: {page.PageKey}, IsMediaPage: {page.IsMediaPage})");
                    }
                }
                return;
            }

            Console.WriteLine($"Adding file {fileHash} to page {targetPageKey}...");
            await hydrus.AddFileToPage(targetPageKey, fileHash);
            Console.WriteLine("Successfully added file to testpage");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}



