using System.Xml;

// static class ReportGenerator
// {

public class RepeaterData
{
    private string Path { get; set; }
    private XmlNode? Node { get; set; }
    private int iterationCount = 0;

    public RepeaterData(string name, XmlNode? node)
    {
        Path = name;
        if (node != null)
        {
            Node = node;
        }
        else
        {
            Node = null;
        }
    }

    public RepeaterData(string path)
    {
        Path = path;
        Node = null;

    }

    public string GetPath()
    {
        return Path;
    }
    public XmlNode? GetNode()
    {
        return Node;
    }
    public int getIterationCount()
    {
        return iterationCount;
    }
    public int Iterate()
    {
        iterationCount++;
        return iterationCount - 1;
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

        output.DumpToFile("outputBasic.csv");
    }

    private string getFieldValue(string location, List<XMLFile> processedXmlFiles)
    {
        foreach (var xmlFile in processedXmlFiles)
        {
            var nodes = xmlFile.GetDocument().SelectNodes(location);
            if (nodes != null && nodes.Count > 0)
            {
                return nodes[0].InnerText; // Return the first matching node's text
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
            foreach (XmlNode childNode in node.ChildNodes)
            {
                Serilog.Log.Information($"Processing child node: {childNode.Name}");
                // Here you would implement the logic to handle child nodes
                // For example, you might want to recursively call a method to handle nested tags
                HandleTag(childNode, processedXmlFiles);
            }
        }
        OnExitTag(node, processedXmlFiles);
    }

    private void OnEnterTag(XmlNode node, List<XMLFile> processedXmlFiles)
    {
        if (node.NodeType == XmlNodeType.Element)
        {
            Serilog.Log.Information($"Schema tag found: {node.Name}");
            if (node.Name == "Repeater")
            {
                Serilog.Log.Information("Processing 'Repeater' tag logic here.");
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