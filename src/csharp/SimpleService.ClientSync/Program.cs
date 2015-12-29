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
                consumer = new Consumer(args[0], int.Parse(args[1]));
            } catch(Exception) {
                Console.WriteLine("failed to connect to " + args[0] + ":" + args[1]);
            }

            while (true)
                Console.ReadKey();
        }
    }
}
