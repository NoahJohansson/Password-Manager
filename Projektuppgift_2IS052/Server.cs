using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Projektuppgift_2IS052
{
    class Server
    {
        public byte[] IV { get; set; }

        public byte[] Encrypted { get; set; }

        public string PathToServer { get; set; }

        public Server()
        {

        }

        public void Init(string server, byte[] key)
        {
            //Generera IV och spara i server.
            Aes myAes = Aes.Create();
            this.IV = myAes.IV;
            this.PathToServer = server;
            if (server.Contains("/") || server.Contains("\\")){
                Directory.CreateDirectory(Path.GetDirectoryName(server));
            }
            Dictionary<string, string> emptyVault = new Dictionary<string, string>();
            this.EncryptVault(key, emptyVault);
            this.SaveTofile();
            
        }
        public void LoadServer(string server)
        {
            string jsonString = File.ReadAllText(server);
            Server server1 = JsonSerializer.Deserialize<Server>(jsonString);
            this.PathToServer = server1.PathToServer;
            this.Encrypted = server1.Encrypted;
            this.IV = server1.IV;
        }
        public void SaveTofile()
        {
            string jsonStringServer = JsonSerializer.Serialize(this);
            var serverFile = File.Create(this.PathToServer);
            serverFile.Close();
            File.WriteAllText(this.PathToServer, jsonStringServer);

        }
        public void AddToVault(string prop, string plainText, byte[] key) {
            Dictionary<string, string> decryptedVault = this.GetDecryptedVault(key);
            if (decryptedVault.ContainsKey(prop)) {
                decryptedVault.Remove(prop);
            }

            decryptedVault.Add(prop, plainText);
            this.EncryptVault(key, decryptedVault);
            this.SaveTofile();
        }

        public void RemoveFromVault(string prop, byte[] key)
        {
            Dictionary<string, string> decryptedVault = this.GetDecryptedVault(key);
            decryptedVault.Remove(prop);
            this.EncryptVault(key, decryptedVault);
            this.SaveTofile();

        }

        public void EncryptVault(byte[] key, Dictionary<string, string> decryptedVault)
        {
            byte[] encrypted;
            string jsonStringVault = JsonSerializer.Serialize(decryptedVault);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = this.IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(jsonStringVault);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            this.Encrypted = encrypted;
            this.SaveTofile();
        }
        public Dictionary<string, string> GetDecryptedVault(byte[] Key)
        {
            byte[] encrypted = this.Encrypted;
            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;
            Dictionary<string, string> Vault1 = null;
            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = this.IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(encrypted))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                            Vault1 = JsonSerializer.Deserialize<Dictionary<string, string>>(plaintext);
                        }
                    }
                }
            }

            return Vault1;

        }


    }
}
