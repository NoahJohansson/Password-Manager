using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Projektuppgift_2IS052
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            string command = args[0];
            if (command == "init")
            {
                try
                {
                    Init(args[1], args[2]);
                }

                catch (Exception)
                {
                    Console.WriteLine("Något gick fel, skriv endast in namn på klient och server");
                }

            }

            else if (command == "login")
            {
                try
                {
                    Login(args[1], args[2]);
                }

                catch (Exception)
                {
                    Console.WriteLine("Något gick fel, skrev du in fel lösenord eller fel secret key?");
                }
            }

            else if (command == "get")
            {
                if (args.Length == 3)
                {
                    try
                    {
                        Get(args[1], args[2], null);
                    }

                    catch (Exception)
                    {

                        Console.WriteLine("Något gick fel, skrev du in fel lösenord?");
                    }
                }

                else if (args.Length == 4)
                {
                    try
                    {
                        Get(args[1], args[2], args[3]);
                    }

                    catch (Exception)
                    {
                        Console.WriteLine("Något gick fel, skrev du in fel lösenord?");
                    }
                }
            }

            else if (command == "set")
            {
                if (args.Length == 3)
                {
                    Console.WriteLine("Du måste ange en prop");
                }

                else if (args.Length == 4)
                {
                    try
                    {
                        Set(args[1], args[2], args[3], null);
                    }

                    catch (Exception)
                    {
                        Console.WriteLine("Något gick fel, skrev du in fel lösenord?");
                    }
                }

                else if (args.Length == 5)
                {
                    try
                    {
                        Set(args[1], args[2], args[3], args[4]);
                    }

                    catch (Exception)
                    {
                        Console.WriteLine("Något gick fel, skrev du in fel lösenord?");
                    }
                }
            }

            else if (command == "drop")
            {
                try
                {
                    Drop(args[1], args[2], args[3]);
                }
                catch (Exception)
                {
                    Console.WriteLine("Något gick fel, skrev du in ett värde som inte finns?");
                }
            }
            else if (command == "secret")
            {
                try
                {
                    Secret(args[1]);
                }

                catch (Exception)
                {
                    Console.WriteLine("Den klienten finns inte");
                }
            }
            else
            {
                Console.WriteLine("Det kommandot finns inte. Se manual för mer info");
            }
        }
        static void StartUp()
        {
            Console.WriteLine("------Welcome to passwordman------");
            Console.WriteLine("-----See docs for instructions-----");
        }
        static void Init(string client, string server)
        {
            Client newClient = new Client();
            newClient.Init(client);

            Server newServer = new Server();
            newServer.Init(server, newClient.GetIMKey());
        }
        static void Set(string client, string server, string prop, string flag)
        {
            Client newClient = new Client();
            newClient.LoadClient(client);

            Server newServer = new Server();
            newServer.LoadServer(server);

            if (flag == "-g" || flag == "--generate")
            {
                string characters = "abcdefghijklmnopqrstuvwxyzåäöABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ0123456789";
                StringBuilder random = new StringBuilder();
                Random randomCharacter = new Random();
                for (int i = 0; i<20; i++)
                {
                    random.Append(characters[randomCharacter.Next(68)]);
                }
                newServer.AddToVault(prop, random.ToString(), newClient.GetIMKey());
            }

            else
            {
                byte[] key = newClient.GetIMKey();
                Console.WriteLine("Mata in det du vill kryptera: ");
                string plaintext = Console.ReadLine();
                newServer.AddToVault(prop, plaintext, key);
            }

        }
        static void Get(string client, string server, string prop)
        {
            Client newClient = new Client();
            newClient.LoadClient(client);
            Server newServer = new Server();
            newServer.LoadServer(server);
            
            if (prop == null)
            {
                foreach (KeyValuePair<string, string> kvp in newServer.GetDecryptedVault(newClient.GetIMKey()))
                {
                    Console.WriteLine("{0}", kvp.Key);
                }
            }
            else
            {
                foreach (KeyValuePair<string, string> kvp in newServer.GetDecryptedVault(newClient.GetIMKey()))
                {
                    if(kvp.Key.ToLower() == prop.ToLower()) {
                        Console.WriteLine("{0}", kvp.Value);
                    }
                }
            }

        }

        static void Drop(string client, string server, string prop)
        {
            Client newClient = new Client();
            newClient.LoadClient(client);
            Server newServer = new Server();
            newServer.LoadServer(server);

            newServer.RemoveFromVault(prop, newClient.GetIMKey());

            newServer.SaveTofile();

            newServer.GetDecryptedVault(newClient.GetIMKey()).Remove(prop);
            newServer.SaveTofile();

        }
        static void Login(string client, string server)
        {
            Client newClient = new Client();
            newClient.Init(client);
            

            Server oldServer = new Server();
            oldServer.LoadServer(server);
            try
            {
                Console.WriteLine("Mata in huvudlösenordet: ");
                string pwd = Console.ReadLine();
                //Kombinera secretkey + pwd för att få key
                byte[] pwdByte = Encoding.ASCII.GetBytes(pwd);
                Console.WriteLine("Mata in din gamla secretkey till servern");
                string oldSecretJsonKey = Console.ReadLine();
                byte[] SecretKey = JsonSerializer.Deserialize<byte[]>(oldSecretJsonKey);
                Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(pwdByte, SecretKey, 1000);
                byte[] key = rfc.GetBytes(32);
                

                try
                {
                    oldServer.GetDecryptedVault(key);
                    newClient.SecretKey = SecretKey;
                    newClient.SaveClient();

                }
                catch (Exception)
                {
                    string path = client;
                    bool tem = File.Exists(path);
                    if (File.Exists(path)) { File.Delete(path); }

                    Console.WriteLine("Du skrev in fel lösenord");

                }
            }
            catch(Exception)
            {

                string path = client;
                bool tem = File.Exists(path);
                if (File.Exists(path)) { File.Delete(path); }
                Console.WriteLine("Du skrev in fel secret key / lösenord");
            }

        }
        

        static void Secret(string client)
        {
            Client newClient = new Client();
            newClient.LoadClient(client);
            string jsonStringSecretKey = JsonSerializer.Serialize(newClient.SecretKey);
            Console.WriteLine(jsonStringSecretKey);

        }


        
    }


}
