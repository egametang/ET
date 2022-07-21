using SyncCodesService;

if(args.Length > 0)
{
    Args.WorkPlace = args[0];
}

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();

public class Args
{
    public static string WorkPlace { get; set; }
}