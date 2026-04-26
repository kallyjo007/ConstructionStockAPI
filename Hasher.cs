using System;

namespace Hasher
{
    class Program
    {
        static void Main(string[] args)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword("admin123");
            System.Console.WriteLine("---HASH_START---");
            System.Console.WriteLine(hash);
            System.Console.WriteLine("---HASH_END---");
        }
    }
}
