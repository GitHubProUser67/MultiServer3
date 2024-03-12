namespace PSHostsFile
{
    public class HostsFileEntry
    {
        public string Hostname;
        public string Address;

        public HostsFileEntry(string hostname, string address)
        {
            Hostname = hostname;
            Address = address;
        }

        public override string ToString()
        {
            return string.Format("{0}({1}={2})", GetType().Name, Hostname, Address);
        }
    }
}
