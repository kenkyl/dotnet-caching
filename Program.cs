using System;
using System.Collections.Generic;
using StackExchange.Redis;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace dotnet_caching
{   
    public class User 
    {
        public int id { get; set; }
        public string name { get; set; }
        public String email { get; set; }
        public String country { get; set; }

        public override string ToString()
        {
            return id + "--" + name + "--" + country + "--" + email;
        }
    }

    class Program
    {
        // convert to env vars 
        public const string redisHost = "localhost";
        public const int redisPort = 6379;
        public const string mysqlHost = "localhost";
        public const int mysqlPort = 3306;
        public const string mysqlDatabase = "database";
        public const string mysqlUser = "root";
        public const string mysqlPassword = "password";


        static void LoadData(MySqlConnection conn) {
            // Create Users table if it does not exist 
            try 
            {
                MySqlCommand createTable = new MySqlCommand(@"
                    CREATE TABLE IF NOT EXISTS `Users` (
                    id INT NOT NULL,
                    name VARCHAR(32) NOT NULL,
                    country VARCHAR(2) NOT NULL,
                    email VARCHAR(32) NOT NULL,
                    PRIMARY KEY (id))", conn)
                ;
                createTable.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.ToString()}");
            }

            // load database with dummy user info for 10 users\
            List<User> users = new List<User>() {
                new User() {id=1, name="Fred Angerman", country="US", email="angerfred@mail.com"},
                new User() {id=2, name="Carrie Bartine", country="CA", email="bartcar@achoo.com"},
                new User() {id=3, name="Gorth Borgenson", country="DE", email="gorth@mail.com"},
                new User() {id=4, name="Rose Ramirez", country="US", email="rosera@achoo.com"},
                new User() {id=5, name="Barry Williams", country="US", email="willbarry@mail.com"},
                new User() {id=6, name="Sue Parker", country="SE", email="swedesue@achoo.com"},
                new User() {id=7, name="Jonathan Smolinksi", country="DE", email="smoljon@mail.com"},
                new User() {id=8, name="Gertrude Fine", country="FR", email="gert.fine@achoo.com"},
                new User() {id=9, name="Anthony Leotardo", country="IT", email="leoan@mail.com"},
                new User() {id=10, name="Cindy Smithe", country="US", email="smindy@achoo.com"}
            };
            
            foreach (var user in users) {
                string sql = $@"INSERT INTO Users (Id, Name, Country, Email) 
                    VALUES ({user.id}, '{user.name}', '{user.country}', '{user.email}')
                    ON DUPLICATE KEY UPDATE Id=Id";
                Console.Write(sql + "\n");
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }

        static (string, int) FetchUser(IDatabase dbRedis, MySqlConnection conn, int userId) {
            int startTime = 0; 
            int endTime = 0;
            string fetchedUser = "";
            // attempt to fetch from cache 
            // if failed, fetch from db 
            // if fetched from db, load cache 
            // return user 
            string sql = $"SELECT Id, Name, Country, Email FROM Users WHERE Id='{userId}'";
            Console.Write(sql + "\n");
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                fetchedUser = rdr[0]+" -- "+rdr[1]+" -- "+rdr[2]+" -- "+rdr[3];
                Console.WriteLine(fetchedUser);
            }
            rdr.Close();
            int queryTime = endTime - startTime; 

            return (fetchedUser, queryTime);
        }

        static void Main(string[] args)
        {
            // establish redis connection
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"{redisHost}:{redisPort}");
            IDatabase dbRedis = redis.GetDatabase();
            
            // establish mysql connection
            string connStr = $"server={mysqlHost};user={mysqlUser};database={mysqlDatabase};port=3306;password=password";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.ToString()}");
            }


            LoadData(conn);
            FetchUser(dbRedis, conn, 1);
            

            

            while (true) {
                Console.WriteLine("\nEnter a command: ");
                var command = Console.ReadLine();
                string[] words = command.Split(' ');
                if (words[0].Equals("quit")) 
                {
                    break;
                }
                else if (words[0].Equals("get")) {
                    var id = Int32.Parse(words[1]); 
                    (string user, int time) res = FetchUser(dbRedis, conn, id);
                    Console.WriteLine($"\nExecution time: {res.time}\nFetched user: {res.user}");
                }

            }

            conn.Close();
            Console.WriteLine("Done.");

            // Console.WriteLine("\nWhat is your name? ");
            // var name = Console.ReadLine();
            // var date = DateTime.Now;
            // Console.WriteLine($"\nHello, {name}, on {date:d} at {date:t}!");
            // Console.Write("\nPress any key to exit...");
            // Console.ReadKey(true);
        }
    }
}
