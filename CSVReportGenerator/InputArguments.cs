
public sealed class InputArguments
{
    private static readonly InputArguments _instance = new InputArguments();

    private InputArguments() { }

    public static InputArguments Instance => _instance;

    public string OutputSchemaFile { get; set; }
    public string InputFileFilter { get; set; }
    public List<string> InputFiles { get; set; }

    public static InputArguments Parse(string[] args)
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
            if (ex is FileNotFoundException || ex is UnauthorizedAccessException)
            {
                throw new FileNotFoundException("Argument Path Validation Failed", ex);
            }
            else
            {
                throw new UnknownErrorException("An unknown error occurred during argument path validation.", ex);
            }
        }
        Serilog.Log.Information("File/Folder validation completed.");

        return instance;
    }

    private static void ValidateArgPaths(InputArguments instance)
    {
        if (string.IsNullOrWhiteSpace(instance.OutputSchemaFile) || !File.Exists(instance.OutputSchemaFile))
        {
            Serilog.Log.Fatal($"Output schema file not found: {instance.OutputSchemaFile}");
            throw new FileNotFoundException($"Output schema file not found: {instance.OutputSchemaFile}");
        }

        if (instance.InputFiles == null || instance.InputFiles.Count == 0)
        {
            Serilog.Log.Information("No input files or folders specified. Using current directory.");
            instance.InputFiles = [Directory.GetCurrentDirectory()];
        }

        foreach (var path in instance.InputFiles)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Serilog.Log.Fatal($"Unable to utilize input path: {path}");
                throw new FileNotFoundException($"Input file or folder not found: {path}");
            }
        }
    }

    private static void ParseArgs(InputArguments instance,string[] args)
    {
        Serilog.Log.Debug("Parsing input arguments.");
        instance.InputFiles = new List<string>();

        // Parse arguments
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-outputSchema" && i + 1 < args.Length)
            {
                instance.OutputSchemaFile = args[i + 1];
                i++;
            }
            else if (args[i] == "-filter" && i + 1 < args.Length)
            {
                instance.InputFileFilter = args[i + 1];
                i++;
            }
            else if (args[i] == "-input" && i + 1 < args.Length)
            {
                int j = i + 1;
                while (j < args.Length && !args[j].StartsWith("-"))
                {
                    instance.InputFiles.Add(args[j]);
                    j++;
                }
                i = j - 1;
            }
        }

    }
}
