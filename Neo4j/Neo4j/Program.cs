using Neo4j.Driver;

namespace Neo4j.ConsoleApp
{
    class Program : IDisposable
    {
        private bool _disposed;
        private readonly IDriver _driver;

        ~Program() => Dispose(false);

        private Program(string uri, string user, string password)
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        }

        private async Task PrintGreeting(string message)
        {
            await using var session = _driver.AsyncSession();
            var greeting = await session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync("CREATE (a:Greeting) " +
                                    "SET a.message = $message " +
                                    "RETURN a.message + ', from node ' + id(a)",
                    new {message});
                
                return result.SingleAsync().Result[0].As<string>();
            });
            Console.WriteLine(greeting);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _driver?.Dispose();
            }

            _disposed = true;
        }
        
        static async Task Main(string[] args)
        {
            using var greeter = new Program("bolt://localhost:7687", "neo4j", "test");
            
            await greeter.PrintGreeting("hello, world");
            Console.WriteLine("test");
        }

    }
}