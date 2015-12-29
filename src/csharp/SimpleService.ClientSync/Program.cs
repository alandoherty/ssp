using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            }

            // send test message
            consumer.Message("TestCommand001", new {
                potato = 5
            });

            while (true)
                consumer.Poll();
        }
    }
}
