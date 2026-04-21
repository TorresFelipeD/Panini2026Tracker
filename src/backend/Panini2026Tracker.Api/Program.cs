using Panini2026Tracker.Application.Albums;
using Panini2026Tracker.Application.Duplicates;
using Panini2026Tracker.Application.Images;
using Panini2026Tracker.Application.Logs;
using Panini2026Tracker.Application.Stickers;
using Panini2026Tracker.Api.Middleware;
using Panini2026Tracker.Domain.Entities;
using Panini2026Tracker.Domain.Repositories;
using Panini2026Tracker.Infrastructure.Configuration;
using Panini2026Tracker.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var appConfigCandidates = new[]
{
    Path.Combine(builder.Environment.ContentRootPath, "app-config.json"),
    Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "app-config.json"))
};

var appConfigPath = appConfigCandidates.FirstOrDefault(File.Exists)
    ?? throw new FileNotFoundException(
        "No se encontro el archivo de configuracion de la aplicacion.",
        "app-config.json");

builder.Configuration.AddJsonFile(appConfigPath, optional: false, reloadOnChange: true);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IStickerService, StickerService>();
builder.Services.AddScoped<IDuplicateService, DuplicateService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "frontend",
        policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync(CancellationToken.None);

    var logRepository = scope.ServiceProvider.GetRequiredService<ISystemLogRepository>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    await logRepository.AddAsync(
        new SystemLog(
            "system",
            "application.started",
            "Backend started successfully.",
            "info",
            DateTime.UtcNow),
        CancellationToken.None);
    await unitOfWork.SaveChangesAsync(CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionLoggingMiddleware>();
app.UseCors("frontend");
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();
app.Run();
