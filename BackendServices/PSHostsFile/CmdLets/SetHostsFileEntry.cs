using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSHostsFile.CmdLets
{
    [Cmdlet("set", "HostsFileEntry")]
    public class SetHostsFileEntry : Cmdlet
    {
        [Parameter(HelpMessage = @"The hostname to be added to c:\windows\system32\drivers\etc\hosts",Position = 0)]
        public string? HostName;

        [Parameter(HelpMessage = @"The static IP address to be associated with the host", Position = 1)]
        public string? Address;

        [Parameter(Mandatory = false,
            ValueFromPipeline = true,
            HelpMessage = "Pair of hostname, address")] //, ValidateCount(2,2)]
        public object InputObject
        {
            get { return _input; }
            set { _input = value; }
        }
        private object? _input;

        [Parameter(Mandatory = false, HelpMessage = @"Hosts filepath (defaults to SystemRoot\system32\drivers\etc\hosts).")]
        [Alias("f")]
        public string? FilePath;

        List<HostsFileEntry>? ValuesToSet;

        protected override void BeginProcessing()
        {
            ValuesToSet = new List<HostsFileEntry>();
        }

        protected override void ProcessRecord()
        {
            if (_input == null)
                return;

            if (((PSObject)_input).BaseObject is object[] input)
            {
                ValuesToSet?.Add(new HostsFileEntry((string)input[0], (string)input[1]));
                base.ProcessRecord();
            }
        }

        protected override void EndProcessing()
        {
            if (HostName != null && Address != null)
                ValuesToSet?.Add(new HostsFileEntry(HostName, Address));

            if (ValuesToSet != null && !ValuesToSet.Any())
                throw new Exception("Must pass pass a hostname and address as parameters, or input string must be a contain some number of host/address pairs.");

            HostsFile.Set(ValuesToSet, FilePath);
        }
    }
}
