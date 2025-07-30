// See https://aka.ms/new-console-template for more information

using Serilog;

/// <summary>
/// CSVReportGenerator
/// This program generates CSV reports based on XML input files and a specified output schema.
/// It allows for filtering input files and specifying multiple input paths.
/// The program uses Serilog for logging and handles various exceptions related to file paths and access.
/// It requires an output schema file to define the structure of the generated CSV.
/// The input files can be specified with a filter, and if no input paths are provided,
/// the program defaults to the current directory.
/// 
/// TO DO:
/// - Create XML parser
/// - generate csv based on schema
/// - utilize sufficent abstraction to allow for substition of output formats such as JSON
/// 
/// DONE:
/// - Implemented Logger
/// - Argument parsing
/// - Create basic test cases
/// </summary>



//First Argument - Output schema file
//-filter - next arg is used for Matching input file filters
//-input - Input XML file/folder
//If no input path is provided, the program will look for files in the current directory.
//If no filter is provided, the program will use .XML files in the current directory.
class CSVReportGenerator
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        Serilog.Log.Logger = new Serilog.LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", retainedFileCountLimit: 10, rollingInterval: Serilog.RollingInterval.Day)
            .CreateLogger();

        Serilog.Log.Information("Serilog initialized."); //Debug, Information, Warning, Error, Fatal

        //Check Arguments
        if (args.Length < 1)
        {
            Serilog.Log.Error("No output schema file provided.");
            Console.WriteLine("Usage: CSVReportGenerator -outputSchema [<output_schema_file>] -filter [<input_file_filters>] -input [<input_files>]");
            return;
        }

        try
        {

            InputArguments inputArgs = InputArguments.Parse(args);
            string outputSchemaFile = inputArgs.OutputSchemaFile;
            //validate output schema file
            Serilog.Log.Information($"Verifying existence of schema file: {outputSchemaFile}");

            // Check if output schema file exists
            if (!File.Exists(outputSchemaFile))
            {
                Serilog.Log.Error($"Output schema file not found: {outputSchemaFile}");
                return;
            }


        }
        catch (Exception ex)
        {
            if (ex is FileNotFoundException || ex is UnauthorizedAccessException)
            {
                Serilog.Log.Fatal(ex, "There was an issue with the file paths provided. Terminating program without processing.");
            }
            else if (ex is UnknownErrorException)
            {
                Serilog.Log.Fatal(ex, "An unknown Fatal Error occurred. Terminating Program.");
            }
            else
            {
                Serilog.Log.Fatal(ex, "An unhandled error occurred. Terminating Program.");
            }
        }
    }
}
//XML Parser
//