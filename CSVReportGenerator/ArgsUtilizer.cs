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

    public static XMLFile LoadSchemaFile(string schemaFilePath)
    {
        Serilog.Log.Information($"Verifying existence of schema file: {schemaFilePath}");

        if (!File.Exists(schemaFilePath))
        {
            Serilog.Log.Error($"Output schema file not found: {schemaFilePath}");
            throw new FileNotFoundException("Output schema file not found.", schemaFilePath);
        }

        Serilog.Log.Information($"Output schema file loaded successfully: {schemaFilePath}");
        return new XMLFile(Path.GetFileName(schemaFilePath), schemaFilePath);
    }
}