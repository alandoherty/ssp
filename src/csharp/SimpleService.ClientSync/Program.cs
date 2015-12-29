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
            Consumer consumer;
            try {
                consumer = new Consumer("localhost", 3333);
            } catch(Exception) {
                Console.WriteLine("failed to connect to localhost:3333");
            }

            // test message
            consumer.Message()
            while (true) {
                consumer.Poll();
            }
        }
    }
}
