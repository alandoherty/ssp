using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Threading;

namespace SimpleService.HostSync
{
    public class Program
    {
        static void Main(string[] args) {
            Console.WriteLine("Starting service host on port 3333...");
            Host host = new Host(3333);

            host.Bind("CanWeDoThis", Visibility.Public, delegate (JObject obj) {
                Console.WriteLine(obj.ToString(Formatting.Indented));
                return new {
                    Answer = true
                };
            });

            host.Bind("TestService", Visibility.Public, delegate (JObject obj) {
                Console.WriteLine(obj.ToString(Formatting.Indented));
            });

            Stopwatch stopwatch = new Stopwatch();
            while (true) {
                stopwatch.Start();
                host.Poll();
                stopwatch.Stop();
                Thread.Sleep((10 - (int)stopwatch.ElapsedMilliseconds) < 1 ? 0 : (10 - (int)stopwatch.ElapsedMilliseconds));
                stopwatch.Reset();
            }
        }
    }
}