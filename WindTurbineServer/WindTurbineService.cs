using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using WindTurbineContracts;

namespace WindTurbineServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WindTurbineService : IWindTurbineService, IDisposable
    {
        private string currentFile;
        private string basePath;
        private string rejectsFile;

        private StreamWriter sessionWriter;
        private StreamWriter rejectsWriter;

        private double lastWindSpeed = -1;
        private double lastNacelle = -1;
        private double lastPower = -1;

        // CONFIG THRESHOLDS
        private readonly double windSpikeThreshold =
    double.Parse(ConfigurationManager.AppSettings["WindSpikeThreshold"]);

        private readonly double lowWindCutIn =
            double.Parse(ConfigurationManager.AppSettings["CutInWindSpeed"]);

        private readonly double nacelleThreshold =
            double.Parse(ConfigurationManager.AppSettings["NacelleThreshold"]);

        private readonly double powerDropThreshold =
            double.Parse(ConfigurationManager.AppSettings["PowerDropThreshold"]);

        // EVENTS
        public event Action OnTransferStarted;
        public event Action<WindTurbineSample> OnSampleReceived;
        public event Action<int> OnBatchReceived;
        public event Action OnTransferCompleted;
        public event Action<string> OnWarningRaised;

        public void StartSession(string turbineId)
        {
            try
            {
                string date =
                    DateTime.Now.ToString("yyyy-MM-dd");

                basePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Data",
                    turbineId,
                    date);

                Directory.CreateDirectory(basePath);

                currentFile =
                    Path.Combine(basePath, "session.csv");

                rejectsFile =
                    Path.Combine(basePath, "rejects.csv");

                sessionWriter =
                    new StreamWriter(currentFile, true);

                rejectsWriter =
                    new StreamWriter(rejectsFile, true);

                Console.WriteLine(
                    $"Session started for {turbineId}");

                OnTransferStarted?.Invoke();
            }
            catch (Exception ex)
            {
                throw new FaultException(
                    $"StartSession error: {ex.Message}");
            }
        }

        public string PushSample(List<WindTurbineSample> samples)
        {
            try
            {
                if (sessionWriter == null ||
                    rejectsWriter == null)
                {
                    throw new FaultException(
                        "Session not started.");
                }

                foreach (var s in samples)
                {
                    try
                    {
                        // NULL CHECK
                        if (s == null)
                        {
                            rejectsWriter.WriteLine(
                                "NULL_SAMPLE");

                            continue;
                        }

                        // NaN VALIDATION
                        if (double.IsNaN(s.WindSpeed) ||
                            double.IsNaN(s.PowerKW))
                        {
                            rejectsWriter.WriteLine(
                                $"{s.RowIndex},INVALID_DATA");

                            continue;
                        }

                        // BASIC VALIDATION
                        if (s.GridFrequencyHz <= 0)
                        {
                            rejectsWriter.WriteLine(
                                $"{s.RowIndex},INVALID_FREQUENCY");

                            continue;
                        }

                        // WRITE DATA
                        sessionWriter.WriteLine(
                            $"{s.Timestamp}," +
                            $"{s.WindSpeed}," +
                            $"{s.WindDirection}," +
                            $"{s.NacellePosition}," +
                            $"{s.PowerKW}," +
                            $"{s.PotentialPowerDefaultKW}," +
                            $"{s.PowerFactor}," +
                            $"{s.ReactivePowerKvar}," +
                            $"{s.GridFrequencyHz}," +
                            $"{s.GeneratorRpm}");

                        // EVENT
                        OnSampleReceived?.Invoke(s);

                        // WIND SPIKE
                        if (lastWindSpeed != -1)
                        {
                            double diff =
                                Math.Abs(
                                    s.WindSpeed -
                                    lastWindSpeed);

                            if (diff > windSpikeThreshold)
                            {
                                OnWarningRaised?.Invoke(
                                    $"WIND SPIKE WARNING: {diff}");
                            }
                        }

                        // CUT-IN ANOMALY
                        if (s.WindSpeed < lowWindCutIn &&
                            s.PowerKW > 0)
                        {
                            OnWarningRaised?.Invoke(
                                "CUT-IN ANOMALY WARNING");
                        }

                        // NACELLE JUMP
                        if (lastNacelle != -1)
                        {
                            double nacelleDiff =
                                Math.Abs(
                                    s.NacellePosition -
                                    lastNacelle);

                            if (nacelleDiff >
                                nacelleThreshold)
                            {
                                OnWarningRaised?.Invoke(
                                    "NACELLE WARNING");
                            }
                        }

                        // POWER DROP
                        if (lastPower != -1 &&
                            lastPower != 0)
                        {
                            double drop =
                                ((lastPower -
                                  s.PowerKW)
                                  / lastPower) * 100;

                            if (drop >
                                powerDropThreshold)
                            {
                                OnWarningRaised?.Invoke(
                                    $"POWER DROP WARNING: {drop}%");
                            }
                        }

                        lastWindSpeed = s.WindSpeed;
                        lastNacelle = s.NacellePosition;
                        lastPower = s.PowerKW;
                    }
                    catch (Exception ex)
                    {
                        rejectsWriter.WriteLine(
                            $"ERROR: {ex.Message}");
                    }
                }

                sessionWriter.Flush();
                rejectsWriter.Flush();

                Console.WriteLine(
                    $"Batch received: {samples.Count}");

                OnBatchReceived?.Invoke(samples.Count);

                return
                    $"ACK: {samples.Count} samples received";
            }
            catch (Exception ex)
            {
                throw new FaultException(
                    $"PushSample error: {ex.Message}");
            }
        }

        public void EndSession()
        {
            try
            {
                sessionWriter?.Flush();
                rejectsWriter?.Flush();

                sessionWriter?.Close();
                rejectsWriter?.Close();

                Console.WriteLine(
                    "Transfer completed");

                OnTransferCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                throw new FaultException(
                    $"EndSession error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            sessionWriter?.Dispose();
            rejectsWriter?.Dispose();
        }
    }
}