using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogerent.Services.Interfaces
{
    public interface INetworkService
    {
        void ExecuteScanNetwork(int startIp, int endIp, int myIp);
    }
}
