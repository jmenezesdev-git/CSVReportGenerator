class CSVReportOutput : IReportOutput
{

    private List<string> outputLines;

    public CSVReportOutput()
    {
        outputLines = new List<string>();
    }

    public void OnEnterHeader()
    {
        // Logic for entering header
        outputLines.Add("");
    }

    public void OnExitHeader()
    {
        // Logic for exiting header
        outputLines.Add("");
    }

    public void OnEnterField(string value)
    {
        // Logic for entering field
        EnsureOutputLinesAvailable();
        outputLines[outputLines.Count - 1] += "\"" + value + "\"";
    }

    public void OnExitField()
    {
        // Logic for exiting field
    }

    public void OnEnterRepeater()
    {
        // Logic for entering repeater
    }

    public void OnExitRepeater()
    {
        // Logic for exiting repeater
    }

    public void OnEnterTotal()
    {
        // Logic for entering total
    }

    public void OnExitTotal()
    {
        // Logic for exiting total
    }

    public void OnEnterNewLine()
    {
        // Logic for entering new line
    }

    public void OnExitNewLine()
    {
        // Logic for exiting new line
        outputLines.Add("");
    }

    public void OnProcessText(string text)
    {
        EnsureOutputLinesAvailable();
        if (text != null)
        {
            outputLines[outputLines.Count - 1] += text;
        }
    }

    public void DumpToFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            string defaultFileName = "report.csv";
            defaultFileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + defaultFileName;
            System.IO.File.WriteAllLines(defaultFileName, outputLines);
            Serilog.Log.Information($"CSV report dumped to file: {defaultFileName}");
        }
        else
        {
            System.IO.File.WriteAllLines(filePath, outputLines);
            Serilog.Log.Information($"CSV report dumped to file: {filePath}");
        }
    }

    private void EnsureOutputLinesAvailable()
    {
        if (outputLines.Count == 0)
        {
            outputLines.Add("");
        }
    }
}