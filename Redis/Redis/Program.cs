using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using StackExchange.Redis;
using System;

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
                database.StringSet($"Tool{i + 1}", Tools[i]);
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
    }
}