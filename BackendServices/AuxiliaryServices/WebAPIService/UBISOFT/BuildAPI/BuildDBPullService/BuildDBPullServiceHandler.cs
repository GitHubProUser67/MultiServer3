﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WebAPIService.UBISOFT.BuildAPI.BuildDBPullService
{
    public class BuildDBPullServiceHandler
    {

        public static string buildDBRequestParser(byte[] PostData, string ContentType)
        {

            // Load the XML string into an XDocument
            XDocument xdoc = XDocument.Parse(Encoding.UTF8.GetString(PostData));

            // Define the namespaces used in the XML document
            XNamespace soap = "http://www.w3.org/2003/05/soap-envelope";
            XNamespace responseNamespace = "http://builddatabasepullapi/";

            var resultElement = xdoc.Descendants(soap + "Body").FirstOrDefault();
            string innerRequestName = resultElement.Name.ToString();

            List<string> buildDbRequests = new List<string>() 
            { "GetCurrentLauncherVersion", "GetConsoleOwner",
                "GetProjectDetailWithMacValidation", "GetFilteredBuildVersionsWithMacValidation",
                "GetProjectsWithMacValidation", "GetFilteredBuildVersionsWithMacValidation"};


            if (buildDbRequests.Contains(innerRequestName))
            {
                switch (innerRequestName) {
                    case "GetCurrentLauncherVersion":
                        {

                            Console.WriteLine($"GetCurrentLauncherVersion: TRIGGERED!");

                            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope
  xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <soap:Body>
    <LauncherVersion
      xmlns=""http://builddatabasepullapi/"">
        <VersionName>2.10.1</VersionName>
        <VersionPath>/BuildDBPullService.asmx</VersionPath>
        <LauncherVersionId>4</LauncherVersionId>
        <PlatformId>4</PlatformId>
    </LauncherVersion>
  </soap:Body>
</soap:Envelope>";
                        }

                    default:
                        {

                            Console.WriteLine($"GetCurrentLauncherVersion: FAILEd!");
                            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope
  xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <soap:Body>
    <LauncherVersion
      xmlns=""http://builddatabasepullapi/"">
        <VersionName>2.10.1</VersionName>
        <VersionPath>/BuildDBPullService.asmx</VersionPath>
        <LauncherVersionId>4</LauncherVersionId>
        <PlatformId>4</PlatformId>
    </LauncherVersion>
  </soap:Body>
</soap:Envelope>";
                        }

                }


                //Console.WriteLine($"ADLogin: {adLogin}");
                //Console.WriteLine($"Name: {name}");
            }
            else
            {
                Console.WriteLine("No GetConsoleOwnerResult found.");
            }

            /*
            // Get the GetConsoleOwner element from the SOAP body
            var resultElement = xdoc.Descendants(soap + "Body").FirstOrDefault();

            if (buildDbRequests.Contains(resultElement.Name.ToString()))
            {
                // Extract the ADLogin and Name elements
                string adLogin = resultElement.Element("ADLogin")?.Value;
                string name = resultElement.Element("Name")?.Value;

                Console.WriteLine($"ADLogin: {adLogin}");
                Console.WriteLine($"Name: {name}");
            }
            else
            {
                Console.WriteLine("No GetConsoleOwnerResult found.");
            }

            */


            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope
  xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <soap:Body>
    <LauncherVersion
      xmlns=""http://builddatabasepullapi/"">
        <VersionName>2.10.2</VersionName>
        <VersionPath>/BuildDBPullService.asmx</VersionPath>
        <LauncherVersionId>4</LauncherVersionId>
        <PlatformId>4</PlatformId>
    </LauncherVersion>
  </soap:Body>
</soap:Envelope>";


        }


        public static string getVersion(byte[] PostData, string ContentType) {


            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope
  xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <soap:Body>
    <GetConsoleOwnerResponse
      xmlns=""http://builddatabasepullapi/"">
      <GetConsoleOwnerResult>
        <ADLogin>hensley.edwin</ADLogin>
        <Name>Hensley Edwin</Name>
      </GetConsoleOwnerResult>
    </GetConsoleOwnerResponse>
  </soap:Body>
</soap:Envelope>";
        }

        public static string getConsoleOwner(byte[] PostData, string ContentType)
        {


            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope
  xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <soap:Body>
    <GetConsoleOwnerResponse
      xmlns=""http://builddatabasepullapi/"">
      <GetConsoleOwnerResult>
        <ADLogin>hensley.edwin</ADLogin>
        <Name>Hensley Edwin</Name>
      </GetConsoleOwnerResult>
    </GetConsoleOwnerResponse>
  </soap:Body>
</soap:Envelope>";
        }
    }
}
