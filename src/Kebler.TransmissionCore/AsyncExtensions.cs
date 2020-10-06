using System;
using System.Threading.Tasks;

namespace Kebler.TransmissionCore
{
    public static class AsyncExtensions
    {
        public static void WaitAndUnwrapException(this Task task)
        {
            try
            {
                task.Wait();
            }
            catch (Exception e)
            {
                if (e.InnerException != null) throw e.InnerException;

                throw e;
            }
        }
    }
}