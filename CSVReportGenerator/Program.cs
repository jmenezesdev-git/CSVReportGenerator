// See https://aka.ms/new-console-template for more information

using System.Xml;
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
/// - Generate secondary data based on schema
/// - generate csv based on schema
/// - utilize sufficent abstraction to allow for substition of output formats such as JSON
/// 
/// DONE:
/// - Implemented Logger
/// - Argument parsing
/// - Import Schema
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
        InitializeLogger();


        //Check Arguments
        if (args.Length < 1)
        {
            Serilog.Log.Error("No output schema file provided.");
            Console.WriteLine("Usage: CSVReportGenerator -outputSchema [<output_schema_file>] -filter [<input_file_filters>] -input [<input_files>] [<input_files2>]");
            return;
        }

        try
        {

            InputArguments inputArgs = InputArguments.Parse(args);
            string outputSchemaFilePath = inputArgs.outputSchemaFile;

            XMLFile outputSchemaFile = ArgsUtilizer.LoadSchemaFile(outputSchemaFilePath);

            List<XMLFile> processedXmlFiles = new List<XMLFile>();
            foreach (var inputPath in inputArgs.inputPathsAndFiles)
            {
                Serilog.Log.Information($"Processing input path: {inputPath}");
                List<string> xmlFiles = ArgsUtilizer.GenerateFileListFromInputArg(inputPath, inputArgs.inputFileFilter);

                if (xmlFiles.Count == 0)
                {
                    Serilog.Log.Warning($"No XML files found in the specified path: {inputPath}");
                    continue;
                }

                foreach (var xmlFile in xmlFiles)
                {
                    Serilog.Log.Information($"Reading XML-like file: {xmlFile}");
                    processedXmlFiles.Add(XMLReader.ReadXMLFile(xmlFile));
                    // Process the XML data as needed
                }
            }

            Serilog.Log.Information("Completed reading XML files.");
            ReportGenerator rg = ReportGenerator.Instance;
            rg.CreateReport(processedXmlFiles, outputSchemaFile, inputArgs.outputFilePath);
            // Next: Generate CSV based on the output schema
        }
        catch (Exception ex)
        {
            TopLevelErrorHandling(ex);
            return;
        }
    }

    /// <summary>
    /// Handles top-level exceptions and logs appropriate messages.
    /// </summary>
    /// <param name="ex"></param>
    private static void TopLevelErrorHandling(Exception ex)
    {
        if (ex is FileNotFoundException || ex is UnauthorizedAccessException)
        {
            Serilog.Log.Fatal(ex, "There was an issue with the file paths provided. Terminating program without processing.");
        }
        else if (ex is UnknownErrorException)
        {
            Serilog.Log.Fatal(ex, "An unknown Fatal Error occurred. Terminating Program.");
        }
        else if (ex is ArgumentException)
        {
            Serilog.Log.Error(ex, "An argument error occurred. Please check your input arguments.");
        }
        else if (ex is NotSupportedException)
        {
            Serilog.Log.Error(ex, "The specified output type is not supported. Please check your output type.");
        }
        else if (ex is XmlException)
        {
            Serilog.Log.Error(ex, "There was an issue processing one of the XML files. Please verify that it is correctly formatted");
        }
        else
        {
            Serilog.Log.Debug("Error Type is: " + ex.GetType().ToString());
            Serilog.Log.Fatal(ex, "An unhandled error occurred. Terminating Program.");
        }
    }

    private static void InitializeLogger()
    {
        Serilog.Log.Logger = new Serilog.LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", retainedFileCountLimit: 10, rollingInterval: Serilog.RollingInterval.Day)
            .CreateLogger();

        Serilog.Log.Information("Serilog initialized."); //Debug, Information, Warning, Error, Fatal

    }
}
//XML Parser
//