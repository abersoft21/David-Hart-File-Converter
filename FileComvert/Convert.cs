using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace David_Hart_File_Converter
{
    public class Convert
    {
        public static string PushCommand(string line)
        {
            var commandParts = new List<string> { line };
            var validExts = new string[3] { "csv", "xml", "json" };

            Console.WriteLine("Please confirm the file extension, eg: \"csv\", \"json\", \"xml\".");
            string ext = string.Empty;


            ext = Console.ReadLine().ToString();


            commandParts.Add(ext);

            return ConvertCsvFile(commandParts);

        }

        private static string ConvertCsvFile(List<string> commandParts)
        {
            string result = string.Empty;

            string sourceFile = commandParts[0];
            string targetExtension = commandParts[1];
            string outputFilename = string.Empty;

            string[] srcFile;
            if (File.Exists(sourceFile))
            {
                try
                {
                    // check file not in use
                    using (FileStream stream = File.Open(sourceFile, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        stream.Close();
                    }
                    srcFile = File.ReadAllLines(sourceFile);
                }
                catch (IOException openException)
                {
                    return openException.Message;
                }
            }
            else
            {
                return $"File{sourceFile} doesn't exist.";
            }


            string srcFileHeader = srcFile.FirstOrDefault();

            string[] headers;

            if (srcFileHeader.Length > 0)
                headers = srcFileHeader.Split(',');
            else
                throw new Exception("CSV header Invalid.");

            if (headers.Any(h => h.Length == 0))
                result = "CSV headers must have no empty values.";


            switch (targetExtension.ToLower())
            {
                case "xml":
                case "json":
                var structure = GetIntermediateFileStructure(headers);
                if (targetExtension == "xml")
                {
                    targetExtension = "xml";

                    string xml = GetXmlFromCsv(structure, srcFile.Skip(1).ToArray());
                    outputFilename = Path.ChangeExtension(sourceFile, "xml");

                    SaveOutputFile(xml, outputFilename);
                    result = $"> Saved to {outputFilename}.";
                }
                if (targetExtension == "json")
                {
                    string json = ConvertCsvJson(structure, srcFile.Skip(1).ToArray());
                    outputFilename = Path.ChangeExtension(sourceFile, "json");

                    SaveOutputFile(json, outputFilename);
                    result = $"> Saved to {outputFilename}.";
                }
                break;

                case "csv":
                // No easy task - whole idea's that csv is 2-dimensional, json & xml can be structured!
                result = ConvertToCsv(sourceFile, outputFilename);
                break;

                default:
                result = "unrecognised conversion type(s)";
                break;
            }
            return result;
        }

        private static void SaveOutputFile(string text, string outputFilename)
        {
            File.WriteAllText(outputFilename, text);
        }

        private static string GetXmlFromCsv(IntermediateStructure structure, string[] lines)
        {
            char separator = ',';

            if (structure.ListGroupHeaders.All(s => s.Value == null))
            {
                // valid CSV - can be converted to XML
                var xml = new XElement("Root",
                lines.Select(line => new XElement("Entry",
                line.Split(separator)
                    .Select((column, index) => new XElement(structure.ListGroupHeaders[index].Key, column)))));

                return xml.ToString();
            }
            else
            {
                // parse structure manually
                XmlDocument xml = new XmlDocument();

                XmlElement header = xml.CreateElement(string.Empty, "Body", string.Empty);
                xml.AppendChild(header);

                foreach (var line in lines)
                {
                    XmlElement entry = xml.CreateElement(string.Empty, "Data", string.Empty);
                    header.AppendChild(entry);
                    var columns = line.Split(',');
                    int colPtr = 0;

                    // for each data entry node, get the structures
                    foreach (var col in structure.ListGroupHeaders)
                    {
                        // everycol has a node, regardless of child nodes
                        XmlElement colValue = xml.CreateElement(string.Empty, col.Key, string.Empty);
                        if (col.Value == null)
                        {
                            // just add the standard next tier nodes
                            XmlText val = xml.CreateTextNode(columns[colPtr]);
                            colValue.AppendChild(val);
                            colPtr++;
                        }
                        else
                        {
                            // build the child nodes for grouped csv 
                            foreach (var groupVal in col.Value)
                            {
                                // created the main tag,add children
                                XmlElement subgroup = xml.CreateElement(string.Empty, groupVal, string.Empty);
                                XmlText sgVal = xml.CreateTextNode(columns[colPtr]);

                                subgroup.AppendChild(sgVal);
                                colValue.AppendChild(subgroup);
                                colPtr++;
                            }
                        }
                        entry.AppendChild(colValue);
                    }
                }

                StringWriter sw = new StringWriter();
                XmlTextWriter xw = new XmlTextWriter(sw);
                xml.WriteTo(xw);
                String XmlString = sw.ToString();

                return XmlString;
            }
        }

        private static IntermediateStructure GetIntermediateFileStructure(string[] headers)
        {
            IntermediateStructure inFile = new IntermediateStructure();
            inFile.ParseCsvHeaders(headers);

            return inFile;
        }

        private static string ConvertToCsv(string sourceFile, string outputFilename)
        {
            Console.WriteLine("Contact your developer for details of the next release patch to use this function.");
            Console.ReadLine();
            throw new NotImplementedException();
        }

        private static string ConvertCsvJson(IntermediateStructure structure, string[] lines)
        {
            try
            {
                if (structure.ListGroupHeaders.All(lg => lg.Value == null))
                {
                    // simple 2D csv convert

                    string[] headers = structure.ListGroupHeaders.Select(h => h.Key).ToArray();
                    var csv = new List<string[]>();

                    foreach (var line in lines)
                    {
                        var lineArray = line.Split(',');
                        csv.Add(lineArray);
                        if (lineArray.Length != structure.ListGroupHeaders.Count())
                            System.Console.WriteLine("Unexpected fields count at line " + csv.Count + 1);
                    }


                    var listObjResult = new List<Dictionary<string, string>>();

                    for (int i = 1; i < lines.Length; i++)
                    {
                        var objResult = new Dictionary<string, string>();

                        for (int j = 0; j < headers.Length; j++)
                        {
                            objResult.Add(headers[j], csv[i][j]);
                        }

                        listObjResult.Add(objResult);
                    }

                    return JsonConvert.SerializeObject(listObjResult);
                }
                else
                {
                    // parse structure manually
                    StringBuilder sb = new StringBuilder();

                    sb.Append("[");

                    int fieldValuePos = 0;
                    int linesProcessed = 0;

                    foreach (var line in lines)
                    {
                        linesProcessed++;
                        fieldValuePos = 0;

                        var lineArray = line.Split(',');
                        // 
                        // build json
                        sb.Append("{");

                        foreach (var ent in structure.ListGroupHeaders)
                        {

                            // correct json should contain double quotes
                            sb.Append("\"");
                            sb.Append(ent.Key);
                            sb.Append("\":");


                            if (ent.Value == null)
                            {
                                sb.Append("\"");
                                sb.Append(lineArray[fieldValuePos]);
                                sb.Append("\",");
                                fieldValuePos++;
                            }
                            else
                            {
                                // append each field name & their values
                                sb.Append("{");

                                for (int j = 0; j < ent.Value.Count; j++)
                                {
                                    string subField = ent.Value[j];
                                    sb.Append($"\"{subField}\":");
                                    sb.Append($"\"{lineArray[fieldValuePos]}\"");
                                    sb.Append(j < ent.Value.Count ? "" : ",");
                                    fieldValuePos++;
                                }

                                sb.Append("},");
                            }

                            // {"Name":"Bob","Address":{"Line1":"Any Road","Town":"Any Town"}}
                            // {"ID":"1","ForeName":"Dave","SurName":"Hart","Address":"362 Calder Rd Edin","Mobile":{"Phone":"7734"},"Land":{"Line":"229946"}
                        }

                        if (sb[sb.Length - 1].Equals(','))
                            sb.Remove(sb.Length - 1, 1);

                        // close the entity
                        sb.Append("}");
                        sb.Append(linesProcessed % lines.Length > 0 ? "," : "");
                    }


                    if (sb[sb.Length - 1].Equals(','))
                        sb.Remove(sb.Length - 1, 1);

                    sb.Append("]");


                    return sb.ToString();

                }
            }
            catch (Exception e)
            {
                return "";
                //throw new Exception("Failed to convert file to JSON format");
            }
        }




        private static string ConvertCsvXml(string sourceFile, string outputFilename)
        {
            try
            {
                // careful here.. what about fields with linebreaks or commas?
                // NB - groupings in csv are underscored:
                // if header.Split('-') > 1
                // then <element><Child></child>
                var lines = File.ReadAllLines(sourceFile);

                string[] headers = GetHeader(lines[0]);

                Dictionary<string, List<string>> dictString = new Dictionary<string, List<string>>();

                foreach (var hdr in headers)
                {
                    if (hdr.Contains('_'))
                    {
                        string[] parts = hdr.Split('_');
                        if (parts.Length == 2)
                        {
                            List<string> group = new List<string>();
                            if (dictString.TryGetValue(hdr, out group))
                            {
                                // append existing group
                                group.Add(parts[1]);
                            }
                            else
                            {
                                dictString.Add(hdr, new List<string> { hdr });
                            }
                        }
                        else
                        {
                            // error
                        }
                    }
                    else
                    {
                        // force a fail if columns not unique
                        dictString.Add(hdr, new List<string> { hdr });
                    }
                }

                StringBuilder sb = new StringBuilder();

                foreach (var header in headers)
                {
                    if (header.Contains("_"))
                    {
                        // grouped
                        var groupProps = header.Split('_');
                        string groupName = groupProps[0];
                        string groupProperty = groupProps[1];




                    }
                    else
                    {

                    }
                }

                var xml = new XElement("RootElement",
                   lines.Select(line => new XElement("EntryLine",
                      line.Split('\n')
                          .Select((column, index) => new XElement("Column" + index, column)))));

                xml.Save(outputFilename);

                return $"Succesfully converted, saved as {outputFilename}";
            }
            catch (Exception e)
            {
                return $"Conversion fail during input file processing \r\n{e.Message}";
            }

        }

        private static string[] GetHeader(string line)
        {
            // be aware of quoted csv fields eg: "fields \r\n with newlines, or commas" 
            return line.Split(',');
        }
    }

    public class User
    {
        public string Name;

        public User()
        {
        }

        public Address Address
        {
            get;
            set;
        }
    }

    public class Address
    {
        public string Line1
        {
            get;
            set;
        }
        public string Town
        {
            get;
            set;
        }
    }

    public class Group
    {
        public string Name;
        public Group Subgroup;
    }
}
