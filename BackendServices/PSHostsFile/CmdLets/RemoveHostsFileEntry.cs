using System.Management.Automation;

namespace PSHostsFile.CmdLets
{
    [Cmdlet("remove", "HostsFileEntry")]
    public class RemoveHostsFileEntry : Cmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = @"The hostname to be removed from c:\windows\system32\drivers\etc\hosts", 
            ValueFromPipeline = true, ValueFromRemainingArguments = true)]
        public string HostName;

        protected override void EndProcessing()
        {
            HostsFile.Remove(HostName);
        }
    }
}
