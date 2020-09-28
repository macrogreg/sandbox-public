using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace ActivityListeningTest01
{
    public class ActivityContextCreation
    {
        public void Exec()
        {
            Console.WriteLine();

            { 
                ulong ddTraceId = 0xFFEEDDCCBBAA9988;

                Console.WriteLine($"ddTraceId = {ddTraceId} = 0x{ddTraceId.ToString("X")}");
                Console.WriteLine();

                ReadOnlySpan<byte> buffer = stackalloc byte[16] { 0, 0, 0, 0, 0, 0, 0, 0,
                                                                (byte) (ddTraceId >> 56),
                                                                (byte) (ddTraceId >> 48),
                                                                (byte) (ddTraceId >> 40),
                                                                (byte) (ddTraceId >> 32),
                                                                (byte) (ddTraceId >> 24),
                                                                (byte) (ddTraceId >> 16), 
                                                                (byte) (ddTraceId >> 8), 
                                                                (byte) (ddTraceId) };

                Console.WriteLine("Buffer:");
                for (int i = 0; i < buffer.Length; i++)
                {
                    Console.Write(buffer[i].ToString("X"));
                    Console.Write(" ");
                }
            
                ActivityTraceId activityTraceId = ActivityTraceId.CreateFromBytes(buffer);
                string activityTraceIdStr = activityTraceId.ToHexString();

                Console.WriteLine();
                Console.WriteLine($"activityTraceIdStr=\"{activityTraceIdStr}\".");
            }

            Console.WriteLine();
            Console.WriteLine();

            {
                ulong ddSpanId = 0xFFEEDDCCBBAA9988;
                Console.WriteLine($"ddSpanId = {ddSpanId} = 0x{ddSpanId.ToString("X")}");
                Console.WriteLine();

                ReadOnlySpan<byte> buffer = stackalloc byte[8] { (byte) (ddSpanId >> 56),
                                                                 (byte) (ddSpanId >> 48),
                                                                 (byte) (ddSpanId >> 40),
                                                                 (byte) (ddSpanId >> 32),
                                                                 (byte) (ddSpanId >> 24),
                                                                 (byte) (ddSpanId >> 16), 
                                                                 (byte) (ddSpanId >> 8), 
                                                                 (byte) (ddSpanId) };

                Console.WriteLine("Buffer:");
                for (int i = 0; i < buffer.Length; i++)
                {
                    Console.Write(buffer[i].ToString("X"));
                    Console.Write(" ");
                }
            
                ActivitySpanId activitySpanId = ActivitySpanId.CreateFromBytes(buffer);
                string activitySpanIdStr = activitySpanId.ToHexString();

                Console.WriteLine();
                Console.WriteLine($"activitySpanIdStr=\"{activitySpanIdStr}\".");
            }

            Console.WriteLine();

            TestParseUInt64FromHex(null);

            TestParseUInt64FromHex("");

            TestParseUInt64FromHex("aBc");

            TestParseUInt64FromHex("FFEEDDCCBBAA9988");

            TestParseUInt64FromHex("0000000000000000");

            TestParseUInt64FromHex("ffffffffffffffff");

            TestParseUInt64FromHex("10000000000000001");

            TestParseUInt64FromHex("1111111x11111111");

            Console.WriteLine();
            Console.WriteLine("-----------");

            TestCreateW3CId_Datadog(datadogTraceId: 0, datadogSpanId: 0, isSampled: false);
            TestCreateActivityContext_Datadog(datadogTraceId: 0, datadogSpanId: 0, isSampled: false);
            TestCreateActivityContext(traceId: null, spanId: null, isSampled: false);
            Console.WriteLine("---");
            Console.WriteLine();

            TestCreateW3CId_Datadog(datadogTraceId: 1, datadogSpanId: 1, isSampled: false);
            TestCreateActivityContext_Datadog(datadogTraceId: 1, datadogSpanId: 1, isSampled: false);
            TestCreateActivityContext(traceId: "1", spanId: "1", isSampled: false);
            Console.WriteLine("---");
            Console.WriteLine();

            TestCreateW3CId_Datadog(datadogTraceId: 1, datadogSpanId: 1, isSampled: true);
            TestCreateActivityContext_Datadog(datadogTraceId: 1, datadogSpanId: 1, isSampled: true);
            TestCreateActivityContext(traceId: "1", spanId: "1", isSampled: true);
            Console.WriteLine("---");
            Console.WriteLine();

            TestCreateW3CId_Datadog(datadogTraceId: 1, datadogSpanId: UInt64.MaxValue, isSampled: true);
            TestCreateActivityContext_Datadog(datadogTraceId: 1, datadogSpanId: UInt64.MaxValue, isSampled: true);
            TestCreateActivityContext(traceId: "1", spanId: "ffffffffffffffff", isSampled: true);
            Console.WriteLine("---");
            Console.WriteLine();

            TestCreateW3CId_Datadog(datadogTraceId: UInt64.MaxValue, datadogSpanId: 0x1000000000000000, isSampled: true);
            TestCreateActivityContext_Datadog(datadogTraceId: UInt64.MaxValue, datadogSpanId: 0x1000000000000000, isSampled: true);
            TestCreateActivityContext(traceId: "ffffffffffffffff", spanId: "1000000000000000", isSampled: true);
            Console.WriteLine("---");
            Console.WriteLine();

            TestCreateActivityContext(traceId: "fffffffffffffffx", spanId: "1000000000000000", isSampled: true);
            Console.WriteLine("---");
            Console.WriteLine();

            TestCreateActivityContext(traceId: "ffffffffffffffff", spanId: "10000000000000001", isSampled: true);
            Console.WriteLine("---");
            Console.WriteLine();

            TestCreateActivityContext(traceId: "10000000000000001", spanId: "ffffffffffffffff", isSampled: true);
            Console.WriteLine("---");
            Console.WriteLine();

            TestCreateActivityContext(traceId: "ffffffffffffffffffffffffffffffff", spanId: "ffffffffffffffff", isSampled: true);
            Console.WriteLine("---");
            Console.WriteLine();

            TestCreateActivityContext(traceId: "ffffffffffffffffffffffffffffffff1", spanId: "ffffffffffffffff", isSampled: true);
            Console.WriteLine("---");
            Console.WriteLine();

            TestCreateActivityContext(traceId: "", spanId: "ffffffffffffffff", isSampled: true);
            Console.WriteLine("---");
            Console.WriteLine();


            Console.WriteLine();
            Console.WriteLine("Finished. Press Enter.");
            Console.ReadLine();
        }

        public void TestCreateActivityContext(string traceId, string spanId, bool isSampled)
        {
            Console.WriteLine();
            Console.WriteLine($"Test DatadogCreateActivityContext(traceId: {traceId}, spanId: {spanId}, bool isSampled: {isSampled}):");
            try
            {
                ActivityContext aCtx = CreateActivityContext(traceId, spanId, isSampled);
                Console.WriteLine($"aCtx.TraceId=\"{aCtx.TraceId}\", aCtx.SpanId=\"{aCtx.SpanId}\", aCtx.TraceFlags=\"{aCtx.TraceFlags}\", aCtx.IsRemote=\"{aCtx.IsRemote}\"");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void TestCreateActivityContext_Datadog(ulong datadogTraceId, ulong datadogSpanId, bool isSampled)
        {
            Console.WriteLine();
            Console.WriteLine($"Test DatadogIdToActivityContext(datadogTraceId: {datadogTraceId}, datadogSpanId: {datadogSpanId}, bool isSampled: {isSampled}):");
            try
            {
                ActivityContext aCtx = CreateActivityContext_Datadog(datadogTraceId, datadogSpanId, isSampled);
                Console.WriteLine($"aCtx.TraceId=\"{aCtx.TraceId}\", aCtx.SpanId=\"{aCtx.SpanId}\", aCtx.TraceFlags=\"{aCtx.TraceFlags}\", aCtx.IsRemote=\"{aCtx.IsRemote}\"");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void TestCreateW3CId_Datadog(ulong datadogTraceId, ulong datadogSpanId, bool isSampled)
        {
            Console.WriteLine();
            Console.WriteLine($"Test DatadogIdToW3C(datadogTraceId: {datadogTraceId}, datadogSpanId: {datadogSpanId}, bool isSampled: {isSampled}):");
            try
            {
                string result = CreateW3CId_Datadog(datadogTraceId, datadogSpanId, isSampled);
                Console.WriteLine($"result=\"{result ?? "<null>"}\"");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public ActivityContext CreateActivityContext(string traceId, string spanId, bool isSampled, string traceState = null)
        {
            string w3cId = CreateW3CId(traceId, spanId, isSampled);
            if (w3cId == null)
            {
                return default(ActivityContext);
            }

            ActivityContext aCtx = ActivityContext.Parse(w3cId, traceState);
            return aCtx;
        }

        public ActivityContext CreateActivityContext(ulong traceId, ulong spanId, bool isSampled, string traceState = null)
        {
            string w3cId = CreateW3CId_Datadog(traceId, spanId, isSampled);
            if (w3cId == null)
            {
                return default(ActivityContext);
            }

            ActivityContext aCtx = ActivityContext.Parse(w3cId, traceState);
            return aCtx;
        }

        public ActivityContext CreateActivityContext_Datadog(ulong datadogTraceId, ulong datadogSpanId, bool isSampled, string traceState = null)
        {
            string w3cId = CreateW3CId_Datadog(datadogTraceId, datadogSpanId, isSampled);
            if (w3cId == null)
            {
                return default(ActivityContext);
            }

            ActivityContext aCtx = ActivityContext.Parse(w3cId, traceState);
            return aCtx;
        }

        public string CreateW3CId(string traceId, string spanId, bool isSampled)
        {
            if (isSampled == false && traceId == null && spanId == null)
            {
                return null;
            }

            if (traceId == null)
            {
                throw new ArgumentException($"Either ALL of {nameof(traceId)}, {nameof(spanId)}, {nameof(isSampled)} must"
                                          + $" be not set (i.e. = 0), or {nameof(traceId)} may not be zero.");
            }

            if (spanId == null)
            {
                throw new ArgumentException($"Either ALL of {nameof(traceId)}, {nameof(spanId)}, {nameof(isSampled)} must"
                                          + $" be not set (i.e. = 0), or {nameof(spanId)} may not be zero.");
            }

            if (traceId.Length < 1 || 32 < traceId.Length)
            {
                throw new ArgumentException($"The specified {nameof(traceId)} was expected to have between 1 and 32 characters,"
                                          + $" but it actually contains {traceId.Length} characters.");
            }

            if (spanId.Length < 1 || 16 < spanId.Length)
            {
                throw new ArgumentException($"The specified {nameof(spanId)} was expected to have between 1 and 16 characters,"
                                          + $" but it actually contains {spanId.Length} characters.");
            }

            return CreateW3CId_Core(traceId, spanId, isSampled);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsLowerHexChar(char c)
        {
            return ('0' <= c && c <= '9') || ('a' <= c && c <= 'f');
        }

        private unsafe string CreateW3CId_Core(string traceId, string spanId, bool isSampled)
        {

            char* template = stackalloc char[55] { '0', '0', '-',
                                                    '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0',
                                                    '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '-', 
                                                    '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '-',
                                                    '0', '0'};

            int tPos = 34;
            for (int sPos = traceId.Length - 1; sPos >= 0; sPos--)
            {
                char c = traceId[sPos];
                if (!IsLowerHexChar(c))
                {
                    throw new ArgumentException($"The specified {nameof(traceId)} was expected to only contain lower-case hex characters;"
                                              + $" however, it contains '{c}' at position {sPos} (specified {nameof(traceId)}=\"{traceId}\").");
                }

                *(template + tPos) = c;
                tPos--;
            }

            tPos = 51;
            for (int sPos = spanId.Length - 1; sPos >= 0; sPos--)
            {
                char c = spanId[sPos];
                if (!IsLowerHexChar(c))
                {
                    throw new ArgumentException($"The specified {nameof(spanId)} was expected to only contain lower-case hex characters;"
                                              + $" however, it contains '{c}' at position {sPos} (specified {nameof(spanId)}=\"{spanId}\").");
                }

                *(template + tPos) = c;
                tPos--;
            }

            if (isSampled)
            {
                *(template + 54) = '1';
            }

            return new String(template, 0, 55);
        }


        public string CreateW3CId(ulong traceId, ulong spanId, bool isSampled)
        {
            string w3cId;
            unsafe
            {
                char* template = stackalloc char[55] { '0', '0', '-',
                                                       '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0',
                                                       '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '-', 
                                                       '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '-',
                                                       '0', '0'};
                w3cId = CreateW3CId_Core(traceId, spanId, isSampled, template);
            }

            return w3cId;
        }

        public string CreateW3CId_Datadog(ulong datadogTraceId, ulong datadogSpanId, bool isSampled)
        {
            string w3cId;
            unsafe
            {
                char* template = stackalloc char[55] { '0', '0', '-',
                                                       'd', 'a', '7', 'a', 'd', '0', '9', '0', '0', '0', '0', '0', '0', '0', '0', '0',
                                                       '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '-', 
                                                       '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '-',
                                                       '0', '0'};
                w3cId = CreateW3CId_Core(datadogTraceId, datadogSpanId, isSampled, template);
            }

            return w3cId;
        }

        private unsafe string CreateW3CId_Core(ulong traceId, ulong spanId, bool isSampled, char* template)
        {
            if (isSampled == false && traceId == 0 && spanId == 0)
            {
                return null;
            }

            if (traceId == 0)
            {
                throw new ArgumentException($"Either ALL of {nameof(traceId)}, {nameof(spanId)}, {nameof(isSampled)} must"
                                          + $" be not set (i.e. = 0), or {nameof(traceId)} may not be zero.");
            }

            if (spanId == 0)
            {
                throw new ArgumentException($"Either ALL of {nameof(traceId)}, {nameof(spanId)}, {nameof(isSampled)} must"
                                          + $" be not set (i.e. = 0), or {nameof(spanId)} may not be zero.");
            }

            const ulong LastCharMask = 0x000000000000000F;
            const int charABase = ((int) 'a') - 10;
            const int char0Base = (int) '0';

            int pos = 34;
            while (traceId > 0)
            {
                int v = (int) (traceId & LastCharMask);
                *(template + pos) = (v < 10) ? (char) (v + char0Base) : (char) (v + charABase);

                pos--;
                traceId >>= 4;
            }

            pos = 51;
            while (spanId > 0)
            {
                int v = (int) (spanId & LastCharMask);
                *(template + pos) = (v < 10) ? (char)(v + char0Base) : (char)(v + charABase);

                pos--;
                spanId >>= 4;
            }

            if (isSampled)
            {
                *(template + 54) = '1';
            }

            return new String(template, 0, 55);
        }


        public void TestParseUInt64FromHex(string str)
        {
            Console.WriteLine();
            Console.WriteLine($"ParseUInt64FromHex(\"{str}\"):");
            try
            {
                ulong val = ParseUInt64FromHex(str);
                Console.WriteLine($"ParseUInt64FromHex(\"{str}\") = {val} = 0x{val.ToString("X")}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public ulong ParseUInt64FromHex(string hexString)
        {
            if (hexString == null)
            {
                throw new ArgumentNullException(nameof(hexString));
            }

            if (hexString.Length < 1 || 16 < hexString.Length)
            {
                throw new ArgumentException($"The specified {nameof(hexString)} was expected to have between 1 and 16 characters,"
                                          + $" but it actually contains {hexString.Length} characters.");
            }

            ulong value = 0;
            int shift = 0;

            for (int i = hexString.Length - 1; i >= 0; i--)
            {
                char c = hexString[i];
                ulong v = 0;

                if ('a' <= c && c <= 'f' )
                {
                    v = 10 + (ulong) (c - 'a');
                }
                else if ('A' <= c && c <= 'F')
                {
                    v = 10 + (ulong) (c - 'A');
                }
                else if ('0' <= c && c <= '9')
                {
                    v = (ulong) (c - '0');
                }
                else
                {
                    throw new ArgumentException($"The specified string was expected to only contain hex characters;"
                                              + $" however, it contains '{c}' at position {i} (specified {nameof(hexString)}=\"{hexString}\").");
                }

                v <<= shift;
                value |= v;

                shift += 4;
            }

            return value;
        }
    }
}
