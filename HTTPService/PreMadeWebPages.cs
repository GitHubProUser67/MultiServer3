namespace MultiServer.HTTPService
{
    public class PreMadeWebPages
    {
        public static string homepage(string clientip)
        {
            if (ServerConfiguration.EnableHomeTools && ServerConfiguration.IsIPAllowed(clientip))
            {
                return "<!DOCTYPE html>\r\n<html lang=\"en\"><head><meta http-equiv=\"Content-Type\"" +
                " content=\"text/html; charset=UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
                "    <title>MultiServer</title>\r\n</head>\r\n<body>    \r\n<div class=\"text-center\">\r\n    <center>\r\n" +
                "    <h1 class=\"display-4\">Welcome to MultiServer</h1>\r\n    \r\n    <br>\r\n    <p>Please select one of the following:</p>\r\n" +
                "    <ul style=\"list-style-type: none; padding-left: 0;\">\r\n        <li><a href=\"/!videoplayer/\">Watch Video</a></li>\r\n" +
                "        <li><a href=\"/!HomeTools/MakeBarSdat/\">ReBar</a></li>\r\n        <li><a href=\"/!HomeTools/UnBar/\">UnBar</a></li>\r\n" +
                "        <li><a href=\"/!HomeTools/ChannelID/\">ChannelID - Calculator</a></li>\r\n     <li><a href=\"/!HomeTools/SceneID/\">SceneID - Calculator</a></li>\r\n" +
                "        <li><a href=\"/!HomeTools/INF/\">INF Tool</a></li>\r\n         <li><a href=\"/!HomeTools/CDS/\">Content Delivery System helper</a></li>\r\n" +
                "    </ul>\r\n\t<footer b-fqjebnty5p=\"\" class=\"border-top footer text-muted\">\r\n        <div b-fqjebnty5p=\"\" class=\"container\">\r\n" +
                "            © 2023 - Home Laboratory\r\n        </div>\r\n    </footer>\r\n    </center>\r\n</div>\r\n</body>\r\n</html>";
            }
            else
            {
                return "<!DOCTYPE html>\r\n<html lang=\"en\"><head><meta http-equiv=\"Content-Type\"" +
                " content=\"text/html; charset=UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
                "    <title>MultiServer</title>\r\n</head>\r\n<body>    \r\n<div class=\"text-center\">\r\n    <center>\r\n" +
                "    <h1 class=\"display-4\">Welcome to MultiServer</h1>\r\n    \r\n    <br>\r\n    <p>Please select one of the following:</p>\r\n" +
                "    <ul style=\"list-style-type: none; padding-left: 0;\">\r\n        <li><a href=\"/!videoplayer/\">Watch Video</a></li>\r\n" +
                "    </ul>\r\n\t<footer b-fqjebnty5p=\"\" class=\"border-top footer text-muted\">\r\n        <div b-fqjebnty5p=\"\" class=\"container\">\r\n" +
                "            © 2023 - Home Laboratory\r\n        </div>\r\n    </footer>\r\n    </center>\r\n</div>\r\n</body>\r\n</html>";
            }
        }

        public static string MakeBarSdat = "<!DOCTYPE html>\n" +
            "<html>\n" +
            "<head>\n" +
            "  <title>Home Tools</title>\n" +
            "  <style>\n" +
            "    body {\n" +
            "      margin: 0;\n" +
            "      padding: 0;\n" +
            "      display: flex;\n" +
            "      justify-content: center;\n" +
            "      align-items: center;\n" +
            "      min-height: 100vh;\n" +
            "      background-image: url('MakeBarSdat.jpg');\n" +
            "      background-size: cover;\n" +
            "      background-repeat: no-repeat;\n" +
            "    }\n\n" +
            "    .upload-container {\n" +
            "      padding: 20px;\n" +
            "      border-radius: 10px;\n" +
            "      box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);\n" +
            "      font-family: Arial, sans-serif;\n" +
            "      text-align: center;\n" +
            "      background-color: rgba(255, 255, 255, 0.8);\n" +
            "    }\n\n" +
            "    h1 {\n" +
            "      font-size: 24px;\n" +
            "      margin-top: 0;\n" +
            "    }\n\n" +
            "    p {\n" +
            "      font-size: 18px;\n" +
            "    }\n\n" +
            "    .upload-form {\n" +
            "      display: flex;\n" +
            "      flex-direction: column;\n" +
            "      gap: 10px;\n" +
            "      margin-top: 20px;\n" +
            "    }\n\n" +
            "    .upload-button {\n" +
            "      padding: 10px;\n" +
            "      background-color: #007bff;\n" +
            "      color: #fff;\n" +
            "      border: none;\n" +
            "      border-radius: 5px;\n" +
            "      cursor: pointer;\n" +
            "    }\n" +
            "  </style>\n" +
            "</head>\n" +
            "<body>\r\n" +
            "  <div class=\"upload-container\">\r\n" +
            "    <h1>Make BAR/SDAT</h1>\r\n" +
            "    <p>Upload a ZIP file:</p>\r\n" +
            "    <form class=\"upload-form\" enctype=\"multipart/form-data\" action=\"/!HomeTools/Packaging/\" method=\"POST\">\r\n" +
            "      <div class=\"centered-input\">\r\n" +
            "        <input type=\"file\" name=\"files[]\" accept=\".zip\" required>\r\n" +
            "      </div>\r\n\r\n" +
            "      <p>Select mode:</p>\r\n" +
            "      <label>\r\n" +
            "        <input type=\"radio\" name=\"mode\" value=\"sdat\" checked> SDAT\r\n" +
            "      </label>\r\n" +
            "      <label>\r\n" +
            "        <input type=\"radio\" name=\"mode\" value=\"sdatnpd\"> SDAT (custom NPD)\r\n" +
            "      </label>\r\n" +
            "      <label>\r\n" +
            "        <input type=\"radio\" name=\"mode\" value=\"bar\"> BAR\r\n" +
            "      </label>\r\n\r\n" +
            "      <label for=\"TimeStamp\">TimeStamp (8 characters):</label>\r\n" +
            "      <input type=\"text\" id=\"TimeStamp\" name=\"TimeStamp\" pattern=\"[A-Fa-f0-9]{8}\" value=\"FFFFFFFF\" required>\r\n" +
            "      <small>Value must be 8 characters long and only contain hexadecimal characters (0-9, A-F).</small>\r\n\r\n" +
            "      <button class=\"upload-button\" type=\"submit\">Upload</button>\r\n" +
            "    </form>\r\n" +
            "  </div>\r\n" +
            "</body>";

        public static string UnBar = "<!DOCTYPE html>\r\n" +
            "<html>\r\n" +
            "<head>\r\n" +
            "  <title>Home Tools</title>\r\n" +
            "  <style>\r\n" +
            "    body {\r\n" +
            "      margin: 0;\r\n" +
            "      padding: 0;\r\n" +
            "      display: flex;\r\n" +
            "      justify-content: center;\r\n" +
            "      align-items: center;\r\n" +
            "      min-height: 100vh;\r\n" +
            "      background-image: url('UnBar.jpg');\r\n" +
            "      background-size: cover;\r\n" +
            "      background-repeat: no-repeat;\r\n" +
            "    }\r\n\r\n" +
            "    .upload-container {\r\n" +
            "      padding: 20px;\r\n" +
            "      border-radius: 10px;\r\n" +
            "      box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);\r\n" +
            "      font-family: Arial, sans-serif;\r\n" +
            "      text-align: center;\r\n" +
            "      background-color: rgba(255, 255, 255, 0.8);\r\n" +
            "    }\r\n\r\n" +
            "    h1 {\r\n" +
            "      font-size: 24px;\r\n" +
            "      margin-top: 0;\r\n" +
            "    }\r\n\r\n" +
            "    p {\r\n" +
            "      font-size: 18px;\r\n" +
            "    }\r\n\r\n" +
            "    .upload-form {\r\n" +
            "      display: flex;\r\n" +
            "      flex-direction: column;\r\n" +
            "      gap: 10px;\r\n" +
            "      margin-top: 20px;\r\n" +
            "    }\r\n\r\n" +
            "    .upload-button {\r\n" +
            "      padding: 10px;\r\n" +
            "      background-color: #007bff;\r\n" +
            "      color: #fff;\r\n" +
            "      border: none;\r\n" +
            "      border-radius: 5px;\r\n" +
            "      cursor: pointer;\r\n" +
            "    }\r\n" +
            "  </style>\r\n" +
            "</head>\r\n" +
            "<body>\r\n" +
            "  <div class=\"upload-container\">\r\n" +
            "    <h1>UnBar</h1>\r\n" +
            "    <p>Upload a file:</p>\r\n" +
            "    <form class=\"upload-form\" enctype=\"multipart/form-data\" action=\"/!HomeTools/UnBarPackaging/\" method=\"POST\">\r\n" +
            "      <div class=\"centered-input\">\r\n" +
            "        <input type=\"file\" name=\"files[]\" accept=\".zip, .bar, .sharc, .dat, .sdat\" required>\r\n" +
            "      </div>\r\n\t" +
            "      <label for=\"prefix\">Path Prefix (if any required):</label>\r\n" +
            "      <input type=\"text\" id=\"prefix\" name=\"prefix\">\r\n\r\n" +
            "      <label for=\"subfolder\">Subfolder mode:</label>\r\n" +
            "      <input type=\"checkbox\" id=\"subfolder\" name=\"subfolder\">\r\n\r\n" +
            "      <label for=\"bruteforce\">Bruteforce Mode</label>\r\n" +
            "      <input type=\"checkbox\" id=\"bruteforce\" name=\"bruteforce\">\r\n\r\n" +
            "      <button class=\"upload-button\" type=\"submit\">Upload</button>\r\n" +
            "    </form>\r\n" +
            "  </div>\r\n" +
            "</body>\r\n" +
            "</html>";

        public static string ChannelID = @"<!DOCTYPE html>
            <html>
            <head>
              <title>Home Tools</title>
              <style>
                body {
                  margin: 0;
                  padding: 0;
                  display: flex;
                  justify-content: center;
                  align-items: center;
                  min-height: 100vh;
                  background-image: url('ChannelID.jpg');
                  background-size: cover;
                  background-repeat: no-repeat;
                }

                .upload-container {
                  padding: 20px;
                  border-radius: 10px;
                  box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);
                  font-family: Arial, sans-serif;
                  text-align: center;
                  background-color: rgba(255, 255, 255, 0.8);
                }

                h1 {
                  font-size: 24px;
                  margin-top: 0;
                }

                p {
                  font-size: 18px;
                }

                .upload-form {
                  display: flex;
                  flex-direction: column;
                  gap: 10px;
                  margin-top: 20px;
                }

                .upload-button {
                  padding: 10px;
                  background-color: #007bff;
                  color: #fff;
                  border: none;
                  border-radius: 5px;
                  cursor: pointer;
                }
              </style>
            </head>
            <body>
              <div class=""upload-container"">
                <h1>ChannelID - Calculator</h1>
                <form class=""upload-form"" enctype=""multipart/form-data"" action=""/!HomeTools/ChannelIDHandling/"" method=""POST"">
                  <label for=""sceneid"">Enter a Scene ID</label>
                  <input type=""number"" id=""sceneid"" name=""sceneid"" min=""0"" max""65535"" required>
                  <label for=""newerhome"">Newer Format</label>
                  <input type=""checkbox"" id=""newerhome"" name=""newerhome"">
                  <button class=""upload-button"" type=""submit"">Calculate</button>
                  <div class=""result-container"">
                    <p>Calculated ChannelID:</p>
                    <input type=""text"" readonly value=""PUT_GUID_HERE"">
                  </div>
                </form>
              </div>
            </body>
            </html>";

        public static string SceneID = @"<!DOCTYPE html>
            <html>
            <head>
              <title>Home Tools</title>
              <style>
                body {
                  margin: 0;
                  padding: 0;
                  display: flex;
                  justify-content: center;
                  align-items: center;
                  min-height: 100vh;
                  background-image: url('SceneID.jpg');
                  background-size: cover;
                  background-repeat: no-repeat;
                }

                .upload-container {
                  padding: 20px;
                  border-radius: 10px;
                  box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);
                  font-family: Arial, sans-serif;
                  text-align: center;
                  background-color: rgba(255, 255, 255, 0.8);
                }

                h1 {
                  font-size: 24px;
                  margin-top: 0;
                }

                p {
                  font-size: 18px;
                }

                .upload-form {
                  display: flex;
                  flex-direction: column;
                  gap: 10px;
                  margin-top: 20px;
                }

                .upload-button {
                  padding: 10px;
                  background-color: #007bff;
                  color: #fff;
                  border: none;
                  border-radius: 5px;
                  cursor: pointer;
                }
            </style>
            <script>
                function validateInput() {
                  var channelInput = document.getElementById('channelid');
                  var pattern = /^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$/;
                  if (!pattern.test(channelInput.value)) {
                    alert('Invalid Channel ID format. Please enter a valid GUID.');
                    return false;
                  }
                  return true;
                }
              </script>
            </head>
            <body>
              <div class=""upload-container"">
                <h1>SceneID - Calculator</h1>
                <form class=""upload-form"" enctype=""multipart/form-data"" action=""/!HomeTools/SceneIDHandling/"" method=""POST"" onsubmit=""return validateInput();"">
                  <label for=""channelid"">Enter a Channel ID</label>
                  <input type=""text"" id=""channelid"" name=""channelid"" pattern=""^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$"" required>
                  <label for=""newerhome"">Newer Format</label>
                  <input type=""checkbox"" id=""newerhome"" name=""newerhome"">
                  <button class=""upload-button"" type=""submit"">Calculate</button>
                  <div class=""result-container"">
                    <p>Calculated SceneID:</p>
                    <input type=""text"" readonly value=""PUT_SCENEID_HERE"">
                  </div>
                </form>
              </div>
            </body>
            </html>";

        public static string INF = "<!DOCTYPE html>\r\n" +
            "<html>\r\n" +
            "<head>\r\n" +
            "  <title>Home Tools</title>\r\n" +
            "  <style>\r\n" +
            "    body {\r\n" +
            "      margin: 0;\r\n" +
            "      padding: 0;\r\n" +
            "      display: flex;\r\n" +
            "      justify-content: center;\r\n" +
            "      align-items: center;\r\n" +
            "      min-height: 100vh;\r\n" +
            "      background-image: url('INFTool.jpg');\r\n" +
            "      background-size: cover;\r\n" +
            "      background-repeat: no-repeat;\r\n" +
            "    }\r\n\r\n" +
            "    .upload-container {\r\n" +
            "      padding: 20px;\r\n" +
            "      border-radius: 10px;\r\n" +
            "      box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);\r\n" +
            "      font-family: Arial, sans-serif;\r\n" +
            "      text-align: center;\r\n" +
            "      background-color: rgba(255, 255, 255, 0.8);\r\n" +
            "    }\r\n\r\n" +
            "    h1 {\r\n" +
            "      font-size: 24px;\r\n" +
            "      margin-top: 0;\r\n" +
            "    }\r\n\r\n" +
            "    p {\r\n" +
            "      font-size: 18px;\r\n" +
            "    }\r\n\r\n" +
            "    .upload-form {\r\n" +
            "      display: flex;\r\n" +
            "      flex-direction: column;\r\n" +
            "      gap: 10px;\r\n" +
            "      margin-top: 20px;\r\n" +
            "    }\r\n\r\n" +
            "    .upload-button {\r\n" +
            "      padding: 10px;\r\n" +
            "      background-color: #007bff;\r\n" +
            "      color: #fff;\r\n" +
            "      border: none;\r\n" +
            "      border-radius: 5px;\r\n" +
            "      cursor: pointer;\r\n" +
            "    }\r\n" +
            "  </style>\r\n" +
            "</head>\r\n" +
            "<body>\r\n" +
            "  <div class=\"upload-container\">\r\n" +
            "    <h1>INF Tool</h1>\r\n" +
            "    <p>Upload a file:</p>\r\n" +
            "    <form class=\"upload-form\" enctype=\"multipart/form-data\" action=\"/!HomeTools/INFProcess/\" method=\"POST\">\r\n" +
            "      <div class=\"centered-input\">\r\n" +
            "        <input type=\"file\" name=\"files[]\" required>\r\n" +
            "      </div>\r\n\t" +
            "      <button class=\"upload-button\" type=\"submit\">Upload</button>\r\n" +
            "    </form>\r\n" +
            "  </div>\r\n" +
            "</body>\r\n" +
            "</html>";

        public static string CDS = "<!DOCTYPE html>\r\n" +
            "<html>\r\n" +
            "<head>\r\n" +
            "  <title>Home Tools</title>\r\n" +
            "  <style>\r\n" +
            "    body {\r\n" +
            "      margin: 0;\r\n" +
            "      padding: 0;\r\n" +
            "      display: flex;\r\n" +
            "      justify-content: center;\r\n" +
            "      align-items: center;\r\n" +
            "      min-height: 100vh;\r\n" +
            "      background-image: url('INFTool.jpg');\r\n" +
            "      background-size: cover;\r\n" +
            "      background-repeat: no-repeat;\r\n" +
            "    }\r\n\r\n" +
            "    .upload-container {\r\n" +
            "      padding: 20px;\r\n" +
            "      border-radius: 10px;\r\n" +
            "      box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);\r\n" +
            "      font-family: Arial, sans-serif;\r\n" +
            "      text-align: center;\r\n" +
            "      background-color: rgba(255, 255, 255, 0.8);\r\n" +
            "    }\r\n\r\n" +
            "    h1 {\r\n" +
            "      font-size: 24px;\r\n" +
            "      margin-top: 0;\r\n" +
            "    }\r\n\r\n" +
            "    p {\r\n" +
            "      font-size: 18px;\r\n" +
            "    }\r\n\r\n" +
            "    .upload-form {\r\n" +
            "      display: flex;\r\n" +
            "      flex-direction: column;\r\n" +
            "      gap: 10px;\r\n" +
            "      margin-top: 20px;\r\n" +
            "    }\r\n\r\n" +
            "    .upload-button {\r\n" +
            "      padding: 10px;\r\n" +
            "      background-color: #007bff;\r\n" +
            "      color: #fff;\r\n" +
            "      border: none;\r\n" +
            "      border-radius: 5px;\r\n" +
            "      cursor: pointer;\r\n" +
            "    }\r\n" +
            "  </style>\r\n" +
            "</head>\r\n" +
            "<body>\r\n" +
            "  <div class=\"upload-container\">\r\n" +
            "    <h1>Content Delivery System Helper</h1>\r\n" +
            "    <p>Upload a file:</p>\r\n" +
            "    <form class=\"upload-form\" enctype=\"multipart/form-data\" action=\"/!HomeTools/CDSProcess/\" method=\"POST\">\r\n" +
            "      <div class=\"centered-input\">\r\n" +
            "        <input type=\"file\" name=\"files[]\" required>\r\n" +
            "      </div>\r\n\t" +
            "      <label for=\"sha1\">SHA1 of decrypted file:</label>\r\n" +
            "      <input type=\"text\" id=\"sha1\" name=\"sha1\" pattern=\"[0-9a-fA-F]{40}\" required>\r\n\r\n" +
            "      <button class=\"upload-button\" type=\"submit\">Upload</button>\r\n" +
            "    </form>\r\n" +
            "  </div>\r\n" +
            "</body>\r\n" +
            "</html>";

        public static string filenotfound = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">" +
            "<title>Page Not Found</title>\r\n\r\n<style type=\"text/css\">h1, h2, h3, h4\r\n{\r\nfont-family: sans-serif;\r\n}" +
            "\r\nhtml\r\n{\r\nfont-family: Cambria, sans-serif;\r\n}\r\nbody\r\n{\r\nmargin:4em auto;\r\nmax-width: 52em;\r\nmin-width:" +
            " 13em;\r\npadding: 3em;\r\n}\r\nli\r\n{\r\nmargin: 0px;\r\n}\r\nul\r\n{\r\nlist-style: square;\r\n}\r\n</style></head><body>\r\n" +
            "<h1>404 - there's no page.</h1>\r\n\r\n<p>The page you are viewing does not exist.</p><p>" +
            "If you think we brought you here on purpose by posting a wrong link, send us that link via GitHub.</p><pre>PUT_LINK_HERE</pre>\r\n\r\n" +
            "</body></html>";

        public static string phperror = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">" +
            "<title>PHP Error</title>\r\n\r\n<style type=\"text/css\">h1, h2, h3, h4\r\n{\r\nfont-family: sans-serif;\r\n}" +
            "\r\nhtml\r\n{\r\nfont-family: Cambria, sans-serif;\r\n}\r\nbody\r\n{\r\nmargin:4em auto;\r\nmax-width: 52em;\r\nmin-width:" +
            " 13em;\r\npadding: 3em;\r\n}\r\nli\r\n{\r\nmargin: 0px;\r\n}\r\nul\r\n{\r\nlist-style: square;\r\n}\r\n</style></head><body>\r\n" +
            "<h1>500 - an error occured in the PHP sub-system.</h1>\r\n\r\n<p>The PHP page you requested produced a server-side error.</p><pre>PUT_ERROR_HERE</pre><p>" +
            "It is highly recommanded to report that error to the system-administrator.</p><pre>PUT_LINK_HERE</pre>\r\n\r\n" +
            "</body></html>";

        public static string phpnotenabled = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">" +
            "<title>PHP Not Active</title>\r\n\r\n<style type=\"text/css\">h1, h2, h3, h4\r\n{\r\nfont-family: sans-serif;\r\n}" +
            "\r\nhtml\r\n{\r\nfont-family: Cambria, sans-serif;\r\n}\r\nbody\r\n{\r\nmargin:4em auto;\r\nmax-width: 52em;\r\nmin-width:" +
            " 13em;\r\npadding: 3em;\r\n}\r\nli\r\n{\r\nmargin: 0px;\r\n}\r\nul\r\n{\r\nlist-style: square;\r\n}\r\n</style></head><body>\r\n" +
            "<h1>404 - there's no php.</h1>\r\n\r\n<p>This server instance has no PHP enabled.</p><p>" +
            "You can still use the server outside of PHP requests.</p>\r\n\r\n" +
            "</body></html>";

        public static string videoplayer = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n" +
            "<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
            "<title>YouTube Video Player</title>\r\n<style>\r\n  body {\r\n    margin: 0;\r\n    padding: 0;\r\n    display: flex;\r\n" +
            "    flex-direction: column;\r\n    align-items: center;\r\n    justify-content: flex-start;\r\n    min-height: 100vh;\r\n" +
            "    background-color: black;\r\n  }\r\n  #video-container {\r\n    position: relative;\r\n    width: 70%;\r\n" +
            "    padding-bottom: 56.25%; /* 16:9 aspect ratio */\r\n    margin-top: 20px;\r\n  }\r\n  #video-player {\r\n" +
            "    position: absolute;\r\n    top: 0;\r\n    left: 0;\r\n    width: 100%;\r\n    height: 100%;\r\n  }\r\n  #controls {\r\n" +
            "    display: flex;\r\n    align-items: center;\r\n    margin-top: 20px;\r\n  }\r\n  #video-url {\r\n    width: 300px;\r\n" +
            "    padding: 5px;\r\n    margin-right: 10px;\r\n  }\r\n</style>\r\n</head>\r\n<body>\r\n  <div id=\"controls\">\r\n" +
            "    <input type=\"text\" id=\"video-url\" placeholder=\"Enter YouTube URL\">\r\n    <button id=\"play-button\">Play</button>\r\n" +
            "  </div>\r\n  <div id=\"video-container\">\r\n    <iframe id=\"video-player\" frameborder=\"0\" allowfullscreen></iframe>\r\n" +
            "  </div>\r\n  <script>\r\n    const playButton = document.getElementById(\"play-button\");\r\n" +
            "    const videoPlayer = document.getElementById(\"video-player\");\r\n    const videoUrlInput = document.getElementById(\"video-url\");\r\n" +
            "\r\n    playButton.addEventListener(\"click\", () => {\r\n      const youtubeUrl = videoUrlInput.value;\r\n" +
            "      const videoId = extractVideoId(youtubeUrl);\r\n      if (videoId) {\r\n" +
            "        const embedUrl = `https://www.youtube.com/embed/${videoId}`;\r\n        videoPlayer.src = embedUrl;\r\n      }\r\n    });\r\n\r\n" +
            "    function extractVideoId(url) {\r\n      const regex = /[?&]v=([^&#]*)/;\r\n      const match = url.match(regex);\r\n" +
            "      return match ? match[1] : null;\r\n    }\r\n  </script>\r\n</body>\r\n</html>";
    }
}
