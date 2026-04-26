using System;
using Xunit;
using Xunit.Abstractions;
using BCrypt.Net;

namespace ConstructionStockAPI.Tests
{
    public class HashTest
    {
        private readonly ITestOutputHelper _output;

        public HashTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void PrintHash()
        {
            var hash = BCrypt.Net.BCrypt.HashPassword("admin123");
            _output.WriteLine("BcryptHash=" + hash);
        }
    }
}
