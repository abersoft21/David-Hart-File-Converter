using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace David_Hart_File_Converter
{
    public class IntermediateStructure
    {
        public IntermediateStructure()
        {
            ListGroupHeaders = new List<KeyValuePair<string, List<string>>>();
        }

        // key is the group name or single header name; value holds group sub-headers if any.
        public List<KeyValuePair<string, List<string>>> ListGroupHeaders; 

        public void ParseCsvHeaders(string[] headers)
        {
            foreach (var header in headers)
            {
                string[] groupHeader = header.Split('_');
                var matchedHeaders = ListGroupHeaders.Where(h => h.Key.Equals(groupHeader[0]));

                if (groupHeader.Length == 1)
                {
                    // single header - add
                    if (matchedHeaders.Count() == 0)
                        ListGroupHeaders.Add(new KeyValuePair<string, List<string>>(groupHeader[0], null));
                    else
                    {
                        // error - duplicate fields in CSV file
                        throw new System.Exception("CSV File has duplicate columns or group headers");
                    }
                }
                else if (groupHeader.Length == 2)
                {
                    // group header
                    if (matchedHeaders.Count() == 0)
                    {
                        // add new header group
                        ListGroupHeaders.Add(new KeyValuePair<string, List<string>>(groupHeader[0], new List<string>() { groupHeader[1] }));
                    }
                    else
                    {
                        // append existing group
                        matchedHeaders.First().Value.Add(groupHeader[1]);
                    }
                }
                else
                {
                    throw new System.Exception("CSV Converter only supports 2 level depth group structuring");
                }
            }
        }
    }
}