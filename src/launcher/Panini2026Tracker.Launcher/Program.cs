using System.Diagnostics;

var launcherDirectory = AppContext.BaseDirectory;
var appDirectory = Path.Combine(launcherDirectory, "app");
var backendExecutablePath = Path.Combine(appDirectory, "Panini2026Tracker.Api.exe");

if (!File.Exists(backendExecutablePath))
{
    Console.Error.WriteLine($"No se encontro el backend en '{backendExecutablePath}'.");
    return 1;
}

using var backendProcess = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = backendExecutablePath,
        WorkingDirectory = appDirectory,
        UseShellExecute = false
    }
};

Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;

    if (!backendProcess.HasExited)
    {
        backendProcess.Kill(entireProcessTree: true);
    }
};

backendProcess.Start();
await backendProcess.WaitForExitAsync();

return backendProcess.ExitCode;
