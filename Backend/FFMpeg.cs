using System.Diagnostics;
using Microsoft.Extensions.Options;

internal class FFMpeg(IOptions<HLSOptions> opt)
{
    private readonly IOptions<HLSOptions> _opt = opt;

    public async Task<(bool Success, string Error)> CreateHLSStream(string mp3InputPath, string m3u8OutputPath)
    {
        string error = string.Empty;
        string arguments = $"-i \"{mp3InputPath}\" -c:a aac -b:a 128k -f hls -hls_time 10 -hls_list_size 0 \"{m3u8OutputPath}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = _opt.Value.FFMpegAbsolutePath, // Must be in PATH or provide full path
            Arguments = arguments,
            RedirectStandardError = true, // FFmpeg logs status/progress to stderr
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                if (e.Data.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
                    e.Data.Contains("Invalid", StringComparison.OrdinalIgnoreCase))
                {
                    error = e.Data.Trim();
                }
            }
        };

        process.Start();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode == 0)
            return (true, error);

        return (false, error);
    }
}