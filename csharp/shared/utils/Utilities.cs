using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.VisualBasic.FileIO;

namespace goqur.shared.utils
{
    public class Utilities 
    {
        public static List<List<string>> ParseCSV (string csv, bool isSkipHeader, bool isDoubleQuotes, string delimeter)
        {
            List<List<string>> result = new List<List<string>>();


            // To use the TextFieldParser a reference to the Microsoft.VisualBasic assembly has to be added to the project. 
            using (TextFieldParser parser = new TextFieldParser(new StringReader(csv))) 
            {
                parser.CommentTokens = new string[] { "#" };
                parser.SetDelimiters(new string[] { delimeter });
                parser.HasFieldsEnclosedInQuotes = isDoubleQuotes;

                // Skip over header line.
                if (isSkipHeader) 
                {
                    parser.ReadLine();
                }

                while (!parser.EndOfData)
                {
                    var values = new List<string>();

                    var readFields = parser.ReadFields();
                    if (readFields != null)
                        values.AddRange(readFields);
                    result.Add(values);
                }
            }

            return result;
        }
    }
}