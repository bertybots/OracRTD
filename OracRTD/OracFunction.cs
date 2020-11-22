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
        //static readonly string ioServer = "http://localhost:8200";
        //static readonly string ioServer = "http://orac.uksouth.cloudapp.azure.com";

        public static bool connected = false;

        public static string ioServer = "Not Provided";

        public static Socket client;
        public static Dictionary<String, Curve> curves = new Dictionary<string, Curve>();

        private static readonly object ioLock = new object();

        [ExcelFunction(Description = "Publish Curve Data")]
        public static string OracPublish(string server, string name, double value, string username)
        {
            if (connected)
            {
                string p = JsonConvert.SerializeObject(new NameValue(name, value, username, 0));
                try
                {
                    client.Emit("publish", p);
                    return "Published Name: " + name + ", Value: " + value;
                } 
                catch (Exception e)
                {
                    return "Error Publishing Value: " + e.ToString();
                }
            } else {
                return "Not Connected";
            }
        }

        [ExcelFunction(Description = "Connection Status")]
        public static object OracConnect(string server)
        {
            var res = XlCall.RTD("OracRTD.OracData", null, "CONNECTION");
            string error = Connect(server);
            if (error != null)
            {
                return error;
            }
            else
            {
                return res;
            }
        }

        [ExcelFunction(Description = "Get Time")]
        public static object OracTime()
        {
            var res = XlCall.RTD("OracRTD.OracData", null, "NOW");
            return res;
        }

        [ExcelFunction(Description = "Get Real-Time Curve Handle")]
        public static object OracCurve(string server, string curveName)
        {
            if (curveName.Trim().Length == 0)
            {
                return "#Invalid Curve Name";
            }
            Curve curve;
            if (!curves.TryGetValue("CURVE." + curveName, out curve))
            {
                curve = new Curve("CURVE." + curveName);
                curves.Add(curve.name, curve);
            }
            var res = XlCall.RTD("OracRTD.OracData", null, curve.name);
            return res;
        }

        [ExcelFunction(Description = "Get Real-Time for All Curve Points")]
        public static object OracCurveAllValues(string curveName)
        {
            int pos = curveName.LastIndexOf(".");
            string key = pos < 0 ? curveName : curveName.Substring(0, pos);
            Curve curve;
            if (curves.TryGetValue(key, out curve)) {
                return curve.array;
            }
            return ExcelDna.Integration.ExcelError.ExcelErrorNA;
        }

        [ExcelFunction(Description = "Get Real-Time for Specific Curve Point")]
        public static object OracCurvePoint(string curveName, string record, string field)
        {
            int pos = curveName.LastIndexOf(".");
            string key = pos < 0 ? curveName : curveName.Substring(0, pos);
            Curve curve;
            if (curves.TryGetValue(key, out curve)) {
                NameValue nv;
                if (curve.map.TryGetValue(record, out nv))
                {
                    switch (field.ToLower()) {
                        case "name": return nv.name;
                        case "value": return nv.value;
                        case "username": return nv.username;
                        case "date": return nv.excelDate;
                    }
                }
            }
            return ExcelDna.Integration.ExcelError.ExcelErrorNA;
        }

        private static void processCurve(string curveName, object json)
        {
            try
            {
                Console.WriteLine($"Got Curve Inputs: \"{json}\"");
                dynamic tokens = JsonConvert.DeserializeObject(json.ToString());

                int count = 0;
                foreach (dynamic token in tokens)
                {
                    count++;
                }

                object[,] resultArray = new object[count, 4];
                Dictionary<String, NameValue> resultMap = new Dictionary<String, NameValue>();

                int c = 0;
                foreach (dynamic token in tokens)
                {
                    string name = token.Name;
                    Double value = Convert.ToDouble(token.Value.value);
                    Double excelDate = Convert.ToDouble(token.Value.excelDate);
                    string username = token.Value.username;

                    resultArray[c, 0] = name;
                    resultArray[c, 1] = value;
                    resultArray[c, 2] = excelDate;
                    resultArray[c, 3] = username;
                    resultMap.Add(token.Name, new NameValue(name, value, username, excelDate));
                    c++;
                }

                Curve curve;
                if (curves.TryGetValue(curveName, out curve))
                {
                    curve.array = resultArray;
                    curve.map = resultMap;
                    curve.updateCount++;
                    OracData.UpdateCurves(curve.name, curve.name + "." + curve.updateCount);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Processing Curve: " + ex.ToString());
            }
        }

        private static string Connect(string server)
        {
            if (ioServer.Equals(server))
            {
                return null;
            }

            ioServer = server;

            try
            {
                client = IO.Socket(server);
            }
            catch (Exception ex)
            {
                connected = false;
                return ex.ToString();
            }

            client.On(Socket.EVENT_CONNECT, () =>
            {
                connected = true;
                OracData.UpdateConnected(server);
                client.Emit("listen", "curveInputs");
                client.Emit("listen", "cooked");
            });

            client.On(Socket.EVENT_DISCONNECT, () =>
            {
                connected = false;
                OracData.UpdateConnected("Disconnected");
            });

            client.On("cooked", json =>
            {
                processCurve("CURVE.cooked", json);
            });

            client.On("curveInputs", json =>
            {
                processCurve("CURVE.inputs", json);
            });

            return null;
        }
    }
}