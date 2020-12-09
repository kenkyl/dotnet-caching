﻿using System;
using StackExchange.Redis;

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


            // Console.WriteLine("\nWhat is your name? ");
            // var name = Console.ReadLine();
            // var date = DateTime.Now;
            // Console.WriteLine($"\nHello, {name}, on {date:d} at {date:t}!");
            // Console.Write("\nPress any key to exit...");
            // Console.ReadKey(true);
        }
    }
}
