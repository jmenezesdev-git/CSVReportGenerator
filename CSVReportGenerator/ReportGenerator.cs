using System.Xml;

// static class ReportGenerator
// {

public class RepeaterData
{
    private string Path { get; set; }
    private List<XmlNode>? Nodes { get; set; }
    private int iterationCount = 1;

    private int iterationMax = 0;

    public RepeaterData(string name, XmlNode node, int fileLocalMax)
    {
        Path = name;
        iterationMax = fileLocalMax;

        Nodes = new List<XmlNode>();
        if (node.HasChildNodes)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                Nodes.Add(childNode);
            }
        }
        else
        {
        }
    }




    public RepeaterData(string path, int fileCount)
    {
        Path = path;
        Nodes = null;
        iterationMax = fileCount;
    }

    public string GetPath()
    {
        return Path;
    }
    public List<XmlNode> GetNodes()
    {
        return Nodes;
    }
    public int GetIterationCount()
    {
        return iterationCount;
    }
    public int Iterate()
    {
        if (iterationCount >= iterationMax)
        {
            return -1;
        }
        iterationCount++;
        return iterationCount;
    }
 }

public sealed class ReportGenerator
{
    private static readonly ReportGenerator _instance = new ReportGenerator();


    private ReportGenerator() { }

    public static ReportGenerator Instance => _instance;

    private Stack<RepeaterData> repeaterStack = new Stack<RepeaterData>();

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

        output.DumpToFile("outputIntermediate.csv");
    }

    private int GetFirstNonNodeRepeaterIteration()
    {
        foreach (var repeater in repeaterStack)
        {
            if (repeater.GetNodes() == null)
            {
                return repeater.GetIterationCount();
            }
        }
        return -1; // Meaningful value indicating no non-node repeater found
    }

    private string getFieldValue(string location, List<XMLFile> processedXmlFiles)
    {
        if (repeaterStack.Count > 0)
        {
            int fileIterationIndex = -1;
            if (repeaterStack.Peek().GetNodes() != null)
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
                        } else {

                            returnValue = nodes[0].InnerText; //We spliced the location, therefore nodes[0] is the correct value
                        }
                        return returnValue; // Return the first matching node's text
                    }
                    else
                    {
                        return "";
                     }
                }
            }
            else //Repeater it is on Files.
            {
                var nodes = processedXmlFiles[repeaterStack.Peek().GetIterationCount()].GetDocument().SelectNodes(location);
                if (nodes != null && nodes.Count > 0)
                {
                    return nodes[0].InnerText; // Return the first matching node's text
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
                    return nodes[0].InnerText; // Return the first matching node's text
                }
            }
        }
        
        return ""; // Return empty if no match found
    }

    private string GetFileName(List<XMLFile> processedXmlFiles)
    {
        if (processedXmlFiles.Count > 0)
        {
            return processedXmlFiles[0].GetFileName();
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
                    output.OnEnterNewLine();
                    output.OnExitNewLine();
                } while (currentRepeater.Iterate() != -1);
            }
            else
            {
               foreach (XmlNode childNode in node.ChildNodes){
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
            foreach (RepeaterData repeater in repeaterStack)
            {
                if (repeater.GetNodes() == null) //This is our file Value
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
                    repeaterStack.Push(new RepeaterData(node.Attributes["location"].Value, node, GetFileLocalMax(node.Attributes["location"].Value, processedXmlFiles)));
                }
                else if (node.Attributes != null && node.Attributes["special"] != null && node.Attributes["special"]?.Value != null)
                {
                    repeaterStack.Push(new RepeaterData(node.Attributes["special"].Value, processedXmlFiles.Count));
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
                Serilog.Log.Information("Processing 'Total' tag logic here.");
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

    private void OnExitTag(XmlNode node, List<XMLFile> processedXmlFiles)
    {
        Serilog.Log.Information($"Exiting tag: {node.Name}");
        if (node.NodeType == XmlNodeType.Element)
        {
            if (node.Name == "Repeater")
            {
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