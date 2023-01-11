using System.ServiceProcess;

internal class Program
{
    private static ServiceController GetService(string serviceName) => new ServiceController(serviceName);

    private static void Main(string[] args)
    {
        Console.Clear();
        Console.WriteLine("Service Go...");
        
        var monitoredServices = GetServiceToMonitor();
        var services = new List<ServiceController>();

        foreach(var service in monitoredServices)
        {
            var getService = GetService(service);
            services.Add(getService);
            Console.WriteLine();
            Console.ForegroundColor= ConsoleColor.White;
            Console.Write("Monitoring service : ");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{getService.DisplayName}");
        }

        Console.WriteLine("Monitoring service status.");
        while (true)
        {
            foreach (var service in services)
            {
                service.Refresh();
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Service: [{service.DisplayName}] - Status : [{service.Status}]");
                    service.Start();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Attempting to start service => {service.DisplayName}");

                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));

                    if ( service.Status != ServiceControllerStatus.Running)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Service: [{service.DisplayName}] - Status : [{service.Status}] - FAILED TO RESTART!");
                    }

                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Service: [{service.DisplayName}] - Status : [{service.Status}]");
                    }
                }
            }

            Thread.Sleep(5000);
        }

    }

    private static List<string> GetServiceToMonitor()
    {
        return new List<string>()
        {
            "SAMSUNG Mobile Connectivity Service",
            "Server"
        };
    }

   
}