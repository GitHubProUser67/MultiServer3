using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using Newtonsoft.Json;
using System.Windows.Controls;

namespace NautilusXP2024
{
    public class FileScannerStandalone
    {
        private readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "Validation.log");
        private TextBox sha1TextBox;

        public FileScannerStandalone(TextBox sha1TextBox)
        {
            this.sha1TextBox = sha1TextBox;
        }

        public async Task ScanDirectoryAsync(string directoryPath)
        {
            await Task.Run(() => ScanFiles(directoryPath));
        }

        public async Task ScanFileAsync(string filePath)
        {
            await Task.Run(() => ValidateFile(filePath));
        }

        private void ScanFiles(string directoryPath)
        {
            var rootFiles = Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly);
            int unmappedFileCount = 0; // Counter for unmapped files
            HashSet<string> unmappedFilePaths = new HashSet<string>();  // Store paths of unmapped files to exclude them from further checks

            foreach (var file in rootFiles)
            {
                string fileName = Path.GetFileName(file);

                if (IsUnmappedFile(Path.GetFileNameWithoutExtension(fileName)))
                {
                    unmappedFileCount++; // Increment counter for unmapped files
                    unmappedFilePaths.Add(file); // Add to set of files to exclude from further validation
                    continue; // Skip further checks for unmapped files
                }
            }

            // Log unmapped files count if any
            string directoryName = Path.GetFileName(Path.TrimEndingDirectorySeparator(directoryPath));
            if (unmappedFileCount > 0)
            {
                LogResult("", $"WARNING: {unmappedFileCount} UNMAPPED Files in {directoryName}");
            }

            var allFiles = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in allFiles)
            {
                if (unmappedFilePaths.Contains(file))
                {
                    continue; // Skip validation for unmapped files
                }
                ValidateFile(file);
            }
        }

        private void ValidateFile(string file)
        {
            // List of filenames that are allowed to be zero bytes
            HashSet<string> allowedZeroByteFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "BAGS.XML", "GAME_FLOW.LUA", "INITIALIZE.LUA", "EXPORTEDINST.LUA", "MAGICCIRCLE.LUA", "BULLET.LUA", "PHASEGAME.LUA"
            };

            // Check for zero-byte files
            FileInfo fileInfo = new FileInfo(file);
            if (fileInfo.Length == 0 && !allowedZeroByteFiles.Contains(fileInfo.Name.ToUpper()))
            {
                LogResult(file, $"WARNING: 0 Byte File");
                return; // Skip further validation for this zero byte file unless it's allowed to be zero bytes
            }

            string extension = Path.GetExtension(file).ToLower();
            // Skip files without an extension
            if (string.IsNullOrEmpty(extension))
            {
                return;
            }

            try
            {
                bool validated = false; // Flag to check if the file was validated

                switch (extension)
                {
                    case ".dds":
                        ValidateDdsImage(file);
                        validated = true;
                        break;
                    case ".bin":
                        // Explicitly skip .bin files as these can contain random data depending on developer
                        break;
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                        ValidateImage(file);
                        validated = true;
                        break;
                    case ".mdl":
                        ValidateFileType(file, ".mdl", new List<byte[]> { new byte[] { 0x48, 0x4D, 0x01 }, new byte[] { 0x4D, 0x52, 0x30 }, new byte[] { 0x48, 0x4D, 0xFF } });
                        validated = true;
                        break;
                    case ".probe":
                        ValidateFileType(file, ".probe", new List<byte[]> { new byte[] { 0x50, 0x52 }, new byte[] { 0x42, 0x4F } });
                        validated = true;
                        break;
                    case ".hkx":
                        ValidateFileType(file, ".hkx", new List<byte[]> { new byte[] { 0x57, 0xE0, 0xE0, 0x57 } });
                        validated = true;
                        break;
                    case ".tga":
                        ValidateFileType(file, ".tga", new List<byte[]> { new byte[] { 0x00, 0x00, 0x02 } });
                        validated = true;
                        break;
                    case ".psd":
                        ValidateFileType(file, ".psd", new List<byte[]> { new byte[] { 0x38, 0x42, 0x50, 0x53 } });
                        validated = true;
                        break;
                    case ".luac":
                        ValidateFileType(file, ".luac", new List<byte[]> { new byte[] { 0x1B, 0x4C, 0x75, 0x61, 0x51 } });
                        validated = true;
                        break;
                    case ".agf":
                        ValidateFileType(file, ".agf", new List<byte[]> { new byte[] { 0x41, 0x47 } });
                        validated = true;
                        break;
                    case ".rig":
                        ValidateFileType(file, ".rig", new List<byte[]> { new byte[] { 0x46, 0x44 } });
                        validated = true;
                        break;
                    case ".raw":
                        ValidateFileType(file, ".raw", new List<byte[]> { new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } });
                        validated = true;
                        break;
                    case ".ani":
                        ValidateFileType(file, ".ani", new List<byte[]> { new byte[] { 0x41, 0x43, 0x31, 0x31 } });
                        validated = true;
                        break;
                    case ".skn":
                        ValidateFileType(file, ".skn", new List<byte[]> { new byte[] { 0x53, 0x4B, 0x30, 0x38 } });
                        validated = true;
                        break;
                    case ".effect":
                        ValidateFileType(file, ".effect", new List<byte[]> { new byte[] { 0x43, 0x48 } });
                        validated = true;
                        break;
                    case ".wav":
                        validated = ValidateWavFile(file);
                        if (!validated)
                        {
                            LogResult(file, "WAV file failed to load correctly.");
                        }
                        break;
                    case ".mp4":
                        validated = ValidateMp4File(file);
                        if (!validated)
                        {
                            LogResult(file, "MP4 file failed to load correctly.");
                        }
                        break;
                    case ".ttf":
                        validated = ValidateTtfFile(file);
                        if (!validated)
                        {
                            LogResult(file, "TTF file failed to load correctly.");
                        }
                        break;
                    case ".lua":
                        var luaResults = VerifyLuaFile(file);
                        validated = luaResults.Item1;
                        if (!validated)
                        {
                            LogResult(file, $"WARNING: Lua Validation Failed: {luaResults.Item2}");
                        }
                        break;
                    case ".cdata":
                        validated = ValidateCdataFile(file);
                        break;
                    case ".json":
                    case ".dat":
                        validated = ValidateJsonFile(file);
                        break;
                    case ".efx":
                        validated = ValidateEfxFile(file);
                        break;
                    case ".mp3":
                        validated = ValidateMp3File(file);
                        if (!validated)
                        {
                            LogResult(file, "MP3 file failed to load correctly.");
                        }
                        break;
                    case ".bank":
                        validated = ValidateBankFile(file);
                        break;
                    case ".bnk":
                        validated = ValidateBnkFile(file);
                        break;
                    case ".xml":
                    case ".scene":
                    case ".atmos":
                    case ".txt":
                    case ".ocean":
                    case ".sdc":
                    case ".oxml":
                    case ".odc":
                    case ".tmx":
                    case ".map":
                    case ".scproj":
                    case ".atgi":
                    case ".enemy":
                    case ".tempo":
                    case ".ini":
                    case ".oel":
                    case ".cam-def":
                    case ".level-setup":
                    case ".node-def":
                    case ".spline-def":
                    case ".ui-setup":
                    case ".gui-setup":
                    case ".repertoire_circuit":
                        validated = ValidateXmlFile(file);
                        break;
                    default:
                        LogResult(file, "No validation method available");
                        break;
                }

                if (validated) // Only log success if validated
                {
                    LogResult(file, "Success"); // Commented so only fails are logged
                }
            }
            catch (Exception ex)
            {
                LogResult(file, $"WARNING: {ex.Message}");
            }
        }

        private bool IsUnmappedFile(string fileName)
        {
            // Normalize the filename to uppercase for case-insensitive comparison
            string normalizedFileName = fileName.ToUpperInvariant();

            // Exclude specific filenames
            if (normalizedFileName == "TIMESTAMP.TXT" || normalizedFileName == "FILES.TXT")
            {
                return false; // Directly return false if the filename is one of the excluded names
            }

            // Check if the filename consists of 8 hexadecimal characters
            return fileName.Length == 8 && System.Text.RegularExpressions.Regex.IsMatch(fileName, "^[0-9A-F]{8}$");
        }

        private bool ValidateCdataFile(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (fs.Length == 0)
                    {
                        throw new Exception("File is empty.");
                    }
                    fs.Seek(-1, SeekOrigin.End); // Move the reader to the last byte of the file
                    int lastByte = fs.ReadByte();
                    if (lastByte != 0x01)
                    {
                        throw new Exception("Invalid ending byte. Expected 0x01.");
                    }
                }
                return true; // Return true if the last byte is 0x01
            }
            catch (Exception ex)
            {
                LogResult(filePath, $"CDATA Validation Failed: {ex.Message}");
                return false; // Return false if there was an exception
            }
        }

        public bool ValidateMp3File(string filePath)
        {
            try
            {
                // Try to open the file with Mp3FileReader
                using (var mp3Reader = new Mp3FileReader(filePath))
                {
                    // Optionally log successful load or check properties
                    Debug.WriteLine($"MP3 Validation: Successfully opened {filePath}");
                }
                return true; // File opened successfully
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MP3 Validation Failed for {filePath}: {ex.Message}");
                return false; // Return false if the file could not be opened
            }
        }

        private bool ValidateBnkFile(string filePath)
        {
            byte[] sequenceOne = new byte[] { 0x6B, 0x6C, 0x42, 0x53 }; // "klBS"
            byte[] sequenceTwo = new byte[] { 0x53, 0x42, 0x6C, 0x6B }; // "SBlk"

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] fileContents = new byte[fs.Length];
                    fs.Read(fileContents, 0, fileContents.Length);

                    // Search for the byte sequences in the file
                    if (ContainsSequence(fileContents, sequenceOne) || ContainsSequence(fileContents, sequenceTwo))
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception("Required byte sequences not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogResult(filePath, $"BNK Validation Failed: {ex.Message}");
                return false;
            }
        }

        private bool ValidateBankFile(string filePath)
        {
            byte[] sequenceOne = new byte[] { 0x6B, 0x6C, 0x42, 0x53 }; // "klBS"
            byte[] sequenceTwo = new byte[] { 0x53, 0x42, 0x6C, 0x6B }; // "SBlk"

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] fileContents = new byte[fs.Length];
                    fs.Read(fileContents, 0, fileContents.Length);

                    // Search for the byte sequences in the file
                    if (ContainsSequence(fileContents, sequenceOne) || ContainsSequence(fileContents, sequenceTwo))
                    {
                        return true; // BNK validation successful
                    }

                    // If BNK validation fails, try validating as XML
                    fs.Seek(0, SeekOrigin.Begin); // Reset stream position to the beginning for XML reading
                    var invalidXmlChars = new HashSet<char>(
                        Enumerable.Range(0, 32) // From 0x00 to 0x1F
                        .Concat(new[] { 127 }) // Adding 0x7F (DEL)
                        .Select(i => (char)i)
                        .Except(new[] { '\t', '\n', '\r' }) // Exclude tab, LF, CR which are valid in XML
                    );

                    using (var reader = new StreamReader(fs))
                    {
                        while (!reader.EndOfStream)
                        {
                            var buffer = new char[1024];
                            int read = reader.Read(buffer, 0, buffer.Length);
                            if (buffer.Take(read).Any(c => invalidXmlChars.Contains(c)))
                            {
                                throw new Exception("Invalid XML characters found.");
                            }
                        }
                    }

                    // If no exceptions were thrown during XML validation, the file is considered a valid XML
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogResult(filePath, $"Validation Failed: {ex.Message}");
                return false; // Return false indicating the file failed both BNK and XML validations
            }
        }

        private void ValidateImage(string filePath)
        {
            using (System.Drawing.Image img = System.Drawing.Image.FromFile(filePath))
            {
                Debug.WriteLine($"Validated image {filePath}");
            }
        }

        private void ValidateDdsImage(string filePath)
        {
            if (IsDdsFile(filePath))
            {
                Debug.WriteLine($"Validated DDS image {filePath}");
            }
            else
            {
                throw new Exception(".dds failed header check - Possibly Corrupt.");
            }
        }

        private bool IsDdsFile(string filePath)
        {
            return CheckFileSignature(filePath, new List<byte[]> { new byte[] { 0x44, 0x44, 0x53, 0x20 } }); // "DDS "
        }

        private void ValidateFileType(string filePath, string fileType, List<byte[]> signatures)
        {
            if (CheckFileSignature(filePath, signatures))
            {
                Debug.WriteLine($"Validated {fileType} file {filePath}");
            }
            else
            {
                throw new Exception($"{fileType} Failed header check - Possibly Corrupt.");
            }
        }

        private bool ValidateXmlFile(string filePath)
        {
            try
            {
                // Define a HashSet to contain all invalid XML characters, excluding NULL (0x00)
                var invalidXmlChars = new HashSet<char>(
                    Enumerable.Range(1, 31)  // From 0x01 to 0x1F
                    .Concat(new[] { 127 })   // Adding 0x7F (DEL)
                    .Select(i => (char)i)
                    .Except(new[] { '\t', '\n', '\r' })  // Exclude tab, LF, CR which are valid in XML
                );

                using (var reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        var buffer = new char[1024];
                        int read = reader.Read(buffer, 0, buffer.Length);
                        if (buffer.Take(read).Any(c => invalidXmlChars.Contains(c)))
                        {
                            throw new Exception("Invalid XML characters found.");
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogResult(filePath, $"XML Validation Failed: {ex.Message}");
                return false;
            }
        }

        private bool ValidateEfxFile(string filePath)
        {
            byte[] ddsUpper = new byte[] { 0x44, 0x44, 0x53 }; // "DDS"
            byte[] ddsLower = new byte[] { 0x64, 0x64, 0x73 }; // "dds"

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] fileContents = new byte[fs.Length];
                    fs.Read(fileContents, 0, fileContents.Length);

                    // Search for "DDS" or "dds" in the file
                    if (ContainsSequence(fileContents, ddsUpper) || ContainsSequence(fileContents, ddsLower))
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception("No 'DDS' or 'dds' sequence found.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogResult(filePath, $"WARNING: EFX Validation Failed: {ex.Message}");
                return false;
            }
        }

        public bool ValidateJsonFile(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);  // Read the JSON file content
                JsonConvert.DeserializeObject(jsonContent);       // Attempt to parse the JSON content

                // If parsing succeeds, log success (if needed) and return true

                return true;
            }
            catch (JsonReaderException jex)
            {
                // Specific error handling for JSON parsing issues
                LogResult(filePath, $"JSON Validation Failed: {jex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                // General error handling for other exceptions such as file access issues
                LogResult(filePath, $"JSON Validation Exception: {ex.Message}");
                return false;
            }
        }

        private bool ContainsSequence(byte[] array, byte[] sequence)
        {
            for (int i = 0; i <= array.Length - sequence.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < sequence.Length; j++)
                {
                    if (array[i + j] != sequence[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    return true;
            }
            return false;
        }

        private bool CheckFileSignature(string filePath, List<byte[]> validSignatures)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] fileSignature = new byte[validSignatures[0].Length];
                    if (fs.Read(fileSignature, 0, fileSignature.Length) == fileSignature.Length)
                    {
                        foreach (var signature in validSignatures)
                        {
                            if (fileSignature.SequenceEqual(signature))
                                return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading file {filePath}: {ex.Message}");
            }
            return false;
        }

        private (bool, string) VerifyLuaFile(string filePath)
        {
            try
            {
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dependencies", "homeluac.exe");
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo(exePath, $"-p \"{filePath}\"")
                    {
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    process.Start();
                    process.WaitForExit();

                    var errorOutput = process.StandardError.ReadToEnd().Trim();
                    bool isValid = string.IsNullOrEmpty(errorOutput);

                    // Directly log within this method only if needed based on specific use case
                    if (!isValid)
                    {
                        // Formatting the error to be more concise
                        string formattedError = FormatLuaError(errorOutput);
                        LogResult(filePath, $"WARNING: Lua Validation Failed: {formattedError}");
                    }

                    return (isValid, errorOutput);
                }
            }
            catch (Exception ex)
            {
                LogResult(filePath, $"WARNING: Lua Validation Exception: {ex.Message}");
                return (false, ex.Message);
            }
        }

        // Helper function to format the Lua error output
        private string FormatLuaError(string errorOutput)
        {
            int thirdColonIndex = NthIndexOf(errorOutput, ':', 3);
            if (thirdColonIndex != -1)
            {
                // Extract the part after the third colon, and prepend "Line "
                string errorDetails = errorOutput.Substring(thirdColonIndex + 1).Trim();
                // Assume the message details the error right after the file path and line info
                int errorStartIndex = errorDetails.IndexOfAny(new char[] { ':', ' ' });
                if (errorStartIndex != -1)
                {
                    string lineInfo = errorDetails.Substring(0, errorStartIndex).Trim();
                    string errorMessage = errorDetails.Substring(errorStartIndex + 1).Trim();
                    return $"{lineInfo}: {errorMessage}";
                }
            }
            return errorOutput; // Return the original error output if no third colon is found or formatting fails
        }

        // Method to find the nth index of a character in a string
        private int NthIndexOf(string text, char c, int n)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == c)
                {
                    count++;
                    if (count == n)
                        return i;
                }
            }
            return -1; // Not found
        }

        public bool ValidateMp4File(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    // MP4 files should start with a file type box ('ftyp') and a movie box ('moov')
                    byte[] buffer = new byte[8];  // To read the size and type of the first box
                    if (fs.Read(buffer, 0, 8) != 8)
                        throw new Exception("Unable to read the file header.");

                    int size = BitConverter.ToInt32(buffer, 0);  // Assuming big-endian
                    string type = Encoding.ASCII.GetString(buffer, 4, 4);

                    if (type != "ftyp")
                        throw new Exception("First box is not 'ftyp', file might be corrupted.");

                    // You might want to check additional structure here, like verifying 'moov' box existence

                    return true;  // Valid MP4 file
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MP4 Validation Failed for {filePath}: {ex.Message}");
                LogResult(filePath, $"MP4 Validation Failed: {ex.Message}");
                return false;  // Return false if the file could not be opened or is not in expected format
            }
        }

        public bool ValidateTtfFile(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] expectedHeader1 = new byte[] { 0x00, 0x01, 0x00, 0x00 }; // Common TTF header
                    byte[] expectedHeader2 = new byte[] { 0x74, 0x72, 0x75, 0x65 }; // "true" in ASCII
                    byte[] fileHeader = new byte[4];

                    if (fs.Read(fileHeader, 0, 4) != 4)
                        throw new Exception("Failed to read file header.");

                    // Check if the file header matches any of the expected headers
                    if (!fileHeader.SequenceEqual(expectedHeader1) && !fileHeader.SequenceEqual(expectedHeader2))
                    {
                        throw new Exception("Invalid TTF file header.");
                    }

                    return true; // The file starts with a valid TTF version number, simple check passed
                }
            }
            catch (Exception ex)
            {
                LogResult(filePath, $"TTF Validation Failed: {ex.Message}");
                return false;  // Return false if the file could not be opened or is not in expected format
            }
        }

        public bool ValidateWavFile(string filePath)
        {
            try
            {
                // Use NAudio's WaveFileReader to try opening the WAV file.
                using (var wavReader = new WaveFileReader(filePath))
                {
                    // If the file opens without throwing an exception, it's considered valid.
                }
                return true;  // File opened successfully, it's a valid WAV file
            }
            catch (Exception ex)
            {
                // Log the error message if the WAV file fails to load
                LogResult(filePath, $"WAV Validation Failed: {ex.Message}");
                return false;  // Return false if the file could not be opened or is in an incorrect format
            }
        }

        private void LogResult(string filePath, string result)
        {
            // Find the index of the slash after "Mapped" and trim the path
            int index = filePath.IndexOf("/");  // This finds the index of the first slash in the path
            string trimmedPath = filePath;

            if (index >= 0) // Check if the slash was found
            {
                trimmedPath = filePath.Substring(index + 1);  // Trim the path to start just after the found slash
            }

            string logText = $"{DateTime.Now}: {result} {trimmedPath}\n";
            try
            {
                File.AppendAllText(logFilePath, logText);

                // Update the TextBox on the UI thread
                sha1TextBox.Dispatcher.Invoke(() =>
                {
                    sha1TextBox.AppendText(logText);
                    sha1TextBox.ScrollToEnd();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to log result for {trimmedPath}: {ex.Message}");
            }
        }
    }
}
