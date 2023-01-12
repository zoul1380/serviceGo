using System.Security.Principal;
using System.ServiceProcess;
using System.Text.Json;

internal class Program
{
    private static ServiceController GetService(string serviceName) => new ServiceController(serviceName);

    private static void Main(string[] args)
    {
        var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        if (!isAdmin)
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine("Please run me as an administrator.");
            Console.ReadLine();
            return;
        }

        var jsonFile = "services.json";
        if (args.Length > 0)
        {
            if (File.Exists(args[0]) == true)
            {
                jsonFile = args[0]; 
            }
        }
     
        var monitorServices = JsonSerializer.Deserialize<List<RegisteredServices>>(File.ReadAllText(jsonFile));

        Console.Clear();

        //var monitoredServices = GetServiceToMonitor();
        var services = new List<ServiceController>();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("----------------------");
        Console.WriteLine("= Monitoring service =");
        Console.WriteLine("----------------------");
        Console.WriteLine();

        foreach (var service in monitorServices)
        {
            if (service.IsMonitored)
            {
                var getService = GetService(service.Name);
                services.Add(getService);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{DateTime.Now.ToShortTimeString()} -> {getService.DisplayName}");
            }
        }
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Monitoring services...");
        Console.WriteLine();

        while (isAdmin == true)
        {
            foreach (var service in services)
            //Parallel.ForEach(services, service =>
            {
                service.Refresh();
                if (service.Status == ServiceControllerStatus.Stopped && isAdmin)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{DateTime.Now.ToShortTimeString()} - Service: [{service.DisplayName}] - Status : [{service.Status}]");
                    service.Start();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{DateTime.Now.ToShortTimeString()} - Attempting to start service => {service.DisplayName}");

                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));

                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"{DateTime.Now.ToShortTimeString()} - Service: [{service.DisplayName}] - Status : [{service.Status}] - FAILED TO RESTART!");
                    }

                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{DateTime.Now.ToShortTimeString()} - Service: [{service.DisplayName}] - Status : [{service.Status}]");
                    }
                }
                
            }
            //);
            Thread.Sleep(3000);
        }
    }

    private static List<string> GetServiceToMonitor()
    {
        return new List<string>()
        {
            "AspContext",
            "aspnet_state",
            "PageUp.AWSCredentialsGenerator",
            "PanGPS"
        };
    }
}

public class RegisteredServices
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool IsMonitored { get; set; }
}
