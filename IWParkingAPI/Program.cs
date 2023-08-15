using IWParkingAPI;
using NLog.Web;
using System.Diagnostics;

public class Program
{
    public static void Main(string[] args)
    {
        var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
        var pathToContentRoot = Path.GetDirectoryName(pathToExe);

        NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(pathToContentRoot, "nlog.config"));
        var logger = NLog.Web.NLogBuilder.ConfigureNLog(NLog.LogManager.Configuration).GetCurrentClassLogger();
        //var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
        try
        {
            logger.Debug("init main");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Program stopped because of an exception");
            throw;
        }
        finally
        {
            NLog.LogManager.Shutdown();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Information);
        })
        .UseNLog();
}
