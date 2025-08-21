
using System.Runtime.CompilerServices;

public interface IInputArguments
{
    string outputSchemaFile { get; }
    string inputFileFilter { get; }
    List<string> inputPathsAndFiles { get; }
    string outputFilePath { get; }
    void Parse(string[] args);

}

public class InputArgument
{
    private readonly IInputArguments _inputArguments;

    public InputArgument(IInputArguments inputArguments)
    {
        _inputArguments = inputArguments;
    }

    public void Parse(string[] args)
    {
        _inputArguments.Parse(args);
    }

    public string OutputSchemaFile => _inputArguments.outputSchemaFile;
    public string InputFileFilter => _inputArguments.inputFileFilter;
    public List<string> InputPathsAndFiles => _inputArguments.inputPathsAndFiles;
    public string OutputFilePath => _inputArguments.outputFilePath;
}

public sealed class InputArguments : IInputArguments
{
    private static readonly InputArguments _instance = new InputArguments();

    private InputArguments() { }

    public static InputArguments Instance => _instance;

    public string outputSchemaFile { get; set; }
    public string inputFileFilter { get; set; }
    public List<string> inputPathsAndFiles { get; set; }
    public string outputFilePath { get; set; } // Default output file path

    private void ResetForTesting()
    {
        outputSchemaFile = null;
        inputFileFilter = null;
        inputPathsAndFiles = new List<string>();
        outputFilePath = null;
    }
    public void Parse(string[] args)
    {
        Serilog.Log.Debug("Parsing input arguments.");
        var instance = Instance;

        ParseArgs(instance, args);

        Serilog.Log.Information("Parsing input arguments completed.");

        try
        {
            ValidateArgPaths(instance);
        }
        catch (Exception ex)
        {
            if (ex is FileNotFoundException || ex is UnauthorizedAccessException || ex is DirectoryNotFoundException)
            {
                throw new FileNotFoundException("Argument Path Validation Failed", ex);
            }
            else
            {
                throw new UnknownErrorException("An unknown error occurred during argument path validation.", ex);
            }
        }
        Serilog.Log.Information("File/Folder validation completed.");

        //return instance;
    }

    private static void ValidateArgPaths(InputArguments instance)
    {
        if (string.IsNullOrWhiteSpace(instance.outputSchemaFile) || !File.Exists(instance.outputSchemaFile))
        {
            Serilog.Log.Fatal($"Output schema file not found: {instance.outputSchemaFile}");
            throw new FileNotFoundException($"Output schema file not found: {instance.outputSchemaFile}");
        }

        if (!string.IsNullOrEmpty(instance.outputFilePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(instance.outputFilePath) ?? string.Empty);
        }

        if (instance.inputPathsAndFiles == null || instance.inputPathsAndFiles.Count == 0)
        {
            Serilog.Log.Information("No input files or folders specified. Using current directory.");
            instance.inputPathsAndFiles = [Directory.GetCurrentDirectory()];
        }

        foreach (var path in instance.inputPathsAndFiles)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Serilog.Log.Fatal($"Unable to utilize input path: {path}");
                throw new FileNotFoundException($"Input file or folder not found: {path}");
            }
        }
    }

    private static void ParseArgs(InputArguments instance, string[] args)
    {
        Serilog.Log.Debug("Parsing input arguments.");
        instance.inputPathsAndFiles = new List<string>();

        // Parse arguments
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-outputSchema" && i + 1 < args.Length)
            {
                instance.outputSchemaFile = args[i + 1];
                i++;
            }
            else if (args[i] == "-filter" && i + 1 < args.Length)
            {
                instance.inputFileFilter = args[i + 1];
                i++;
            }
            else if (args[i] == "-input" && i + 1 < args.Length)
            {
                int j = i + 1;
                while (j < args.Length && !args[j].StartsWith("-"))
                {
                    instance.inputPathsAndFiles.Add(args[j]);
                    j++;
                }
                i = j - 1;
            }
            else if (args[i] == "-output" && i + 1 < args.Length)
            {
                instance.outputFilePath = args[i + 1];
                i++;
            }
        }

    }
}
