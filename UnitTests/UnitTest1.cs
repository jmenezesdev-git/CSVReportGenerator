using System.Net.WebSockets;
using System.Reflection;
using Moq;

namespace UnitTests;

public class UnitTest1
{

    //This theory tests the example files and some args for them to ensure that InputArguments.Parse is working correctly.
    [Theory]
    [MemberData(nameof(TestDataInputArgs1))]
    public void TestInputArgs1(string[] args, string expectedOutputSchemaFile, string expectedInputFileFilter, string[] expectedInputPathsAndFiles, string expectedOutputFilePath)
    {
        IInputArguments inputArgs = InputArguments.Instance;


        InputArgument inputArg = new InputArgument(inputArgs);
        object result = typeof(InputArguments).GetMethod("ResetForTesting", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(inputArgs, null);

        inputArg.Parse(args);

        Assert.Equal(expectedOutputSchemaFile, inputArg.OutputSchemaFile);
        Assert.Equal(expectedInputFileFilter, inputArg.InputFileFilter);
        Assert.Equal(expectedInputPathsAndFiles, inputArg.InputPathsAndFiles);
        Assert.Equal(expectedOutputFilePath, inputArg.OutputFilePath);
    }

    //This theory establishes a Mock of InputArgs allowing us to define them in an easier manner without having to process the CL args.
    [Theory]
    [MemberData(nameof(TestDataInputArgs1))]
    public void TestInputFollowthrough(string[] args, string outputSchemaFile, string inputFileFilter, string[] inputPathsAndFiles, string outputFilePath)
    {
        var mockService = new Mock<IInputArguments>();
        mockService.Setup(m => m.outputSchemaFile).Returns(outputSchemaFile);
    //     mockService.Setup(m => m.inputFileFilter).Returns(inputFileFilter);
    //     mockService.Setup(m => m.inputPathsAndFiles).Returns(inputPathsAndFiles.ToList());
    //     mockService.Setup(m => m.outputFilePath).Returns(outputFilePath);

        InputArgument inputArgX = new InputArgument(mockService.Object);

    //     // inputArg.Parse(args);
        Assert.Equal(outputSchemaFile, inputArgX.OutputSchemaFile);

    //     // mockService.Verify(m => m.Parse(args), Times.Once);
        
    }

    public static IEnumerable<object[]> TestDataInputArgs1()
    {
        string cwd = Environment.CurrentDirectory;

        yield return new object[]
        {
            new string[] { "-outputSchema", cwd + "/Examples/Single/SingleTestSchema.xml", "-input", cwd + "/Examples/Single/xmlFile1.xml" },
            new string(cwd + "/Examples/Single/SingleTestSchema.xml"),
            null,
            new string[] { cwd + "/Examples/Single/xmlFile1.xml" },
            null
        };
        yield return new object[]
        {
            new string[] { "-outputSchema", cwd + "/Examples/Basic/BasicTestSchema.xml", "-input", cwd + "/Examples/Basic/xmlFile1.xml" },
            new string(cwd + "/Examples/Basic/BasicTestSchema.xml"),
            null,
            new string[] { cwd + "/Examples/Basic/xmlFile1.xml" },
            null
        };
        yield return new object[]
        {
            new string[] { "-outputSchema", cwd + "/Examples/Intermediate/IntermediateTestSchema.xml", "-input", cwd + "/Examples/Intermediate/xmlFile.xml" },
            new string(cwd + "/Examples/Intermediate/IntermediateTestSchema.xml"),
            null,
            new string[] { cwd + "/Examples/Intermediate/xmlFile.xml" },
            null
        };
        yield return new object[]
        {
            new string[] { "-outputSchema", cwd + "/Examples/Advanced/AdvancedTestSchema.xml", "-input", cwd + "/Examples/Advanced/testFolder" },
            new string(cwd + "/Examples/Advanced/AdvancedTestSchema.xml"),
            null,
            new string[] { cwd + "/Examples/Advanced/testFolder" },
            null
        };
        yield return new object[]
        {
            new string[] { "-outputSchema", cwd + "/Examples/Complete/CompleteTestSchema.xml", "-input", cwd + "/Examples/Complete/testFolder", cwd + "/Examples/Complete/testFolder2" },
            new string(cwd + "/Examples/Complete/CompleteTestSchema.xml"),
            null,
            new string[] { cwd + "/Examples/Complete/testFolder", cwd + "/Examples/Complete/testFolder2" },
            null
        };
        //The above cases minimally test the example folders
        //This case tests the output path conditional
        yield return new object[]
        {
            new string[] { "-outputSchema", cwd + "/Examples/Complete/CompleteTestSchema.xml", "-input", cwd + "/Examples/Complete/testFolder", cwd + "/Examples/Complete/testFolder2", "-output", cwd + "/Test123/output.csv" },
            new string(cwd + "/Examples/Complete/CompleteTestSchema.xml"),
            null,
            new string[] { cwd + "/Examples/Complete/testFolder", cwd + "/Examples/Complete/testFolder2" },
            new string(cwd + "/Test123/output.csv")
        };
        //This case tests the filter argument
        yield return new object[]
        {
            new string[] { "-filter", "*.xmlinfo", "-outputSchema", cwd + "/Examples/Complete/CompleteTestSchema.xml", "-input", cwd + "/Examples/Complete/testFolder", cwd + "/Examples/Complete/testFolder2", "-output", cwd + "/Test123/output.csv" },
            new string(cwd + "/Examples/Complete/CompleteTestSchema.xml"),
            new string("*.xmlinfo"),
            new string[] { cwd + "/Examples/Complete/testFolder", cwd + "/Examples/Complete/testFolder2" },
            new string(cwd + "/Test123/output.csv")
        };
        //This case tests that order is irrelevant
        yield return new object[]
        {
            new string[] {"-input", cwd + "/Examples/Complete/testFolder", cwd + "/Examples/Complete/testFolder2", "-output", cwd + "/Test123/output.csv",  "-outputSchema", cwd + "/Examples/Complete/CompleteTestSchema.xml", "-filter", "*.xmlinfo" },
            new string(cwd + "/Examples/Complete/CompleteTestSchema.xml"),
            new string("*.xmlinfo"),
            new string[] { cwd + "/Examples/Complete/testFolder", cwd + "/Examples/Complete/testFolder2" },
            new string(cwd + "/Test123/output.csv")
        };
    }
}
