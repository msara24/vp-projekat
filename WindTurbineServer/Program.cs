using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;

namespace WindTurbineServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host =
                new ServiceHost(
                    typeof(WindTurbineService));

            host.Open();

            Console.WriteLine(
                "WCF Server started...");

            Console.ReadLine();

            host.Close();
        }
    }
}
