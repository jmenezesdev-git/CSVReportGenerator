using System.IO;
using System.Xml;

static class XMLReader
{
    public static XMLFile ReadXMLFile(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var xmlFile = new XMLFile(fileName, filePath);

        //Got AI to write a script to load the relevant contents into custom objects so referencing the values/attributes will be easier later
        // Recursively parse XML nodes
        // foreach (XmlNode node in doc.ChildNodes)
        // {
        //     var tag = ParseXmlNode(node);
        //     if (tag != null)
        //         xmlFile.AddTag(tag);
        // }

        return xmlFile;
    }

    // private static XMLTag ParseXmlNode(XmlNode node)
    // {
    //     if (node.NodeType != XmlNodeType.Element)
    //         return null;

    //     var tag = new XMLTag(node.Name);

    //     // attributes
    //     if (node.Attributes != null)
    //     {
    //         foreach (XmlAttribute attr in node.Attributes)
    //         {
    //             tag.AddAttribute(attr.Name, attr.Value);
    //         }
    //     }

    //     //child tags or text content
    //     foreach (XmlNode child in node.ChildNodes)
    //     {
    //         if (child.NodeType == XmlNodeType.Element)
    //         {
    //             var childTag = ParseXmlNode(child);
    //             if (childTag != null)
    //                 tag.AddChild(childTag);
    //         }
    //         else if (child.NodeType == XmlNodeType.Text || child.NodeType == XmlNodeType.CDATA)
    //         {
    //             if (child.Value != null)
    //             {
    //                 tag.SetTextContent(child.Value);
    //             }
    //         }
    //     }

    //     return tag;
    // }
}