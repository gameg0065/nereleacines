using Cassandra;
using System;
using Cassandra.DataStax.Auth;

namespace Cassandra.DB
{
    class CassandraConnect
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello CassandraUni!");

            var cluster = Cluster.Builder()
                .AddContactPoints("127.0.0.1").WithPort(9042)
                .WithAuthProvider(new PlainTextAuthProvider("cassandra", "cassandra"))
                .Build();
            var session = cluster.Connect("duombaziuuni");
            Console.WriteLine("Connected to cluster: " + cluster.Metadata.ClusterName);
            CreateAuthorTable(session);
            CreateBooksTable(session);
            CreateBooksTableByTitle(session);
            CreateBooksTableByAuthors(session);
            
            Console.WriteLine("Data load is done.");
            
            Console.WriteLine("Show all books available in database:");
            var result1 = session.Execute("SELECT * FROM books;").Select(row => (row.GetValue<string>("title"), row.GetValue<string>("releasedate"), row.GetValue<int>("bookid")));
            foreach (var result in result1)
            {
                Console.WriteLine($"{result.Item3} {result.Item1}  {result.Item2}");
            }
            
            Console.WriteLine("Show books filtered by id:");
            var result2 = session.Execute("SELECT * FROM books WHERE bookID = 1;").Select(row => (row.GetValue<string>("title"), row.GetValue<string>("releasedate"), row.GetValue<int>("bookid")));
            foreach (var result in result2)
            {
                Console.WriteLine($"{result.Item3} {result.Item1}  {result.Item2}");
            }
            
            Console.WriteLine("Show books filtered by title:");
            var result3 = session.Execute("SELECT * FROM books_by_title WHERE title = 'Tas';").Select(row => (row.GetValue<string>("title"), row.GetValue<string>("releasedate"), row.GetValue<int>("bookid")));
            foreach (var result in result3)
            {
                Console.WriteLine($"{result.Item3} {result.Item1}  {result.Item2}");
            }

            Console.WriteLine("\nShow books written by J.K. Rowling");
            var result4 = session.Execute("SELECT * FROM books_by_authors WHERE authorId = 2;").Select(row => (row.GetValue<string>("title"), row.GetValue<string>("releasedate"), row.GetValue<int>("bookid")));
            foreach (var result in result4)
            {
                Console.WriteLine($"{result.Item3} {result.Item1}  {result.Item2}");
            }
            
            Console.WriteLine("\nShow all writters:");
            var result5 = session.Execute("SELECT * FROM authors;").Select(row => (row.GetValue<string>("name"), row.GetValue<string>("lastname"), row.GetValue<int>("authorid")));
            foreach (var result in result5)
            {
                Console.WriteLine($"{result.Item3} {result.Item1}  {result.Item2}");
            }
            
            Console.WriteLine("\nFind writter by id:");
            var result6 = session.Execute("SELECT * FROM authors Where authorId = 3;").Select(row => (row.GetValue<string>("name"), row.GetValue<string>("lastname"), row.GetValue<int>("authorid")));
            foreach (var result in result6)
            {
                Console.WriteLine($"{result.Item3} {result.Item1}  {result.Item2}");
            }


        }
        private static void CreateAuthorTable(ISession session)
        {
            session.Execute("DROP TABLE if exists authors;");
            session.Execute("CREATE TABLE authors (authorID INT PRIMARY KEY, name TEXT,lastName TEXT);");
            session.Execute("BEGIN BATCH INSERT INTO authors (authorID, name, lastName) VALUES (1,'William', 'Shakespeare'); INSERT INTO authors (authorID, name, lastName) VALUES (2,'Joanne', 'Rowling');INSERT INTO authors (authorID, name, lastName) VALUES (3,'Agatha', 'Christie');INSERT INTO authors (authorID, name, lastName) VALUES (4,'Stephen', 'King');INSERT INTO authors (authorID, name, lastName) VALUES (5,'Georges', 'Simenon'); APPLY BATCH;");
        }
        
        private static void CreateBooksTable(ISession session)
        {
            session.Execute("DROP TABLE if exists books;");
            session.Execute("CREATE TABLE books (bookID INT PRIMARY KEY, title TEXT,releaseDate TEXT);");
            session.Execute("BEGIN BATCH INSERT INTO books (bookID, title, releaseDate) VALUES (1,'Hamletas', '1605');INSERT INTO books (bookID, title, releaseDate) VALUES (2,'Romeo ir Džiuljeta', '1597');INSERT INTO books (bookID, title, releaseDate) VALUES (3,'Karalius Lyras', '1608');INSERT INTO books (bookID, title, releaseDate) VALUES (4,'Haris Poteris ir išminties akmuo', '1997');INSERT INTO books (bookID, title, releaseDate) VALUES (5,'Haris Poteris ir paslapčių kambarys', '1998');INSERT INTO books (bookID, title, releaseDate) VALUES (6,'Haris Poteris ir Azkabano kalinys', '1999');INSERT INTO books (bookID, title, releaseDate) VALUES (7,'Haris Poteris ir Ugnies taurė', '2000');INSERT INTO books (bookID, title, releaseDate) VALUES (8,'Haris Poteris ir Fenikso brolija', '2003');INSERT INTO books (bookID, title, releaseDate) VALUES (9,'Haris Poteris ir Netikras Princas', '2005');INSERT INTO books (bookID, title, releaseDate) VALUES (10,'Haris Poteris ir Mirties relikvijos', '2007');INSERT INTO books (bookID, title, releaseDate) VALUES (11,'Fantastiniai gyvūnai ir kur juos rasti', '2001');INSERT INTO books (bookID, title, releaseDate) VALUES (12,'Žmogžudystė Rytų eksprese', '1934');INSERT INTO books (bookID, title, releaseDate) VALUES (13,'Mirtis ant Nilo', '1937');INSERT INTO books (bookID, title, releaseDate) VALUES (14,'Tas', '1986');INSERT INTO books (bookID, title, releaseDate) VALUES (15,'Fairy Tale', '2022');INSERT INTO books (bookID, title, releaseDate) VALUES (16,'Maigret', '1934');INSERT INTO books (bookID, title, releaseDate) VALUES (17,'The blue room', '1964');APPLY BATCH;");
        }
        private static void CreateBooksTableByTitle(ISession session)
        {
            session.Execute("DROP TABLE if exists books_by_title;");
            session.Execute("CREATE TABLE books_by_title (bookID INT, title TEXT, releaseDate TEXT, PRIMARY KEY ((title), releaseDate))");
            session.Execute("BEGIN BATCH INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (1,'Hamletas', '1605');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (2,'Romeo ir Džiuljeta', '1597');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (3,'Karalius Lyras', '1608');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (4,'Haris Poteris ir išminties akmuo', '1997');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (5,'Haris Poteris ir paslapčių kambarys', '1998');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (6,'Haris Poteris ir Azkabano kalinys', '1999');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (7,'Haris Poteris ir Ugnies taurė', '2000');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (8,'Haris Poteris ir Fenikso brolija', '2003');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (9,'Haris Poteris ir Netikras Princas', '2005');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (10,'Haris Poteris ir Mirties relikvijos', '2007');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (11,'Fantastiniai gyvūnai ir kur juos rasti', '2001');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (12,'Žmogžudystė Rytų eksprese', '1934');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (13,'Mirtis ant Nilo', '1937');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (14,'Tas', '1986');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (15,'Fairy Tale', '2022');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (16,'Maigret', '1934');INSERT INTO books_by_title (bookID, title, releaseDate) VALUES (17,'The blue room', '1964');APPLY BATCH;");
        }
        
        private static void CreateBooksTableByAuthors(ISession session)
        {
            session.Execute("DROP TABLE if exists books_by_authors;");
            session.Execute("CREATE TABLE books_by_authors (authorID INT, bookID INT, title TEXT, releaseDate TEXT, name TEXT,lastName TEXT, PRIMARY KEY ((authorID), bookID, name));");
            session.Execute("BEGIN BATCH INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (1, 1,'Hamletas', '1605', 'William', 'Shakespeare' );INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (1, 2,'Romeo ir Džiuljeta', '1597', 'William', 'Shakespeare');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (1, 3,'Karalius Lyras', '1608', 'William', 'Shakespeare');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (2,4,'Haris Poteris ir išminties akmuo', '1997', 'Joanne', 'Rowling');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (2,5,'Haris Poteris ir paslapčių kambarys', '1998', 'Joanne', 'Rowling');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (2,6,'Haris Poteris ir Azkabano kalinys', '1999', 'Joanne', 'Rowling');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (2,7,'Haris Poteris ir Ugnies taurė', '2000', 'Joanne', 'Rowling');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (2,8,'Haris Poteris ir Fenikso brolija', '2003', 'Joanne', 'Rowling');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (2,9,'Haris Poteris ir Netikras Princas', '2005', 'Joanne', 'Rowling');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (2,10,'Haris Poteris ir Mirties relikvijos', '2007', 'Joanne', 'Rowling');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (2,11,'Fantastiniai gyvūnai ir kur juos rasti', '2001', 'Joanne', 'Rowling');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (3,12,'Žmogžudystė Rytų eksprese', '1934', 'Agatha', 'Christie');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (3,13,'Mirtis ant Nilo', '1937', 'Agatha', 'Christie');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (4,14,'Tas', '1986', 'Stephen', 'King');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (4,15,'Fairy Tale', '2022', 'Stephen', 'King');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (5,16,'Maigret', '1934', 'Georges', 'Simenon');INSERT INTO books_by_authors (authorID, bookID, title, releaseDate, name, lastName) VALUES (5,17,'The blue room', '1964', 'Georges', 'Simenon'); APPLY BATCH;");
            //session.Execute("BEGIN BATCH APPLY BATCH;");
        }
    }
}