using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kebler.TransmissionCore
{
    public static class Extensions
    {
        public static async Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest request, CancellationToken ct)
        {
            await using (ct.Register(request.Abort, false))
            {
                var response = await request.GetResponseAsync();
                ct.ThrowIfCancellationRequested();
                return (HttpWebResponse)response;
            }
        }

        public static async Task<Stream> GetResponseStream(this HttpWebResponse webResponse, CancellationToken ct)
        {
            await using (ct.Register(webResponse.Close, false))
            {
                var responseStream = webResponse.GetResponseStream();
                ct.ThrowIfCancellationRequested();
                return responseStream;
            }
        }

        public static async Task<string> ReadToEndAsync(this StreamReader reader, CancellationToken ct)
        {
            await using (ct.Register(reader.Close, false))
            {
                var responseString = await reader.ReadToEndAsync();
                ct.ThrowIfCancellationRequested();
                return responseString;
            }
        }
    }
}
