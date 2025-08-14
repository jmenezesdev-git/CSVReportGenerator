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

    public TotalRepeaterData(string path) //For File Iterators
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
}