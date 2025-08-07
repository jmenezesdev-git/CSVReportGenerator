public class ReportOutputFactory
{
    public static IReportOutput CreateReportOutput(string outputType)
    {
        if (outputType.Equals("CSV", StringComparison.OrdinalIgnoreCase))
        {
            return new CSVReportOutput();
        }
        else
        {
            throw new NotSupportedException($"Output type '{outputType}' is not supported.");
        }
    }
}