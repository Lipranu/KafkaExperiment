using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public static class MySuperConsistentPartitionSelectionAlgorithm
    {
        private readonly static Guid factory0 = new("dd789adc-d242-44e6-a36b-b93f1e3a7908");
        //private readonly static Guid factory1 = new("");
        private readonly static Guid factory2 = new("85a7f627-e120-4289-b7eb-8f981e16cdf8");
        private readonly static Guid factory3 = new("ad1c3084-2523-442c-ac9f-b7376cd16d42");
        //private readonly static Guid factory4 = new("");

        public static int SelectPartition(Guid id)
        {
            if (id == factory0) return 0;
            //if (id == factory1) return 1;
            if (id == factory2) return 2;
            if (id == factory3) return 3;
            //if (id == factory4) return 4;
            return 5;
        }
    }
}
