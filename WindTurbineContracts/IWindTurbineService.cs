using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace WindTurbineContracts
{
    [ServiceContract]
    public interface IWindTurbineService
    {
        [OperationContract]
        void StartSession(string turbineId);

        [OperationContract]
        string PushSample(List<WindTurbineSample> samples);

        [OperationContract]
        void EndSession();
    }
}