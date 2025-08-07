using System.Xml;

public class XMLFile
{
    private string FileName { get; set; }
    private string FilePath { get; set; }
    private XmlDocument XmlDocument { get; set; }

    public XMLFile(string fileName, string filePath)
    {
        FileName = fileName;
        FilePath = filePath;
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);
        XmlDocument = doc;
    }


    public XmlNodeList GetTags()
    {
        return XmlDocument.ChildNodes;
    }
    public XmlDocument GetDocument()
    {
        return XmlDocument;
    }
    public string GetFileName()
    {
        return FileName;
    }
    public string GetFilePath()
    {
        return FilePath;
    }
}