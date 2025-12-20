using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public static class SystemInfo
{
    public static object GetReport(string? description)
    {
        return new
        {
            description = description,
            hostName = Dns.GetHostName(),
            ipAddress = GetLocalIPv4(),
            macAddress = GetMacAddress()
        };
    }

    private static string GetLocalIPv4()
    {
        return Dns.GetHostAddresses(Dns.GetHostName())
            .First(x => x.AddressFamily == AddressFamily.InterNetwork)
            .ToString();
    }

    private static string GetMacAddress()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .First(n =>
                n.OperationalStatus == OperationalStatus.Up &&
                n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .GetPhysicalAddress()
            .ToString();
    }
}
