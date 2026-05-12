using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using WindTurbineContracts;

namespace WindTurbineClient
{
    public class CsvReader
    {
        public List<WindTurbineSample> ReadCsv(
            string path,
            string turbineId)
        {
            List<WindTurbineSample> samples =
                new List<WindTurbineSample>();

            using (StreamReader sr =
                   new StreamReader(path))
            {
                for (int i = 0; i < 9; i++)
                {
                    sr.ReadLine();
                }

                string header = sr.ReadLine();

                string line;
                int rowIndex = 11;

                while ((line = sr.ReadLine()) != null)
                {
                    if (samples.Count >= 100)
                    {
                        break;
                    }

                    try
                    {
                        string[] parts = line.Split(',');

                        WindTurbineSample sample =
                            new WindTurbineSample();

                        sample.Timestamp =
                            DateTime.Parse(
                                parts[0],
                                CultureInfo.InvariantCulture);

                        sample.WindSpeed =
                            ParseDouble(parts[1]);

                        sample.WindDirection =
                            ParseDouble(parts[2]);

                        sample.NacellePosition =
                            ParseDouble(parts[3]);

                        sample.PowerKW =
                            ParseDouble(parts[4]);

                        sample.PotentialPowerDefaultKW =
                            ParseDouble(parts[5]);

                        sample.PowerFactor =
                            ParseDouble(parts[6]);

                        sample.ReactivePowerKvar =
                            ParseDouble(parts[7]);

                        sample.GridFrequencyHz =
                            ParseDouble(parts[8]);

                        sample.GeneratorRpm =
                            ParseDouble(parts[9]);

                        sample.RowIndex = rowIndex;
                        sample.TurbineId = turbineId;

                        samples.Add(sample);
                    }
                    catch
                    {
                        File.AppendAllText(
                            "client_rejects.log",
                            $"Invalid row: {rowIndex}\n");
                    }

                    rowIndex++;
                }
            }

            return samples;
        }

        private double ParseDouble(string value)
        {
            if (value == "NaN")
            {
                return double.NaN;
            }

            return double.Parse(
                value,
                CultureInfo.InvariantCulture);
        }
    }
}