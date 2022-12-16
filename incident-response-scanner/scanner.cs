using System.Text;
using System.Security.Cryptography;
using System.Net.NetworkInformation;
using System.Net;

namespace MalwareScanner
{
     class scanner {

        public scanner(TextBox displayRegistries, TextBox displayProcesses, TextBox displayFiles, TextBox displayIPAddress)
        {
            this.displayRegistries = displayRegistries;
            this.displayProcesses = displayProcesses;
            this.displayFiles = displayFiles;
            this.displayIPAddress = displayIPAddress;
        }

        TextBox displayRegistries;
        TextBox displayProcesses;
        TextBox displayFiles;
        TextBox displayIPAddress;
        int foundMalware = 0;

        /* Edit the variables below to match your malware symptoms */

        /* proceses names to search for */
        string[] proceses = { "" };

        /* Searches ALL users AppData folders for a specific file and verfies the hash if wanted
        * e.g: If the malicous file was located in C:\Users\DrewQ\AppData\Roaming\backdoor.ps1 
        * and its file has was c7a5fa3e56640ce48dcc3e8d972e444d9cdd2306
        * 
        * you would configure the dictionary below as   
        */
        Dictionary<string, object?> appDataFiles = new Dictionary<string, object?>() {
            {"\\Roaming\\backdoor.ps1", "c7a5fa3e56640ce48dcc3e8d972e444d9cdd2306"}
        };
        /* Note: If you are not concerned about the files hash you can pass null.
         * Dictionary<string, object?> appDataFiles = new Dictionary<string, object?>() {
            {"\\Roaming\\backdoor.ps1", null}
          };
         */

        /* List of IP's to flag if they show up in the active TCP connections */
        string[] IPAddresses = { "" };

        /* Non user specific files to look for */
        Dictionary<string, object?> files = new Dictionary<string, object?>() {
            {"C:\\Windows\\Boot\\Mal.exe", "b32dab7b26cdf6b9548baea6f3cfe5b8f326ceda"}
        };

        string[] GetUsersFolders() {
            /* Get's All Users folders on a system */
            return Directory.GetDirectories("C:\\Users\\", "*", SearchOption.TopDirectoryOnly);
        }

        object? GetSha1FileHash(string filename)
        /* Gets a SHA1 hash of a file
            * :param filename: Filename to get the hash of
            * :return: A filehash as a string
            */
        {
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (var sha1 = SHA1.Create())
                    {
                        byte[] hash = sha1.ComputeHash(bs);
                        StringBuilder formatted = new StringBuilder(2 * hash.Length);
                        foreach (byte b in hash)
                        {
                            formatted.AppendFormat("{0:X2}", b);
                        }
                        return formatted.ToString();

                    }
                }
            }
            // Any File/Directory Errors will be treated as non malicous 
            catch (System.IO.IOException)
            {
                return null;
            }
        }

        void WriteSymptomToTextbox(string text, TextBox displayBox) {
            /* Writes text to a textbox an increases the malware counter
            * :param text: Text to display
            * :param symptomText
            */
            foundMalware += 1; // :(
            displayBox.Text += text + "\r\n";
        }

        void WriteIfFileIsMalicous(string path, object? hash, TextBox displayBox) {
            /*  Writes to a textbox if the file is malicous
             *  :param path: Path to write to
             *  :param hash: Hash to compare
             *  :param displayBox: Text Box to write to
             */
            if (IsFileMalicous(path, hash))
            {
                WriteSymptomToTextbox(path, displayBox);
            }
        }

        bool IsFileMalicous(string path, object? hash) {
            /* Checks to see if a file if malicous 
                :param path: File path to check
                :param hash: If set, The file will be evaulated otherwise, existsence is checked

            IsFileMalious("C:\Users\DrewQ\AppData\Roaming\backdoor.ps1", null) <-- Checks if file exists
            IsFileMalious("C:\Users\DrewQ\AppData\Roaming\backdoor.ps1",  "b3..") <-- Only Returns true if sha1 hash matches
            */
            if (hash == null) return File.Exists(path);
            object? fetchedHash = GetSha1FileHash(path);
            if (fetchedHash == null) return false;
            return hash == fetchedHash;

        }

        void ProcessSystemFiles() {
        /* Checks for malicous system files
        * Edit the variable 'files' to your symptoms
        */
        foreach (KeyValuePair<string, object?> kvp in files)
        {
            string path = kvp.Key;
            object? hash = kvp.Value;
            WriteIfFileIsMalicous(path, hash, displayFiles);
        }
        }

        List<string> GetActiveTCPConnections()
        /* Gets all IPV4 active TCP connections
         * :return: A list of IPV4 addresses
         */
        {
            List<string> results = new List<string>();
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            foreach (TcpConnectionInformation connection in connections)
            {
                // Doesn't support IPV6
                results.Add(connection.RemoteEndPoint.ToString().Split(":")[0]);
            }
            return results;
        } 

        void ProcessAppDataFiles()
        /* Checks ALL Users AppData folders for malware 
         * Edit the variable 'appDataFiles' to your symptoms
         */
        {
            foreach (KeyValuePair<string, object?> kvp in appDataFiles)
            {
                foreach(string userFolder in GetUsersFolders())
                {
                    string path = kvp.Key;
                    object? hash = kvp.Value;
                    WriteIfFileIsMalicous(userFolder + path, hash, displayFiles);
                }
            }
        }

        void ProcessTCPConnections() {
            /* Checks IPV4 TCP connections for malicous IP Addresses
             * Edit the variable 'IPAddresses' to your symptoms
             */
            foreach (string ip in GetActiveTCPConnections())
            {
                if (IPAddresses.Contains(ip)) {
                    WriteSymptomToTextbox(ip, displayIPAddress);
                }
            }
        }


        public void scan() {
            ProcessTCPConnections();
            ProcessSystemFiles();
            ProcessAppDataFiles();
        }
    }
}

