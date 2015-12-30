using Newtonsoft.Json.Linq;
using System;

namespace SimpleService.HostSync
{
    public class Program
    {
        static void Main(string[] args) {
            Console.WriteLine("Starting service host on port 3333...");
            Host host = new Host(3333);

            host.Bind("TestCommand001", Visibility.Private, delegate (JObject obj) {
                Console.WriteLine(obj.ToString(Newtonsoft.Json.Formatting.Indented));
            });

            while(true)
                host.Poll();
        }
    }
}