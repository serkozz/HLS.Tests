internal static class Utility
{
    public static void ThrowIfPathInvalid(string path, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path, paramName);

        if (path.ContainsAny(Path.GetInvalidPathChars()))
            throw new ArgumentException($"Invalid characters in ${paramName} string");
    }

    public static void ThrowIfFilePathInvalid(string path, string paramName)
    {
        ThrowIfPathInvalid(path, paramName);

        if (!Path.HasExtension(path))
            throw new ArgumentException($"${paramName} string has no .extension");
    }
}