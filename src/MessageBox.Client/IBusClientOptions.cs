using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox
{
    public interface IBusClientOptions
    {
        TimeSpan DefaultCallTimeout { get; set; }

        int MaxDegreeOfParallelism { get; set; }
    }
}
