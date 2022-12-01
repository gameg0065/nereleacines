using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

namespace Redis.ConsoleApp
{
    class Program
    {
        private static List<string> Tools = new() { "Lamp", "BigLamp", "Drill", "Hammer drill", "Sander" };
        
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
                userName = Console.ReadLine();
                lastName = Console.ReadLine();
                try
                {
                    Console.WriteLine("Enter fist name");
                    ch = Convert.ToChar(userName[0]);
                    Console.WriteLine("Enter last name");
                    ch = Convert.ToChar(lastName[0]);

                    if (userName.Length != 0 && lastName.Length != 0)
                    {
                        Console.WriteLine(userName);
                    }
                }
                catch (OverflowException e)
                {
                    Console.WriteLine("{0} Value read.", e.Message);
                    ch = Char.MinValue;
                    Console.WriteLine("\nType name then press Enter type lastname then press Enter. Type '+' anywhere in the text to quit:\n");
                }
            } while (ch != '+');
        
            var commandresponse = database.Execute("PING");
            Console.WriteLine(commandresponse.ToString());        
            
            //Set a Value in Cache
            //database.StringSet("TestConsole", "Hello from Console App, how are you doing");

            //Read recently setted Value From Cache
            var cachedresponse = database.StringGet("Tool1");
            Console.WriteLine(cachedresponse.ToString());
        }
        
        private class RentingGuy
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string LastName { get; set; }
            public List<string> ReservedTools { get; set; }
        }

        private static void AddNewHuman(IDatabase database, RentingGuy human)
        {
            var jsonString = JsonConvert.SerializeObject(human);
            //database.HashSet()
            database.StringSet($"human{human.Id}", jsonString);
        }
        
        private static Lazy<ConnectionMultiplexer> lazyConnection = CreateConnection();

        private static ConnectionMultiplexer Connection
        {
            get { return lazyConnection.Value; }
        }

        private static void LoadTools(IDatabase database)
        {
            var toolName = database.StringGet("Tool1");
            
            if (toolName.ToString().Length != 0)
            {
                return;
            }
            
            for (var i = 0; i < Tools.Count; i++)
            {
                database.StringSet(GetHashString(Tools[i]), Tools[i]);
            }
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