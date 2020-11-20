using System;
using System.Linq;

using ExcelDna.Integration;
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace OracRTD
{
    public static class MyFunctions
    {
        public static bool started = false;
        public static bool connected = false;
        //public static SocketIoClient client = new SocketIoClient();
        public static Socket client;

        public static int inputsCount = 0;
        public static object[,] inputsArray = new object[1, 1];
        private static Dictionary<String, Double> inputsMap = new Dictionary<String, Double>();

        private static readonly object ioLock = new object();

        [ExcelFunction(Description = "Publish Curve Data")]
        public static string OracPublish(string name, double value)
        {
            ConnectToOrac();
            if (true)
            {
                string p = JsonConvert.SerializeObject(new NameValue(name, value));
                client.Emit("publish", p);
                return "Published Name: " + name + ", Value: " + value;
            } else {
                return "Connecting...";
            }
        }

        [ExcelFunction(Description = "Connection Status")]
        public static bool OracStatus(string name)
        {
            ConnectToOrac();
            return connected;
        }

        [ExcelFunction(Description = "Get Time from the Oracle")]
        public static object OracTime()
        {
            var res = XlCall.RTD("OracRTD.OracData", null, "NOW");
            return res;
        }

        [ExcelFunction(Description = "Get Real-Time Curve from the Oracle")]
        public static object OracCurveInputs()
        {
            ConnectToOrac();
            var res = XlCall.RTD("OracRTD.OracData", null, "CURVE");
            return res;
        }

        [ExcelFunction(Description = "Get Real-Time Curve from the Oracle")]
        public static object OracCurveInputsData(string reference)
        {
            return inputsArray;
        }

        [ExcelFunction(Description = "Get Point from Real-Time Curve from the Oracle")]
        public static object OracCurveInputsDatum(string reference, string datum)
        {
            Double d;
            if (inputsMap.TryGetValue(datum, out d))
            {
                return d;
            } else {
                return ExcelDna.Integration.ExcelError.ExcelErrorNA;
            }            
        }

        private static object[,] string2array(String s)
        {
            if (s.EndsWith(";"))
            {
                s = s.Substring(0, s.Length - 1);
            }
            string[][] MultiArray = s.Split(';').Select(t => t.Split(',')).ToArray();
            int dim1 = MultiArray.Length;
            int dim2 = MultiArray.Select(a => a.Length).Max();
            object[,] arr = new object[dim1, dim2];
            for (int i = 0; i < dim1; i++)
            {
                for (int j = 0; j < MultiArray[i].Length; j++)
                {
                    arr[i, j] = MultiArray[i][j];
                }
            }
            return arr;
        }

        private static void ConnectToOrac()
        {
            if (started)
            {
                return;
            }
            started = true;

            //client = IO.Socket("http://localhost:8200");
            client = IO.Socket("http://orac.uksouth.cloudapp.azure.com");

            client.On(Socket.EVENT_CONNECT, () =>
            {
                connected = true;
            });

            client.On("curve", json =>
            {
                Console.WriteLine($"Got Curve: \"{json}\"");
                dynamic tokens = JsonConvert.DeserializeObject(json.ToString());

                int count = 0;
                foreach (dynamic token in tokens)
                {
                    count++;
                }

                object[,] resultArray = new object[count, 2];
                Dictionary<String, Double> resultMap = new Dictionary<String, Double>();

                int c = 0;
                foreach (dynamic token in tokens)
                {
                    Double v = Convert.ToDouble(token.Value.Value);
                    Console.WriteLine(token.Name, v);
                    resultArray[c, 0] = token.Name;
                    resultArray[c, 1] = v;
                    resultMap.Add(token.Name, v);
                    c++;
                }

                inputsArray = resultArray;
                inputsMap = resultMap;

                OracData.UpdateCurves("Curve." + inputsCount++);
            });
        }

//        private static async Task ConnectToOrac()
//        {
//            client.Connected += (sender, args) =>
//            {
//                connected = true;
//                Console.WriteLine($"Connected: {args.Namespace}");
//            };
//            client.Disconnected += (sender, args) =>
//            {
//                Console.WriteLine($"Disconnected. Reason: {args.Reason}, Status: {args.Status:G}");
//            };
//            client.EventReceived += (sender, args) =>
//            {
//                Console.WriteLine($"EventReceived: Namespace: {args.Namespace}, Value: {args.Value}, IsHandled: {args.IsHandled}");
//            };
//            client.HandledEventReceived += (sender, args) =>
//            {
//                Console.WriteLine($"HandledEventReceived: Namespace: {args.Namespace}, Value: {args.Value}");
//            };
//            client.UnhandledEventReceived += (sender, args) =>
//            {
//                Console.WriteLine($"UnhandledEventReceived: Namespace: {args.Namespace}, Value: {args.Value}");
//            };
//            client.ErrorReceived += (sender, args) =>
//            {
//                Console.WriteLine($"ErrorReceived: Namespace: {args.Namespace}, Value: {args.Value}");
//            };
//            client.ExceptionOccurred += (sender, args) =>
//            {
//                Console.WriteLine($"ExceptionOccurred: {args.Value}");
//            };
//
//            client.On("curve", json =>
//            {
//                Console.WriteLine($"Got Curve: \"{json}\"");
//                dynamic tokens = JsonConvert.DeserializeObject(json);
// 
//                int count = 0;
//                foreach (dynamic token in tokens)
//                {
//                    count++;
//                }
//                object[,] result = new object[count, 2];
//                int c = 0;
//                foreach (dynamic token in tokens)
//                {
//                    Console.WriteLine(token.Name, token.Value.Value);
//                    result[c, 0] = token.Name;
//                    result[c, 1] = Convert.ToDouble(token.Value.Value);
//                    c++;
//                }
//                
//                inputs = result;
//
//                OracData.UpdateCurves("Curve." + inputsCount++);
//            });
//
//            await client.ConnectAsync(new Uri("ws://localhost:8200/"));
//        }

    }
}