using System.Xml;

public enum RepeaterType
{
    Basic,
    File,
    Total,
    SpecialTotal
}

public interface IRepeaterData
{
    string GetPath();
    List<XmlNode>? GetNodes(); //Flagged for removal
    int GetIterationCount();
    int Iterate();
    RepeaterType GetRepeaterType();
    string GetFieldValue(string location, List<XMLFile> processedXmlFiles, Stack<IRepeaterData> otherRepeaterDataStack);

    public static int GetFirstNonNodeRepeaterIteration(Stack<IRepeaterData> otherRepeaterDataStack)
    {
        foreach (var repeater in otherRepeaterDataStack)
        {
            if (repeater.GetRepeaterType() == RepeaterType.File)
            {
                return repeater.GetIterationCount();
            }
        }
        return -1; // Meaningful value indicating no non-node repeater found
    }
}

public class RepeaterDataFactory
{
    public static IRepeaterData CreateRepeaterData(RepeaterType outputType, string path, XmlNode node = null, int iterationMax = -1)
    {
        if (outputType.Equals(RepeaterType.Total))
        {
            return new TotalRepeaterData(path);
        }
        else if (outputType.Equals(RepeaterType.SpecialTotal))
        {
            return new TotalRepeaterData(path, RepeaterType.SpecialTotal);
        }
        else if (outputType.Equals(RepeaterType.File) && node == null && iterationMax != -1)
        {
            return new FileRepeaterData(path, iterationMax);
        }
        else if (outputType.Equals(RepeaterType.Basic) && node != null && iterationMax != -1)
        {
            return new BasicRepeaterData(path, node, iterationMax);
        }
        else
        {
            throw new NotSupportedException($"Output type '{outputType}' is not supported.");
        }
    }

    public static IRepeaterData CreateRepeaterData(RepeaterType outputType, string path, int iterationMax = -1)
    {
        if (outputType.Equals(RepeaterType.Total))
        {
            return new TotalRepeaterData(path);
        }
        else if (outputType.Equals(RepeaterType.SpecialTotal))
        {
            return new TotalRepeaterData(path, RepeaterType.SpecialTotal);
        }
        else if (outputType.Equals(RepeaterType.File) && iterationMax != -1)
        {
            return new FileRepeaterData(path, iterationMax);
        }
        else if (outputType.Equals(RepeaterType.Basic))
        {
            throw new NotSupportedException("Basic repeater data requires a node and iteration max.");
        }
        else
        {
            throw new NotSupportedException($"Output type '{outputType}' is not supported.");
        }
    }

    public static IRepeaterData CreateRepeaterData(RepeaterType outputType, string path)
    {
        if (outputType.Equals(RepeaterType.Total))
        {
            return new TotalRepeaterData(path);
        }
        else if (outputType.Equals(RepeaterType.SpecialTotal))
        {
            return new TotalRepeaterData(path, RepeaterType.SpecialTotal);
        }
        if (outputType.Equals(RepeaterType.File))
        {
            throw new NotSupportedException("File repeater data requires a file count.");
        }
        if (outputType.Equals(RepeaterType.Basic))
        {
            throw new NotSupportedException("Basic repeater data requires a node and iteration max.");
        }
        else
        {
            throw new NotSupportedException($"Output type '{outputType}' is not supported.");
        }
    }
}

public class TotalRepeaterData : IRepeaterData
{
    private string Path { get; set; }
    private RepeaterType repeaterType;

    public TotalRepeaterData(string path)
    {
        Path = path;
        repeaterType = RepeaterType.Total;
    }
    public TotalRepeaterData(string path, RepeaterType type) //For File Iterators
    {
        Path = path;
        repeaterType = type;
    }

    public string GetPath()
    {
        return Path;
    }
    public List<XmlNode> GetNodes()
    {
        return null;
    }
    public int GetIterationCount()
    {
        return 0;
    }
    public int Iterate()
    {
        return -1;
    }


    public RepeaterType GetRepeaterType()
    {
        return repeaterType;
    }

    public string GetFieldValue(string location, List<XMLFile> processedXmlFiles, Stack<IRepeaterData> otherRepeaterDataStack)
    {

        if (GetRepeaterType() == RepeaterType.Total)
            {
                int fileIterationIndex = -1;
                fileIterationIndex = IRepeaterData.GetFirstNonNodeRepeaterIteration(otherRepeaterDataStack);
                if (otherRepeaterDataStack.Peek().GetIterationCount() != -1)
                {
                    // Build a new location string by splicing in repeater iteration indices
                    string updatedLocation = location;
                    foreach (var repeater in otherRepeaterDataStack)
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

                        if (location.Contains(otherRepeaterDataStack.Peek().GetPath()))
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
            else if (GetRepeaterType() == RepeaterType.SpecialTotal)
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


        return "";
    }
}

public class FileRepeaterData : IRepeaterData
{
    private string Path { get; set; }
    private int iterationCount = 0;

    private int iterationMax = 0;


    public FileRepeaterData(string path, int fileCount) //For File Iterators
    {
        Path = path;
        iterationMax = fileCount - 1;
    }

    public string GetPath()
    {
        return Path;
    }
    public List<XmlNode> GetNodes()
    {
        return null;
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

    public RepeaterType GetRepeaterType()
    {
        return RepeaterType.File;
    }

    public string GetFieldValue(string location, List<XMLFile> processedXmlFiles, Stack<IRepeaterData> otherRepeaterDataStack)
    {
        var nodes = processedXmlFiles[otherRepeaterDataStack.Peek().GetIterationCount()].GetDocument().SelectNodes(location);
        if (nodes != null && nodes.Count > 0)
        {
            return nodes[0].InnerText;
        }
        return "";
    }
}

public class BasicRepeaterData : IRepeaterData
{
    private string Path { get; set; }
    private List<XmlNode>? Nodes { get; set; }
    private int iterationCount = 1;

    private int iterationMax = 0;

    public BasicRepeaterData(string name, XmlNode node, int fileLocalMax) //For location Iterators
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

    public RepeaterType GetRepeaterType()
    {
        return RepeaterType.Basic;
    }

    public string GetFieldValue(string location, List<XMLFile> processedXmlFiles, Stack<IRepeaterData> otherRepeaterDataList)
    {
        int fileIterationIndex = -1;
        fileIterationIndex = IRepeaterData.GetFirstNonNodeRepeaterIteration(otherRepeaterDataList);
        if (otherRepeaterDataList.Peek().GetIterationCount() != -1)
        {
            // Build a new location string by splicing in repeater iteration indices
            string updatedLocation = location;
            foreach (var repeater in otherRepeaterDataList)
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
        return "";
    }
}