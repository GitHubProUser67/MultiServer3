# PSMultiServer
 A Unified Server Software with PlayStation Online games in mind.
 
![image](https://github.com/GitHubProUser67/PSMultiServer/assets/127040195/b4c10194-5e0f-4822-9987-7fd20106fd6b)
 
 You can expect the software in particlar to run many versions of PlayStation Home,
 
 in combination with the JumpSuit's awesome fork of Horizon.
 
 It is very easy to compile and get up and running. No extra configs required aside TCP port management.
 
 You need to open ports : 80 , 10060 , 10071, 10072, 10073, 10075, 10443, 10086.
 
 When it comes to medius, you will need to configure the appids : 20374 and 20371 in DME, MEDIUS, MUIS.
 
 For the variable responses in MUIS, Home will expect this format :
 
 	"20374": [
      {
        "Name": "muis",
        "Description": "44",
        "Endpoint": "YOUR IP",
        "Status": 1,        
        "UserCount": 1,
        "MaxUsers": 15000,
		"SvoURL": "http://homeps3.online.scee.com:10060/HUBPS3_SVML/unity/start.jsp ",
        "ExtendedInfo": "PSHOMEVERSION http://YOUR IP/LINKTOCDN/",
		"UniverseBilling": "SCEA",
		"BillingSystemName": "Sony Computer Entertainment America, Inc. Billing System",
        "Port": 10075,
        "UniverseId": 1,
      }
    ],

This read-me will be improved over time to better illustrate the steps of how to run a Home server.

Please note that this server in a near future will also support many other games related to Medius stuff.

The Horizon fork will probably also makes it's way inside the software for ease of setup.

