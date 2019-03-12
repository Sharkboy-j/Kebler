using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transmission.API.RPC
{
    public static class AsyncExtensions
    {
        public static void WaitAndUnwrapException(this Task task)
        {
            try
            {
                task.Wait();
            } catch(Exception e)
            {
                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }

                throw e;
            }
        }
    }
}
