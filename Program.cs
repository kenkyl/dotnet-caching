﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using StackExchange.Redis;
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
            return id + "," + name + "," + country + "," + email;
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
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }

        static (string, int) FetchUser(IDatabase dbRedis, MySqlConnection conn, int userId) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string fetchedUser = "";
            // ** 1. attempt to fetch from cache 
            RedisValue cacheRes = dbRedis.StringGet($"users:{userId}");
            if (cacheRes.IsNullOrEmpty) {
                Console.WriteLine($"\nCache miss for User {userId}... fetching from database");
                // ** 2.a.i. cache miss, fetch from database 
                string sql = $"SELECT Id, Name, Country, Email FROM Users WHERE Id='{userId}'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    fetchedUser = rdr[0]+","+rdr[1]+","+rdr[2]+","+rdr[3];
                }
                rdr.Close();
                // ** 2.a.ii. load cache with TTL 15 sec
                TimeSpan ttl = new TimeSpan(0,0,15); 
                dbRedis.StringSet($"users:{userId}", fetchedUser, expiry:ttl);

            } else {
                // ** 2.b. cache hit
                Console.WriteLine($"\nCache hit for User {userId}!");
                fetchedUser = cacheRes.ToString();
            }
            
            // ** 3. return user 
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            int queryTime = ts.Milliseconds;
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

            // load data into sql 
            LoadData(conn);            

            while (true) {
                Console.Write("\nEnter a command: ");
                var command = Console.ReadLine();
                string[] words = command.Split(' ');
                if (words[0].Equals("quit")) 
                {
                    break;
                }
                else if (words[0].Equals("get")) {
                    var id = Int32.Parse(words[1]); 
                    (string user, int time) res = FetchUser(dbRedis, conn, id);
                    Console.WriteLine($"Execution time: {res.time} ms\nFetched user: {res.user}");
                }
            }

            conn.Close();
            Console.WriteLine("Done.");
        }
    }
}
