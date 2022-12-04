using System.Collections.ObjectModel;
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
        
        private async Task ClearDatabase()
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync("match (a) -[r] -> () delete a, r");
                await tx.RunAsync("match (a) delete a");
                //await tx.RunAsync("CALL gds.graph.drop('myGraph', false)");
            });
        }

        private async Task ImportDatabaseSchema1()
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync("    MERGE (a:Station:Train {name: 'Traku gelezinkelio stotis', trainCapacity: 1})MERGE (b:Station:Train {name: 'Lentvario gelezinkelio stotis', trainCapacity: 4})MERGE (c:Station:Train {name: 'Vilniaus gelezinkelio stotis', trainCapacity: 10})MERGE (d:Station:Train {name: 'Kauno gelezinkelio stotis', trainCapacity: 5})MERGE (e:Station:Train {name: 'Klaipedos gelezinkelio stotis', trainCapacity: 5})MERGE (a)-[:CONNECTS_TO {price: '1'}]->(b)<-[:CONNECTS_TO {price: '1'}]-(a)MERGE (b)-[:CONNECTS_TO {price: '1.2'}]->(c)<-[:CONNECTS_TO {price: '1.2'}]-(b)-[:CONNECTS_TO {price: '4'}]->(d)<-[:CONNECTS_TO {price: '4'}]-(b)MERGE (c)-[:CONNECTS_TO {price: '6'}]->(d)<-[:CONNECTS_TO {price: '6'}]-(c)-[:CONNECTS_TO {price: '20'}]->(e)<-[:CONNECTS_TO {price: '20'}]-(c)MERGE (train1:Train:Electric {name: 'Vilnius-Lentvaris-Trakai-Lenvtaris-Vilnius', peopleCapacity: 60, seatReservation: False})MERGE (train2:Train:Diesel {name: 'Vilnius-Klaipeda-Vilnius', peopleCapacity: 120, seatReservation: True})MERGE (train3:Train:Electric {name: 'Vilnius-Lentvaris-Kaunas-Lenvtaris-Vilnius', peopleCapacity: 80, seatReservation: False})MERGE (train4:Train:Electric {name: 'Vilnius-Kaunas-Vilnius', peopleCapacity: 80, seatReservation: False})MERGE (train1)-[:STOPS_AT]->(a)MERGE (train1)-[:STOPS_AT]->(b)MERGE (train1)-[:STOPS_AT]->(c)MERGE (e)<-[:STOPS_AT]-(train2)-[:STOPS_AT]->(c)MERGE (train3)-[:STOPS_AT]->(c)MERGE (train3)-[:STOPS_AT]->(a)MERGE (train3)-[:STOPS_AT]->(d)MERGE (d)<-[:STOPS_AT]-(train4)-[:STOPS_AT]->(c)");
            });
        }
        
        private async Task CreateTable()
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync("CALL gds.graph.project('myGraph','City','ROAD',{ relationshipProperties: 'cost' })");
            });
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

            //await greeter.PrintGreeting("hello, world");
            //await greeter.ClearDatabase();
            //await greeter.ImportDatabaseSchema();
            //await greeter.CreateTable();
            //await greeter.FindTrainsByName("Bus-A");

            await greeter.ImportDatabaseSchema2();
            Console.WriteLine("test");
        }

        private async Task FindTrainsByName(string name)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync("MATCH (bus:Bus {name:'" + name +"'}) RETURN bus.name as name");
                var queryResult = await result.ToListAsync(record => record["name"].As<string>());

                foreach (var name in queryResult)
                {

                    Console.WriteLine(name);
                }
            });
        }
        
        private async void FindTrainsThatContains()
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync("MERGE (a:City {name: 'A'}) MERGE (b:City {name: 'B'}) MERGE (c:City {name: 'C'})MERGE (d:City {name: 'D'})MERGE (e:City {name: 'E'})MERGE (f:City {name: 'F'})MERGE (a) -[:ROAD {cost: 10}]-> (b)MERGE (a) -[:ROAD {cost: 7}]-> (c)MERGE (a) -[:ROAD {cost: 10}]-> (d)MERGE (a) -[:ROAD {cost: 150}]-> (d)MERGE (a) -[:ROAD {cost: 200}]-> (e)MERGE (b) -[:ROAD {cost: 15}]-> (c)MERGE (c) -[:ROAD {cost: 5}]-> (a)MERGE (c) -[:ROAD {cost: 150}]-> (a)MERGE (d) -[:ROAD {cost: 8}]-> (a)MERGE (d) -[:ROAD {cost: 10}]-> (e)MERGE (e) -[:ROAD {cost: 150}]-> (a)MERGE (e) -[:ROAD {cost: 1500}]-> (a)MERGE (e) -[:ROAD {cost: 10}]-> (f)MERGE (f) -[:ROAD {cost: 15}]-> (e)MERGE (busA:Bus {name: 'Bus-A'})MERGE (busB:Bus {name: 'Bus-B'})MERGE (busC:Bus {name: 'Bus-C'})MERGE (busD:Bus {name: 'Bus-D'})MERGE (busA) -[:STOPS_AT]-> (a)MERGE (busA) -[:STOPS_AT]-> (b)MERGE (busA) -[:STOPS_AT]-> (c)MERGE (busB) -[:STOPS_AT]-> (a)MERGE (busB) -[:STOPS_AT]-> (c)MERGE (busB) -[:STOPS_AT]-> (d)MERGE (busC) -[:STOPS_AT]-> (a)MERGE (busC) -[:STOPS_AT]-> (d)MERGE (busC) -[:STOPS_AT]-> (e)MERGE (busD) -[:STOPS_AT]-> (a)MERGE (busD) -[:STOPS_AT]-> (e)MERGE (busD) -[:STOPS_AT]-> (f)");
            });
        }
    }
}