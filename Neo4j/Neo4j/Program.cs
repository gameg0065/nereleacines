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

        private async Task ImportDatabaseSchema()
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync("MERGE (a:Station:Train {name: 'Traku gelezinkelio stotis', trainCapacity: 1})MERGE (b:Station:Train {name: 'Lentvario gelezinkelio stotis', trainCapacity: 4})MERGE (c:Station:Train {name: 'Vilniaus gelezinkelio stotis', trainCapacity: 10})MERGE (d:Station:Train {name: 'Kauno gelezinkelio stotis', trainCapacity: 5})MERGE (e:Station:Train {name: 'Klaipedos gelezinkelio stotis', trainCapacity: 5})MERGE (b)-[:CONNECTS_TO {price: 1}]->(a)MERGE (a)-[:CONNECTS_TO {price: 6}]->(d)MERGE (c)-[:CONNECTS_TO {price: 3}]->(a)MERGE (b)-[:CONNECTS_TO {price: 2}]->(c)MERGE (d)-[:CONNECTS_TO {price: 5}]->(b)MERGE (d)-[:CONNECTS_TO {price: 7}]->(c)MERGE (c)-[:CONNECTS_TO {price: 20}]->(e)MERGE (e)-[:CONNECTS_TO {price: 15}]->(d)MERGE (train1:Train:Electric {name: 'Vilnius-Lentvaris-Trakai-Lenvtaris-Vilnius', peopleCapacity: 60, seatReservation: False})MERGE (train2:Train:Diesel {name: 'Vilnius-Klaipeda-Vilnius', peopleCapacity: 120, seatReservation: True})MERGE (train3:Train:Electric {name: 'Vilnius-Lentvaris-Kaunas-Lenvtaris-Vilnius', peopleCapacity: 80, seatReservation: False})MERGE (train4:Train:Electric {name: 'Vilnius-Kaunas-Vilnius', peopleCapacity: 80, seatReservation: False})MERGE (train1)-[:STOPS_AT]->(a)MERGE (train1)-[:STOPS_AT]->(b)MERGE (train1)-[:STOPS_AT]->(c)MERGE (b)<-[:STOPS_AT]-(train2)-[:STOPS_AT]->(d)MERGE (e)<-[:STOPS_AT]-(train2)-[:STOPS_AT]->(c)MERGE (train3)-[:STOPS_AT]->(c)MERGE (train3)-[:STOPS_AT]->(a)MERGE (train3)-[:STOPS_AT]->(d)MERGE (d)<-[:STOPS_AT]-(train4)-[:STOPS_AT]->(c)");

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
            
            await greeter.ClearDatabase();
            await greeter.ImportDatabaseSchema();
            Console.WriteLine("Duombaze importuota sekmingai.");
            
            Console.WriteLine("\nFind existing trains by name:");
            await greeter.FindTrainsByName("Vilnius-Kaunas-Vilnius");
            
            Console.WriteLine("\nFind existing trains that begins with Vilnius-Lentvaris:");
            await greeter.FindTrainsThatContainName("Vilnius-Lentvaris");
            
            Console.WriteLine("\nFind trains that get from station to station:");
            await greeter.FindTrainsToGetFromStationToStation("Vilniaus gelezinkelio stotis",
                  "Lentvario gelezinkelio stotis");
            
            Console.WriteLine("\nFind trains shortest trains to get from and to station:");
            await greeter.FindShortestToGetFromStationToStation("Vilniaus gelezinkelio stotis",
                 "Kauno gelezinkelio stotis");
            
            Console.WriteLine("\nFind trains to get from and to station with limited number of hopes:");
            await greeter.FindStopstFromStationToStation("Vilniaus gelezinkelio stotis",
                "Kauno gelezinkelio stotis", 3);
        }

        class Train
        {
            public string Name { get; set; }
            
            public int PeopleCapacity { get; set; }
            
            public bool SeatReservation { get; set; }
        }

        private async Task FindTrainsByName(string name)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync($"MATCH (train:Train) Where train.name = '{name}' RETURN train.name, train.peopleCapacity, train.seatReservation");
                var queryResult = await result.ToListAsync(record => new Train()
                {
                    Name = record["train.name"].As<string>(),
                    PeopleCapacity = record["train.peopleCapacity"].As<int>(),
                    SeatReservation = record["train.seatReservation"].As<bool>()
                });

                foreach (var train in queryResult)
                {
                    Console.WriteLine($"{train.Name}, {train.PeopleCapacity}, {train.SeatReservation}");
                }
            });
        }
        
        private async Task FindTrainsThatContainName(string name)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync($"MATCH (train:Train) Where train.name Contains '{name}' RETURN train.name, train.peopleCapacity, train.seatReservation");
                var queryResult = await result.ToListAsync(record => new Train()
                {
                    Name = record["train.name"].As<string>(),
                    PeopleCapacity = record["train.peopleCapacity"].As<int>(),
                    SeatReservation = record["train.seatReservation"].As<bool>()
                });

                foreach (var train in queryResult)
                {
                    Console.WriteLine($"{train.Name}, {train.PeopleCapacity}, {train.SeatReservation}");
                }
            });
        }
        
        private async Task FindTrainsToGetFromStationToStation(string startStation, string endStation)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync("MATCH (start:Station {name:'" + startStation + "'}), (end:Station  {name:'" + endStation +"'}), paths = allShortestPaths((start)-[:STOPS_AT*]-(end)) RETURN [node IN nodes(paths) | Case  WHEN node:Train and not node:Station THEN node.name Else '' END] as resultArray");
                var queryResult = await result.ToListAsync(record => record["resultArray"].As<List<string>>());
                
                foreach (var station in queryResult)
                {
                    foreach (var name in station.Where(x => x.Length > 0))
                    {
                        Console.WriteLine($"{name}");
                    }
                }
            });
        }
        
        private async Task FindShortestToGetFromStationToStation(string startStation, string endStation)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync("MATCH (start:Station{name:'"+ startStation +"'}), (end:Station{name:'" + endStation+ "'}),paths = allShortestPaths((start) -[:CONNECTS_TO*]-> (end))RETURN paths, reduce(acc3 = 0, v3 in [rel in relationships(paths) | rel.price] | acc3 + v3) as cost,[node IN nodes(paths) | node.name] AS stationNames");
                var queryResult = await result.ToListAsync(record => (record["cost"].As<int>(), record["stationNames"].As<List<string>>()));
                
                foreach (var station in queryResult)
                {
                    foreach (var stat in station.Item2)
                    {
                        Console.Write($"{stat} - ");
                    }
                    Console.Write($"{station.Item1} pinigeliai\n");
                }
            });
        }
        
        private async Task FindStopstFromStationToStation(string startStation, string endStation, int numHops)
        {
            await using var session = _driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                var result = await tx.RunAsync("MATCH stations = (start:Station{name:'"+ startStation +"'}) -[:CONNECTS_TO *.." + numHops+ "]-> (end:Station{name:'" + endStation+ "'}) RETURN stations,size(relationships(stations)) AS numHops, [node IN nodes(stations) | node.name] AS stationNames, [r IN relationships(stations) | r.price] AS costs, reduce(acc3 = 0, v3 in [rel in relationships(stations) | rel.price] | acc3 + v3) as Totalcost ORDER BY numHops, Totalcost");
                var queryResult = await result.ToListAsync(record => (record["Totalcost"].As<int>(), record["costs"].As<List<int>>(), record["stationNames"].As<List<string>>()));
                
                foreach (var station in queryResult)
                {
                    Console.Write($"Stations:");
                    foreach (var stat in station.Item3)
                    {
                        Console.Write($"{stat} - ");
                    }
                    Console.Write($"Price per stations: ");
                    foreach (var price in station.Item2)
                    {
                        Console.Write($"{price} - ");
                    }
                    Console.Write($" total price - {station.Item1} pinigeliai\n");
                }
            });
        }
    }
}