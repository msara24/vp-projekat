using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using WindTurbineContracts;

namespace WindTurbineServer
{
    public class WindTurbineService : IWindTurbineService
    {
        private static string currentFile;

        public void StartSession(string turbineId)
        {
            string baseFolder =
                AppDomain.CurrentDomain.BaseDirectory;

            string date =
                DateTime.Now.ToString("yyyy-MM-dd");

            string folder =
                Path.Combine(
                    baseFolder,
                    "Data",
                    turbineId,
                    date);

            Directory.CreateDirectory(folder);

            currentFile =
                Path.Combine(folder, "session.csv");

            Console.WriteLine(
                $"Session started for {turbineId}");
        }

        public void PushSample(
            List<WindTurbineSample> samples)
        {
            Console.WriteLine(currentFile);
            using (StreamWriter sw =
                   new StreamWriter(currentFile, true))
            {
                foreach (var s in samples)
                {
                    sw.WriteLine(
                        $"{s.Timestamp}," +
                        $"{s.WindSpeed}," +
                        $"{s.PowerKW}");
                }
            }

            Console.WriteLine(
                $"Batch received: {samples.Count}");
        }

        public void EndSession()
        {
            Console.WriteLine(
                "Transfer completed");
        }
    }
}