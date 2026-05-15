using System;
using System.Collections.Generic;
using System.ServiceModel;
using WindTurbineContracts;

namespace WindTurbineClient
{
    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();

            // IMPORTANT
            binding.Security.Mode = SecurityMode.None;

            binding.TransferMode = TransferMode.Streamed;

            binding.MaxReceivedMessageSize = 10485760;

            EndpointAddress address =
                new EndpointAddress(
                    "net.tcp://localhost:9000/WindService");

            ChannelFactory<IWindTurbineService> factory =
                new ChannelFactory<IWindTurbineService>(
                    binding,
                    address);

            IWindTurbineService proxy =
                factory.CreateChannel();

            CsvReader reader =
                new CsvReader();

            List<WindTurbineSample> samples =
                reader.ReadCsv(
                    "Turbine_Data_Kelmarsh_1_2018-01-01_-_2019-01-01_228.csv",
                    "Kelmarsh_1");

            try
            {
                proxy.StartSession("Kelmarsh_1");

                Console.WriteLine("Session started");

                for (int i = 0; i < samples.Count; i += 10)
                {
                    List<WindTurbineSample> batch =
                        samples.GetRange(
                            i,
                            Math.Min(10, samples.Count - i));

                    string response =
                        proxy.PushSample(batch);

                    Console.WriteLine(response);

                    Console.WriteLine(
                        $"Sent batch {(i / 10) + 1}");
                }

                Console.WriteLine("Samples sent");

                proxy.EndSession();

                Console.WriteLine("Transfer completed");

                ((ICommunicationObject)proxy).Close();

                factory.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                factory.Abort();
            }

            Console.ReadLine();
        }
    }
}