using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Projektuppgift_2IS052
{
    class Client
    {
        public byte[] SecretKey { get; set; }
        public string PathToClient { get; set; }

        public Client()
        {
        }

        public void Init(string client)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] secretkey = new byte[32];
            rng.GetBytes(secretkey);
            if (client.Contains("/") || client.Contains("\\"))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(client));
            }

            var clientFile = File.Create(client);
            clientFile.Close();

            this.SecretKey = secretkey;
            this.PathToClient = client;

            this.SaveClient();
        }

        public void LoadClient(string client)
        {
            string jsonString = File.ReadAllText(client);
            Client client1 = JsonSerializer.Deserialize<Client>(jsonString);
            this.PathToClient = client1.PathToClient;
            this.SecretKey = client1.SecretKey;
        }

        public byte[] GetIMKey()
        {
            Console.WriteLine("Mata in ditt huvudlösen");
            string pwd = Console.ReadLine();
            //Kombinera secretkey + pwd för att få key
            byte[] pwdByte = Encoding.ASCII.GetBytes(pwd);
            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(pwdByte, this.SecretKey, 1000);
            byte[] key = rfc.GetBytes(32);

            return key;
        }

        public void SaveClient() {
            string jsonStringSecretKey = JsonSerializer.Serialize(this);
            File.WriteAllText(this.PathToClient, jsonStringSecretKey);
        }

    }
}
