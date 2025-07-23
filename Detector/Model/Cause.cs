namespace Detector.Model;

public class Cause
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Arg { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
}
