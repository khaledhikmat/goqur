using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

using goqur.shared.models;
using goqur.shared.services;

namespace goqur.manager
{
    class Program
    {
        private static string AYAS_INDEX_NAME = "gq-ayahs-managed-index";

        public static IConfiguration Configuration;
        public static ISearchService _searchService;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Search service manager!");

            if (args != null && args.Length > 0 && args[0] == "create") 
            { 
                Console.WriteLine("Creating an index....");
                await GetSearchService().CreateIndex<Ayah>();
            } 
            else if (args != null && args.Length > 0 && args[0] == "delete")
            {
                Console.WriteLine("Deleting an index....");
                await GetSearchService().DeleteIndex();
            }
            else if (args != null && args.Length > 0 && args[0] == "upload")
            {
                Console.WriteLine("Uploading Ayah documents....");
                List<Ayah> ayahs = new List<Ayah>();
                ayahs.Add(new Ayah { 
                        Id = "1-1", 
                        SurahNumber = "1" ,
                        EnglishSurahName = "Baquara" ,
                        ArabicSurahName = "بقرة" ,
                        EnglishSurahType = "Madani" ,
                        ArabicSurahType = "مدني" ,
                        AyahNumber = "1" ,
                        EnglishNouns = new List<string>() {"Moussa", "Ferroh"} ,
                        ArabicNouns = new List<string>() {"موســـى", "فرعون"} ,
                        EnglishConcepts = new List<string>() {"Paradise", "Hell"} ,
                        ArabicConcepts = new List<string>() {"جنة", "نار"}
                    }
                );

                await GetSearchService().UploadDocs<Ayah>(ayahs);
            }
        }
        private static ISearchService GetSearchService() 
        {
            if (_searchService == null)
            {
                Configuration = new ConfigurationBuilder()
                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables()
                    .Build();

                string searchEndpoint = Configuration.GetValue<string>("SEARCH_ENDPOINT");
                string searchKey = Configuration.GetValue<string>("SEARCH_API_KEY");
                Console.WriteLine($"URL: {searchEndpoint} - KEY: {searchKey}");
                _searchService = new SearchService(searchEndpoint, searchKey, AYAS_INDEX_NAME);
            }

            return _searchService;
        }
    }
}
