using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Xml;

//For Totals
//Create repeaterStack element for Totals
//When flagged for Totals - Sum all relevant values based on Path, if special loop over all files



public sealed class ReportGenerator
{
    private static readonly ReportGenerator _instance = new ReportGenerator();

    private ReportGenerator() { }

    public static ReportGenerator Instance => _instance;

    private Stack<IRepeaterData> repeaterStack = new Stack<IRepeaterData>();

    private IReportOutput? output;

    //Default type is CSV
    public void CreateReport(List<XMLFile> processedXmlFiles, XMLFile schemaFile)
    {
        Serilog.Log.Information("Generating default report type based on processed XML files and output schema.");

        output = ReportOutputFactory.CreateReportOutput("CSV"); //Report Generator is currently setup to only generate CSV Reports. 
                                                                //However, this could be extended in the future without major rework as the ReportOutput has been decoupled from the information layer.
        XmlNodeList schemaTags = schemaFile.GetTags();
        foreach (XmlNode node in schemaTags) //Process all top-level nodes in the schema
        {
            Serilog.Log.Information($"Processing schema tag: {node.Name}");

            HandleTag(node, processedXmlFiles);
        }

        output.DumpToFile("outputComplete.csv");
        //Currently Hard Coded output file. Will implement dynamic file naming in the future.
    }

    private int GetFirstNonNodeRepeaterIteration()
    {
        foreach (var repeater in repeaterStack)
        {
            if (repeater.GetRepeaterType() == RepeaterType.File)
            {
                return repeater.GetIterationCount();
            }
        }
        return -1; // Meaningful value indicating no non-node repeater found
    }
/// <summary>
/// Consider refactoring this. It may become unwieldy.
/// </summary>
/// <param name="location">XPath to target field</param>
/// <param name="processedXmlFiles">List of target XML files</param>
/// <returns>Field value as a string</returns>
    private string getFieldValue(string location, List<XMLFile> processedXmlFiles)
    {
        if (repeaterStack.Count > 0)
        {
            int fileIterationIndex = -1;
            if (repeaterStack.Peek().GetRepeaterType() == RepeaterType.Basic)
            { //Checking if current repeater is on files or nodes

                fileIterationIndex = GetFirstNonNodeRepeaterIteration();
                if (repeaterStack.Peek().GetIterationCount() != -1)
                {
                    // Build a new location string by splicing in repeater iteration indices
                    string updatedLocation = location;
                    foreach (var repeater in repeaterStack)
                    {
                        string path = repeater.GetPath();
                        int idx = updatedLocation.IndexOf(path, StringComparison.Ordinal);
                        if (idx != -1)
                        {
                            // Find the end of the path in the location string
                            int pathEnd = idx + path.Length;
                            // Insert [iteration] after the path
                            updatedLocation = updatedLocation.Substring(0, pathEnd) +
                                $"[{repeater.GetIterationCount()}]" +
                                updatedLocation.Substring(pathEnd);
                        }
                    }
                    XmlNodeList? nodes = null;


                    if (fileIterationIndex != -1)
                    {
                        nodes = processedXmlFiles[fileIterationIndex].GetDocument().SelectNodes(updatedLocation);
                    }
                    else
                    {
                        nodes = processedXmlFiles[0].GetDocument().SelectNodes(updatedLocation);
                    }

                    if (nodes != null && nodes.Count > 0)
                    {

                        string returnValue = "";
                        if (updatedLocation == location)
                        {
                            returnValue = nodes[0].InnerText;
                        }
                        else
                        {

                            returnValue = nodes[0].InnerText; //We spliced the location, therefore nodes[0] is the correct value
                        }
                        return returnValue;
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            else if (repeaterStack.Peek().GetRepeaterType() == RepeaterType.Total)
            {
                fileIterationIndex = GetFirstNonNodeRepeaterIteration();
                if (repeaterStack.Peek().GetIterationCount() != -1)
                {
                    // Build a new location string by splicing in repeater iteration indices
                    string updatedLocation = location;
                    foreach (var repeater in repeaterStack)
                    {
                        if (repeater.GetRepeaterType() == RepeaterType.Total || repeater.GetRepeaterType() == RepeaterType.SpecialTotal)
                        {
                            continue; // Skip Total repeaters for location splicing
                        }
                        string path = repeater.GetPath();
                        int idx = updatedLocation.IndexOf(path, StringComparison.Ordinal);
                        if (idx != -1)
                        {
                            // Find the end of the path in the location string
                            int pathEnd = idx + path.Length;
                            // Insert [iteration] after the path
                            updatedLocation = updatedLocation.Substring(0, pathEnd) +
                                $"[{repeater.GetIterationCount()}]" +
                                updatedLocation.Substring(pathEnd);
                        }
                    }
                    XmlNodeList? nodes = null;


                    if (fileIterationIndex != -1)
                    {
                        nodes = processedXmlFiles[fileIterationIndex].GetDocument().SelectNodes(updatedLocation);
                    }
                    else
                    {
                        nodes = processedXmlFiles[0].GetDocument().SelectNodes(updatedLocation);
                    }

                    if (nodes != null && nodes.Count > 0)
                    {

                        string returnValue = "";
                        string returnProcess = "";
                        double totalValue = 0;

                        if (location.Contains(repeaterStack.Peek().GetPath()))
                        {
                            //Get All relevant Values   
                            foreach (XmlNode node in nodes)
                            {
                                if (int.TryParse(node.InnerText, out int intOutputVal))
                                {
                                    totalValue += intOutputVal;
                                    if (returnProcess.Length == 0)
                                    {
                                        returnProcess = "number";
                                    }
                                }
                                else if (double.TryParse(node.InnerText, out double doubleOutputVal))
                                {
                                    totalValue += doubleOutputVal;
                                    if (returnProcess.Length == 0)
                                    {
                                        returnProcess = "number";
                                    }
                                }
                                else if (node.InnerText.Length > 0)
                                {
                                    returnValue = node.InnerText;
                                }
                                else
                                {
                                    Console.WriteLine($"'{node.InnerText}' is neither an integer nor a double.");
                                }
                            }
                        }
                        else
                        {
                            returnValue = nodes[0].InnerText; //We spliced the location, therefore nodes[0] is the correct value
                        }
                         if (returnProcess == "number")
                        {
                            if (totalValue % 1 == 0)
                            {
                                return totalValue.ToString("F0");
                            }
                            else
                            {
                                return totalValue.ToString();
                            }
                        }
                        else
                        {
                            return returnValue;
                        }
                    }
                    else
                    {
                        return "";
                    }
                }


             } //Get Value from all lower structures in file
            else if (repeaterStack.Peek().GetRepeaterType() == RepeaterType.SpecialTotal)
            {

                string returnProcess = "";
                var returnValue = "";
                double totalValue = 0;
                foreach (var xmlFile in processedXmlFiles)
                {
                    var nodes = xmlFile.GetDocument().SelectNodes(location);
                    if (nodes != null && nodes.Count > 0)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            if (int.TryParse(node.InnerText, out int intOutputVal))
                            {
                                totalValue += intOutputVal;
                                if (returnProcess.Length == 0)
                                {
                                    returnProcess = "number";
                                }
                            }
                            else if (double.TryParse(node.InnerText, out double doubleOutputVal))
                            {
                                totalValue += doubleOutputVal;
                                if (returnProcess.Length == 0)
                                {
                                    returnProcess = "number";
                                }
                            }
                            else if (node.InnerText.Length > 0)
                            {
                                returnValue = node.InnerText;
                            }
                            else
                            {
                                Console.WriteLine($"'{node.InnerText}' is neither an integer nor a double.");
                            }
                        }

                    }
                }
                if (returnProcess == "number")
                {
                    if (totalValue % 1 == 0)
                    {
                        return totalValue.ToString("F0");
                    }
                    else
                    {
                        return totalValue.ToString();
                    }
                }
                else
                {
                    return returnValue;
                }

            } //Get Value from across all files
            else if (repeaterStack.Peek().GetRepeaterType() == RepeaterType.File)
            {
                var nodes = processedXmlFiles[repeaterStack.Peek().GetIterationCount()].GetDocument().SelectNodes(location);
                if (nodes != null && nodes.Count > 0)
                {
                    return nodes[0].InnerText;
                }
            }
        }
        else
        {
            foreach (var xmlFile in processedXmlFiles)
            {
                var nodes = xmlFile.GetDocument().SelectNodes(location);
                if (nodes != null && nodes.Count > 0)
                {
                    return nodes[0].InnerText;
                }
            }
        }

        return ""; // Return empty if no match found
    }

    private string GetFileName(List<XMLFile> processedXmlFiles)  ///Need to process for file Repeater
    {
        if (processedXmlFiles.Count > 0)
        {
            int fileIterationIndex = GetFirstNonNodeRepeaterIteration();
            if (fileIterationIndex != -1)
            {
                return processedXmlFiles[fileIterationIndex].GetFileName();
            }
            else
            {
                return processedXmlFiles[0].GetFileName(); // Default to the first file if no repeater is active
            }
        }
        return string.Empty; // Return empty if no files are processed
    }

    private void HandleTag(XmlNode node, List<XMLFile> processedXmlFiles)
    {
        Serilog.Log.Information($"Handling tag: {node.Name}");
        OnEnterTag(node, processedXmlFiles);


        if (node.HasChildNodes)
        {
            if (node.Name == "Repeater" && repeaterStack.Count > 0)
            {
                var currentRepeater = repeaterStack.Peek();
                do
                {
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        Serilog.Log.Information($"Processing child of repeater node: {childNode.Name}");
                        HandleTag(childNode, processedXmlFiles);
                    }
                } while (currentRepeater.Iterate() != -1);
            }
            else
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    Serilog.Log.Information($"Processing child node: {childNode.Name}");
                    HandleTag(childNode, processedXmlFiles);
                }
            }

        }
        OnExitTag(node, processedXmlFiles);
    }

    private int GetFileLocalMax(string location, List<XMLFile> processedXmlFiles)
    {
        int fileVal = 0;
        if (repeaterStack.Count > 0)
        {
            foreach (IRepeaterData repeater in repeaterStack)
            {
                if (repeater.GetRepeaterType() == RepeaterType.File) //This is our file Value
                {
                    fileVal = repeater.GetIterationCount();
                }
            }
        }

        var nodes = processedXmlFiles[fileVal].GetDocument().SelectNodes(location);
        if (nodes != null)
        {
            return nodes.Count;
        }
        return 0;
    }

/// <summary>
/// This method needs to be refactored. It is too long and will only get longer as the program gets more complicated
/// </summary>
/// <param name="node">Contains the node being processed</param>
/// <param name="processedXmlFiles">Contains the list of xml files to process</param>
    private void OnEnterTag(XmlNode node, List<XMLFile> processedXmlFiles)
    {
        if (node.NodeType == XmlNodeType.Element)
        {
            Serilog.Log.Information($"Schema tag found: {node.Name}");
            if (node.Name == "Repeater")
            {
                Serilog.Log.Information("Processing 'Repeater' tag logic here.");
                if (node.Attributes != null && node.Attributes["location"] != null && node.Attributes["location"]?.Value != null)
                {
                    repeaterStack.Push(RepeaterDataFactory.CreateRepeaterData(RepeaterType.Basic, node.Attributes["location"].Value, node, GetFileLocalMax(node.Attributes["location"].Value, processedXmlFiles)));
                }
                else if (node.Attributes != null && node.Attributes["special"] != null && node.Attributes["special"]?.Value != null)
                {
                    repeaterStack.Push(RepeaterDataFactory.CreateRepeaterData(RepeaterType.File, node.Attributes["special"].Value, processedXmlFiles.Count));
                }


            }
            else if (node.Name == "Field")
            {
                Serilog.Log.Information("Processing 'Field' tag logic here.");
                if (node.Attributes != null && node.Attributes["location"] != null && node.Attributes["location"]?.Value != null)
                {
                    string tempValue = getFieldValue(node.Attributes["location"].Value, processedXmlFiles);
                    output.OnEnterField(tempValue);
                }
                else if (node.Attributes != null && node.Attributes["special"] != null && node.Attributes["special"]?.Value != null)
                {
                    if (node.Attributes["special"].Value == "_FileName")
                    {
                        output.OnEnterField(GetFileName(processedXmlFiles));
                    }
                }

            }
            else if (node.Name == "Total")
            {
                XmlAttributeCollection firstPreviousRepeaterSiblingAtt = GetFirstPreviousRepeaterSiblingAttribute(node);
                Serilog.Log.Information("Processing 'Total' tag logic here.");
                if (node.Attributes != null && node.Attributes["location"] != null && node.Attributes["location"]?.Value != null)
                {
                    repeaterStack.Push(RepeaterDataFactory.CreateRepeaterData(RepeaterType.Total, node.Attributes["location"].Value));
                }
                else if (node.Attributes != null && node.Attributes["special"] != null && node.Attributes["special"]?.Value != null)
                {
                    repeaterStack.Push(RepeaterDataFactory.CreateRepeaterData(RepeaterType.SpecialTotal, node.Attributes["special"].Value));
                }
                else if (firstPreviousRepeaterSiblingAtt != null)
                {
                    // Handle first sibling repeater relevant attributes
                    if (firstPreviousRepeaterSiblingAtt["location"] != null)
                    {
                        repeaterStack.Push(RepeaterDataFactory.CreateRepeaterData(RepeaterType.Total, firstPreviousRepeaterSiblingAtt["location"].Value));
                    }
                    else if (firstPreviousRepeaterSiblingAtt["special"] != null)
                    {
                        repeaterStack.Push(RepeaterDataFactory.CreateRepeaterData(RepeaterType.SpecialTotal, firstPreviousRepeaterSiblingAtt["special"].Value));
                    }
                }
            }
            else if (node.Name == "NewLine")
            {
                Serilog.Log.Information("Processing 'NewLine' tag logic here.");
                output.OnEnterNewLine();
            }
            else
            {

            }
        }
        else if (node.NodeType == XmlNodeType.Text)
        {
            if (node.Name == "#text")
            {
                Serilog.Log.Information("Processing 'Text' tag logic here.");
                output.OnProcessText(node.Value);
            }

        }
        else
        {
            Serilog.Log.Warning($"Unhandled node type in schema: {node.NodeType}");
            Serilog.Log.Warning($"Value: {node.Value}");
        }
    }

    private XmlAttributeCollection GetFirstPreviousRepeaterSiblingAttribute(XmlNode node)
    {
        XmlNode? previousSibling = node.PreviousSibling;
        while (previousSibling != null)
        {
            if (previousSibling.NodeType == XmlNodeType.Element && previousSibling.Name == "Repeater")
            {
                if (previousSibling.Attributes != null && previousSibling.Attributes["location"] != null)
                {
                    return previousSibling.Attributes;
                }
                else if (previousSibling.Attributes != null && previousSibling.Attributes["special"] != null)
                {
                    return previousSibling.Attributes;
                }
            }
            previousSibling = previousSibling.PreviousSibling;
        }
        return null; // No previous repeater sibling found
    }

    private void OnExitTag(XmlNode node, List<XMLFile> processedXmlFiles)
    {
        Serilog.Log.Information($"Exiting tag: {node.Name}");
        if (node.NodeType == XmlNodeType.Element)
        {
            if (node.Name == "Repeater")
            {
                repeaterStack.Pop();
                output.OnExitRepeater();
            }
            else if (node.Name == "Field")
            {
                output.OnExitField();
            }
            else if (node.Name == "Header")
            {
                output.OnExitHeader();
            }
            else if (node.Name == "Total")
            {
                repeaterStack.Pop();
                output.OnExitTotal();
            }
            else if (node.Name == "NewLine")
            {
                output.OnExitNewLine();
            }
            else
            {

            }
        }
        else if (node.NodeType == XmlNodeType.Text)
        {
            if (node.Name == "#text")
            {
                //No specific exit logic for text nodes
            }

        }
        else
        {
            Serilog.Log.Warning($"Unhandled node type in schema: {node.NodeType}");
            Serilog.Log.Warning($"Value: {node.Value}");
        }
    }
}