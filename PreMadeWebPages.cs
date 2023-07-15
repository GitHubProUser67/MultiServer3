namespace PSMultiServer
{
    public class PreMadeWebPages
    {
        public static string rootrefused = @"
            <!DOCTYPE html>
            <html>
            <head>
              <title>No Access</title>
              <style>
                body {
                  margin: 0;
                  padding: 0;
                }

                .error-container {
                  position: absolute;
                  top: 20px;
                  left: 20px;
                  background-color: #000;
                  color: #fff;
                  padding: 20px;
                  border-radius: 10px;
                  box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);
                  font-family: Arial, sans-serif;
                }

                h1 {
                  font-size: 24px;
                  margin-top: 0;
                }

                p {
                  font-size: 18px;
                }
              </style>
            </head>
            <body>
              <div class=""error-container"">
                <h1>No Access</h1>
                <p>We're sorry, but this part of the server is not accessible.</p>
              </div>
            </body>
            </html>";

        public static string filenotfound = @"
            <!DOCTYPE html>
            <html>
            <head>
              <title>File Not Found</title>
              <style>
                body {
                  margin: 0;
                  padding: 0;
                }

                .error-container {
                  position: absolute;
                  top: 20px;
                  left: 20px;
                  background-color: #000;
                  color: #fff;
                  padding: 20px;
                  border-radius: 10px;
                  box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);
                  font-family: Arial, sans-serif;
                }

                h1 {
                  font-size: 24px;
                  margin-top: 0;
                }

                p {
                  font-size: 18px;
                }
              </style>
            </head>
            <body>
              <div class=""error-container"">
                <h1>File Not Found</h1>
                <p>We're sorry, but the file you requested could not be found.</p>
              </div>
            </body>
            </html>";

        public static string phpnotenabled = @"
            <!DOCTYPE html>
            <html>
            <head>
              <title>PHP Disabled</title>
              <style>
                body {
                  margin: 0;
                  padding: 0;
                }

                .error-container {
                  position: absolute;
                  top: 20px;
                  left: 20px;
                  background-color: #000;
                  color: #fff;
                  padding: 20px;
                  border-radius: 10px;
                  box-shadow: 0 0 10px rgba(0, 0, 0, 0.3);
                  font-family: Arial, sans-serif;
                }

                h1 {
                  font-size: 24px;
                  margin-top: 0;
                }

                p {
                  font-size: 18px;
                }
              </style>
            </head>
            <body>
              <div class=""error-container"">
                <h1>PHP Disabled</h1>
                <p>We're sorry, but the PHP system is disabled right now.</p>
              </div>
            </body>
            </html>";
    }
}
