public interface IReportOutput
{
    void OnEnterHeader();
    void OnExitHeader();
    void OnEnterField(string value);
    void OnExitField();
    void OnEnterRepeater();
    void OnExitRepeater();
    void OnEnterTotal();
    void OnExitTotal();
    void OnEnterNewLine();
    void OnExitNewLine();
    void OnProcessText(string text);
    void DumpToFile(string filePath);
}