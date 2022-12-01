﻿using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace Redis.ConsoleApp
{
    class Program
    {
        private  static Guid ToolId = new Guid("2c447cfd-d4be-4c8d-910b-901df77bf921");
        static void Main(string[] args)
        {
            IDatabase database = GetDatabase();

            var testResult = TestDatabaseConnection(database);
            
            if (!testResult)
            {
                return;
            }

            LoadTools(database);
            
            char ch;
            string userName, lastName;
            do
            {
                Console.WriteLine("Enter fist name");
                userName = Console.ReadLine();
                Console.WriteLine("Enter last name");
                lastName = Console.ReadLine();
                try
                {
                    if (userName.Length != 0 && lastName.Length != 0)
                    {
                        var newHuman = new RentingGuy()
                        {
                            Id = Guid.NewGuid(),
                            Name = userName,
                            LastName = lastName,
                        };

                        if (!AddNewHuman(database, newHuman))
                        {
                            Console.WriteLine($"Error creating user {userName}");
                        }
                    }
                    
                    Console.WriteLine("Enter + to stop or press enter to continue.");
                    var input = Console.Read();
                    ch = Convert.ToChar(input);
                }
                catch (OverflowException e)
                {
                    Console.WriteLine("{0} Value read.", e.Message);
                    ch = Char.MinValue;
                    Console.WriteLine("\nType name then press Enter type lastname then press Enter. Type '+' anywhere in the text to quit:\n");
                }
            } while (ch != '+');
            
            char stop;
            string userGuid, toolName, action;
            do
            {
                Console.WriteLine("Enter user guid to get or return tool from");
                userGuid = Console.ReadLine();
                Console.WriteLine("Enter tool name");
                toolName = Console.ReadLine();
                Console.WriteLine("Enter action type: get/return");
                action = Console.ReadLine();
                try
                {
                    if (userGuid.Length != 0 && toolName.Length != 0 && action.Length != 0)
                    {
                        
                    }
                    
                    Console.WriteLine("Enter + to stop or press enter to continue.");
                    var input = Console.Read();
                    stop = Convert.ToChar(input);
                }
                catch (OverflowException e)
                {
                    Console.WriteLine("{0} Value read.", e.Message);
                    stop = Char.MinValue;
                    Console.WriteLine("\nType name then press Enter type lastname then press Enter. Type '+' anywhere in the text to quit:\n");
                }
            } while (stop != '+');

            //Read recently setted Value From Cache
            var cachedresponse = database.StringGet("Tool1");
            Console.WriteLine(cachedresponse.ToString());
        }
        
        private class RentingGuy
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string LastName { get; set; }
            public List<string> ReservedTools { get; set; }
        }
        
        private class Tools
        {
            public Guid Id { get; set; }

            public List<string> ToolList { get; set; } = new() { "Lamp", "BigLamp", "Drill", "Hammer drill", "Sander" };
        }

        private static bool AddNewHuman(IDatabase database, RentingGuy human)
        {
            var jsonString = JsonConvert.SerializeObject(human);
            return database.StringSet($"human-{human.Id}", jsonString);
        }
        
        private static Lazy<ConnectionMultiplexer> lazyConnection = CreateConnection();

        private static ConnectionMultiplexer Connection
        {
            get { return lazyConnection.Value; }
        }

        private static void LoadTools(IDatabase database)
        {
            var toolName = database.StringGet($"tool-{ToolId}");

            if (toolName.ToString().Length != 0)
            {
                return;
            }

            var tools = new Tools()
            {
                Id = ToolId
            };
            
            var jsonString = JsonConvert.SerializeObject(tools);
            database.StringSet($"tool-{ToolId}", jsonString);
        }

        private static bool TestDatabaseConnection(IDatabase database)
        {
            var commandresponse = database.Execute("PING");
            return commandresponse.ToString() == "PONG";
        }

        private static Lazy<ConnectionMultiplexer> CreateConnection()
        {
            return new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect("localhost:6379");
            });
        }
        
        private static RetryPolicy retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetry(5, p =>
            {
                var timeToWait = TimeSpan.FromSeconds(90);
                Console.WriteLine($"Waiting for reconnection {timeToWait}");
                return timeToWait;
            });
        
        private static IDatabase GetDatabase()
        {
            return retryPolicy.Execute(() => Connection.GetDatabase());
        }
        
        private static System.Net.EndPoint[] GetEndPoints()
        {
            return retryPolicy.Execute(() => Connection.GetEndPoints());
        }
        
        private static IServer GetServer(string host, int port)
        {
            return retryPolicy.Execute(() => Connection.GetServer(host,port));
        }
        
        private static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}