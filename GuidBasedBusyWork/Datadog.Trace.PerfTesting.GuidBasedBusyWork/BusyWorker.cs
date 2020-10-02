using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace Datadog.Trace.PerfTesting.GuidBasedBusyWork
{
    public class BusyWorker
    {
        public async Task<BusyWorkResult> DoUselessStuff()
        {
            Tuple<Guid, Guid, Guid, Guid> guids = await GetGuidsAsync();

            BusyWorkResult result = ProcessGuilds(guids.Item1, guids.Item2, guids.Item3, guids.Item4);
            return result;
        }

        private async Task<Tuple<Guid, Guid, Guid, Guid>> GetGuidsAsync()
        {
            

            using (HttpClient client = new HttpClient())
            {
                // Get some in parallel, and some sequentially:
                Task<Guid> guid1Task = GetGuidAsync(client);
                Task<Guid> guid2Task = GetGuidAsync(client);
                Task<Guid> guid3Task = GetGuidAsync(client);

                await Task.WhenAll(guid1Task, guid2Task, guid3Task);

                Task<Guid> guid4Task = GetGuidAsync(client);

                return Tuple.Create(await guid1Task, await guid2Task, await guid3Task, await guid4Task);
            }

        }

        private async Task<Guid> GetGuidAsync(HttpClient client)
        {
            // Fetch guids remotely, or debug with a local mock:
            const string GuidServiceUrl = "to-do";

            if (String.IsNullOrEmpty(GuidServiceUrl) || GuidServiceUrl.Equals("to-do", StringComparison.OrdinalIgnoreCase))
            {
                return Guid.NewGuid();
            }
            else
            {
                using (HttpResponseMessage response = await client.GetAsync(GuidServiceUrl))
                {
                    string payload = await response.Content.ReadAsStringAsync();
                    
                    if(payload != null)
                    {
                        payload = payload.Trim();
                        while(payload.StartsWith("\"") && payload.EndsWith("\""))
                        {
                            payload = payload.Substring(1, payload.Length - 2);
                        }
                    }

                    Guid guid = Guid.Parse(payload);
                    return guid;
                }
            }
        }

        private BusyWorkResult ProcessGuilds(Guid guid1, Guid guid2, Guid guid3, Guid guid4)
        {
            string allGuids = String.Empty;

            allGuids = AddToString(allGuids, guid1);
            allGuids = AddToString(allGuids, guid2);
            allGuids = AddToString(allGuids, guid3);
            allGuids = AddToString(allGuids, guid4);

            var result = new BusyWorkResult()
            {
                InputGuids = new string[] { guid1.ToString("D"), guid2.ToString("D"), guid3.ToString("D"), guid4.ToString("D") },
                AllGuidsAsBytes = allGuids,
                Timestamp = DateTimeOffset.Now.ToString("O"),
            };

            Guid roundTrippedGuid1 = ExtractFromString(ref allGuids);
            Guid roundTrippedGuid2 = ExtractFromString(ref allGuids);
            Guid roundTrippedGuid3 = ExtractFromString(ref allGuids);
            Guid roundTrippedGuid4 = ExtractFromString(ref allGuids);

            result.RoundTrippedGuids = new string[] { roundTrippedGuid1.ToString("D"), roundTrippedGuid2.ToString("D"), roundTrippedGuid3.ToString("D"), roundTrippedGuid4.ToString("D") };

            return result;
        }
        private string AddToString(string allGuids, Guid guid)
        {
            byte[] arr = guid.ToByteArray();
            for (int i = 0; i < arr.Length; i++)
            {
                if (allGuids.Length > 0)
                {
                    allGuids += "-";
                }

                allGuids += arr[i].ToString("X2");
            }

            return allGuids;
        }

        private Guid ExtractFromString(ref string allGuids)
        {
            byte[] buff = new byte[16];
            for (int i = 0; i < buff.Length; i++)
            {
                if (allGuids.StartsWith("-"))
                {
                    allGuids = allGuids.Substring(1);
                }

                string numStr = allGuids.Substring(0, 2);
                allGuids = allGuids.Substring(2);

                buff[i] = Byte.Parse(numStr, NumberStyles.HexNumber);
            }

            return new Guid(buff);
        }
    }
}
