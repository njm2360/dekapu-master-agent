using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public record AgentInfo(
    string? Description,
    string HostName,
    string IpAddress,
    string MacAddress
)
{
    public static AgentInfo Collect(string? description)
    {
        var nic = SelectPrimaryInterface();

        var ip = nic.GetIPProperties().UnicastAddresses
            .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
            ?.Address.ToString()
            ?? throw new InvalidOperationException(
                "IPv4 address not found on primary NIC: " + nic.Description);

        var mac = nic.GetPhysicalAddress().ToString();

        return new AgentInfo(
            Description: description,
            HostName: Dns.GetHostName(),
            IpAddress: ip,
            MacAddress: mac
        );
    }

    private static NetworkInterface SelectPrimaryInterface()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up)
            .Where(n => n.NetworkInterfaceType is NetworkInterfaceType.Ethernet
                                                or NetworkInterfaceType.Wireless80211)
            .Where(n => !IsVirtualAdapter(n))
            .FirstOrDefault(n => n.GetIPProperties().UnicastAddresses
                .Any(a => a.Address.AddressFamily == AddressFamily.InterNetwork))
            ?? throw new InvalidOperationException(
                "No active physical network interface with IPv4 found");
    }

    private static bool IsVirtualAdapter(NetworkInterface nic)
    {
        var d = nic.Description;
        return d.Contains("Virtual", StringComparison.OrdinalIgnoreCase)
            || d.Contains("Hyper-V", StringComparison.OrdinalIgnoreCase)
            || d.Contains("VMware", StringComparison.OrdinalIgnoreCase)
            || d.Contains("VirtualBox", StringComparison.OrdinalIgnoreCase)
            || d.Contains("WSL", StringComparison.OrdinalIgnoreCase)
            || d.Contains("Tunneling", StringComparison.OrdinalIgnoreCase)
            || d.Contains("Loopback", StringComparison.OrdinalIgnoreCase);
    }
}
