using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;

namespace WindTurbineContracts
{
    [ServiceContract]
    public interface IWindTurbineService
    {
        [OperationContract]
        void StartSession(string turbineId);

        [OperationContract]
        void PushSample(List<WindTurbineSample> samples);

        [OperationContract]
        void EndSession();
    }
}