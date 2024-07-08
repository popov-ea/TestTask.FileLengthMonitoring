using TestTask.FileLengthMonitoring.Models;

namespace TestTask.FileLengthMonitoring.Validators;
public class CommandLineOptionsValidator
{
    public IEnumerable<string> GetValidationErrors(CommandLineOptions options)
    {
        var errors = new List<string>();
        if (!Directory.Exists(options.InputFilesDirPath))
        {
            errors.Add("Input directory does not exist");
        }
        return errors;
    }
}
