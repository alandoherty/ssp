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
            for (int i = 0; i < 10; i++) {
                consumer.Message("TestCommand001", new {
                    potato = i + 1
                });
                System.Threading.Thread.Sleep(1000);
            }

            while (true)
                consumer.Poll();
        }
    }
}
