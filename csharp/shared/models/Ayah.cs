using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using goqur.shared.utils;

namespace goqur.shared.models
{
    public class Ayah 
    {
        [JsonPropertyName("id")]
        [SimpleField(IsKey = true, IsFilterable = true)]
        public string Id { get; set; } = "";

        [JsonPropertyName("surah_number")]
        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string SurahNumber { get; set; } = "";

        [JsonPropertyName("surah_name_en")]
        [SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string EnglishSurahName { get; set; } = "";

        [JsonPropertyName("surah_name_ar")]
        [SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string ArabicSurahName { get; set; } = "";

        [JsonPropertyName("surah_type_en")]
        [SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string EnglishSurahType { get; set; } = "";

        [JsonPropertyName("surah_type_ar")]
        [SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string ArabicSurahType { get; set; } = "";

        [JsonPropertyName("ayah_number")]
        [SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string AyahNumber { get; set; } = "";

        [JsonPropertyName("nouns_en")]
        [SearchableField(IsFilterable = true, IsFacetable = true)]
        public List<string> EnglishNouns { get; set; } = new List<string>();

        [JsonPropertyName("nouns_ar")]
        [SearchableField(IsFilterable = true, IsFacetable = true)]
        public List<string> ArabicNouns { get; set; } = new List<string>();

        [JsonPropertyName("concepts_en")]
        [SearchableField(IsFilterable = true, IsFacetable = true)]
        public List<string> EnglishConcepts { get; set; } = new List<string>();

        [JsonPropertyName("concepts_ar")]
        [SearchableField(IsFilterable = true, IsFacetable = true)]
        public List<string> ArabicConcepts { get; set; } = new List<string>();

        public static List<Ayah> ParseToList(string csv, bool isSkipHeader, bool isDoubleQuote, string delimeter) 
        {
            List<Ayah> ayahs = new List<Ayah>();
            List<List<string>> docs =  Utilities.ParseCSV(csv, isSkipHeader, isDoubleQuote, delimeter);    
            foreach(List<string> doc in docs) 
            {
                Ayah ayah = new Ayah();
                int index = 0;
                foreach (string field in doc) 
                {
                    if (!string.IsNullOrEmpty(field)) 
                    {
                        if (index == 0) 
                        {
                            ayah.Id = field;        
                        }
                        else if (index == 1) 
                        {
                            ayah.SurahNumber = field;        
                        }
                        else if (index == 2) 
                        {
                            ayah.EnglishSurahName = field;        
                        }
                        else if (index == 3) 
                        {
                            ayah.ArabicSurahName = field;        
                        }
                        else if (index == 4) 
                        {
                            ayah.EnglishSurahType = field;        
                        }
                        else if (index == 5) 
                        {
                            ayah.ArabicSurahType = field;        
                        }
                        else if (index == 6) 
                        {
                            ayah.AyahNumber = field;        
                        }
                        else if (index == 7) 
                        {
                            ayah.EnglishNouns = field.Split(',').ToList();        
                        }
                        else if (index == 8) 
                        {
                            ayah.ArabicNouns = field.Split(',').ToList();        
                        }
                        else if (index == 9) 
                        {
                            ayah.EnglishConcepts = field.Split(',').ToList();        
                        }
                        else if (index == 10) 
                        {
                            ayah.ArabicConcepts = field.Split(',').ToList();        
                        }
                    }

                    index++;
                }

                ayahs.Add(ayah);
            }

            return ayahs;
        }
    }
}