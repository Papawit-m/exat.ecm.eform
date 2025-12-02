namespace EXAT.ECM.EService.API.Model.Configuration
{
    public class NetworkInterfaceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool DhcpEnabled { get; set; }
        public List<string> IPv4Addresses { get; set; } = new();
        public List<string> IPv6Addresses { get; set; } = new();
        public List<string> SubnetMasks { get; set; } = new();
        public List<string> DefaultGateways { get; set; } = new();
        public List<string> DnsServers { get; set; } = new();
        public string DnsSuffix { get; set; } = string.Empty;
    }
}
