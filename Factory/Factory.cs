using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory
{
    internal class Factory
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly Guid _id;

        internal Factory()
        {
            _id = Guid.Parse(Environment.GetEnvironmentVariable("FACTORY_ID"));
        }
    }
}
