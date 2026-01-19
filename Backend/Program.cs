using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
});

builder.Services.AddOpenApi();
builder.Services.AddOptions<HLSOptions>()
    .BindConfiguration(nameof(HLSOptions))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<FFMpeg>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Adds the /scalar/v1 endpoint
}

app.UseHttpsRedirection();
app.UseCors();

app.MapGet("test", () => "All working" );

app.MapPost("playlist/create", async (FFMpeg ffmpeg, IOptions<HLSOptions> opt, [FromBody] string mediaFileFullName) =>
{
    Utility.ThrowIfFilePathInvalid(mediaFileFullName, nameof(mediaFileFullName));

    var fileName = Path.GetFileName(mediaFileFullName);
    var fileNameNoExt = Path.GetFileNameWithoutExtension(fileName);

    var inputPath = Path.Combine(opt.Value.ContentInputPath, fileName);

    if (!Path.Exists(inputPath))
        throw new ArgumentException($"${nameof(mediaFileFullName)} file path is not presented in content input folder");

    var outputPath = Path.Combine(opt.Value.ContentOutputPath, fileNameNoExt, fileNameNoExt + ".m3u8");

    if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

    var (Success, Error) = await ffmpeg.CreateHLSStream(inputPath, outputPath);

    var problemDetails = new ProblemDetails
    {
        Status = Success ? 200 : 400,
        Title = "ApiResult",
        Detail = Success ? "HLS Stream Created Successfully" : Error,
        Instance = "/playlist/create"
    };

    return problemDetails.Status switch
    {
        200 => Results.Ok(problemDetails),
        400 => Results.BadRequest(problemDetails),
        _ => Results.InternalServerError("Something really really bad happened")
    };
});

app.MapPost("content/upload", async (IOptions<HLSOptions> opt, IFormFile file) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest("Файл не выбран");

    string filePath = Path.Combine(opt.Value.ContentInputPath, file.FileName);

    using var stream = new FileStream(filePath, FileMode.Create);
    await file.CopyToAsync(stream);

    var problemDetails = new ProblemDetails
    {
        Status = 201,
        Title = "FileUploaded",
        Detail = $"/Content/Input/{file.FileName}",
        Instance = "/content/upload"
    };

    return problemDetails.Status switch
    {
        201 => Results.Created(filePath, problemDetails),
        _ => Results.InternalServerError("Something really really bad happened")
    };
})
.DisableAntiforgery();

app.MapDelete("playlist/delete", async (FFMpeg ffmpeg, IOptions<HLSOptions> opt, [FromBody] string mediaFileFullName) =>
{
    Utility.ThrowIfPathInvalid(mediaFileFullName, nameof(mediaFileFullName));

    var isDeleted = false;

    var matchingDirectories = Directory.EnumerateDirectories(opt.Value.ContentOutputPath, Path.GetFileNameWithoutExtension(mediaFileFullName), SearchOption.AllDirectories);

    foreach (var dirPath in matchingDirectories)
    {
        Directory.Delete(dirPath, recursive: true);
        isDeleted = true;
    }

    var problemDetails = new ProblemDetails
    {
        Status = isDeleted ? 200 : 204,
        Title = "ApiResult",
        Detail = isDeleted ? "HLS Stream Deleted Successfully" : $"HLS Stream Was Not Found",
        Instance = "/playlist/delete"
    };

    return problemDetails.Status switch
    {
        200 => Results.Ok(problemDetails),
        204 => Results.NoContent(),
        _ => Results.InternalServerError("Something really really bad happened")
    };
});

app.MapGet("stream/{*path}", async (IOptions<HLSOptions> opt, string path) =>
{
    string fullPath = Path.GetFullPath(Path.Combine(opt.Value.ContentOutputPath, path));

    if (!fullPath.StartsWith(opt.Value.ContentOutputPath, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        return Results.NotFound();

    string extension = Path.GetExtension(fullPath).ToLower();
    string contentType = extension switch
    {
        ".m3u8" => "application/vnd.apple.mpegurl",
        ".ts"   => "video/mp2t",
        _       => "application/octet-stream"
    };

    return Results.File(fullPath, contentType);
});

app.Run();