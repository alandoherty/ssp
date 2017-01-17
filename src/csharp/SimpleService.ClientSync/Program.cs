using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleService.ClientSync
{
    class Program
    {
        static void Main(string[] args) {
            Consumer consumer = null;

            try {
                consumer = new Consumer(args[0], int.Parse(args[1]));
            } catch(Exception) {
                Console.WriteLine("failed to connect to " + args[0] + ":" + args[1]);
                Console.ReadKey();
                return;
            }

            Stopwatch stopwatch = new Stopwatch();

            while (true) {
                stopwatch.Start();
                consumer.Poll();
                consumer.Request("CanWeDoThis", new {
                    CanYouHearMe = 5
                }, new ServiceResponseHandler(delegate (JObject obj) {
                    Console.WriteLine(obj.ToString(Formatting.Indented));
                }));
                stopwatch.Stop();
                Thread.Sleep((10 - (int)stopwatch.ElapsedMilliseconds) < 1 ? 0 : (10 - (int)stopwatch.ElapsedMilliseconds));
                stopwatch.Reset();
            }
        }
    }
}
