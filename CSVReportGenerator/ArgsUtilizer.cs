using System.CodeDom.Compiler;
using System.Text.RegularExpressions;

static class ArgsUtilizer
{
    public static List<string> GenerateFileListFromInputArg(string filePath, string inputFileFilter)
    {
        List<string> fileList = new List<string>();

        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.");
        }

        if (Directory.Exists(filePath))
        {
            if (string.IsNullOrEmpty(inputFileFilter))
            {
                fileList.AddRange(Directory.GetFiles(filePath, "*.xml"));
            }
            else
            {
                Regex rx = new Regex(inputFileFilter);
                //fileList.AddRange(Directory.GetFiles(filePath, inputFileFilter));
                fileList.AddRange(Directory.GetFiles(filePath, inputFileFilter).Where(f => rx.IsMatch(Path.GetFileName(f))));
            }
        }
        else if (File.Exists(filePath))
        {
            if (string.IsNullOrEmpty(inputFileFilter) && Path.GetExtension(filePath).Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                fileList.Add(filePath);
            }
            else if (Regex.IsMatch(Path.GetFileName(filePath), inputFileFilter))
            {
                fileList.Add(filePath);
            }
        }
        else
        {
            throw new FileNotFoundException("The specified file or directory does not exist.", filePath);
        }

        return fileList;
    }
}