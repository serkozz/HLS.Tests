using System.ComponentModel.DataAnnotations;

public record HLSOptions([property: Required] string FFMpegAbsolutePath, [property: Required] string ContentInputPath, [property: Required] string ContentOutputPath)
{
    public HLSOptions() : this(default!, default!, default!) { }
}