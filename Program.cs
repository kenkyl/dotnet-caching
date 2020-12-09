using System;
using StackExchange.Redis;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace dotnet_caching
{
    class Program
    {
        public const string redisHost = "localhost";
        public const int redisPort = 6379;


        void LoadData() {
            // load database with dummy user info
        }

        void FetchUser(string userId) {
            // attempt to fetch from cache 
            // if failed, fetch from db 
            // if fetched from db, load cache 
            // return user 
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
            string connStr = "server=localhost;user=root;database=users;port=3306;password=password";
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
