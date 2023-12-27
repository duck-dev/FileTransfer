using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;
using FileTransfer.UtilityCollection;

namespace FileTransfer;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task Main(string[] args)
    {
        if(args.Length > 0)
            Utilities.IPKey = args[0];
        else
        {
            string ipKeyPath = Path.Combine(Utilities.ExecutingAssemblyPath, "IPKey.txt");
            if (File.Exists(ipKeyPath))
            {
                string encryptedIpKey = await File.ReadAllTextAsync(ipKeyPath);
                string finalIpKey = string.Empty;
                foreach (char c in encryptedIpKey)
                {
                    if (char.IsNumber(c))
                        finalIpKey += Utilities.Cipher(c);
                    else
                        finalIpKey += c;
                }

                Utilities.IPKey = finalIpKey;
            }
            else
            {
                throw new IOException($"File '{ipKeyPath}' doesn't exist!");
            }
        }
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}