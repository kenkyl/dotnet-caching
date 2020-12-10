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

            }

            // load database with dummy user info
            List<User> users = new List<User>() {
                new User() {id=1, name="Fred Angerman", country="US", email="angerfred@mail.com"},
                new User() {id=2, name="Carrie Bartine", country="CA", email="bartcar@achoo.com"}
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

        static void FetchUser(IDatabase dbRedis, MySqlConnection conn, int userId) {
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
                Console.WriteLine(rdr[0]+" -- "+rdr[1]+" -- "+rdr[2]+" -- "+rdr[3]);
            }
            rdr.Close();
        }

        static void Main(string[] args)
        {
            // establsih mysql connection 


            // establish redis connection
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"{redisHost}:{redisPort}");
            IDatabase dbRedis = redis.GetDatabase();
            string value = "abcdefg";
            dbRedis.StringSet("mykey", value);
            
            string value2 = dbRedis.StringGet("mykey");
            Console.WriteLine(value2);

            // establish mysql connection
            string connStr = $"server={mysqlHost};user={mysqlUser};database={mysqlDatabase};port=3306;password=password";
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();
                // Perform database operations
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            LoadData(conn);
            FetchUser(dbRedis, conn, 1);
            

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
