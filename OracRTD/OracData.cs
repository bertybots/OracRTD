using System;
using System.Collections.Generic;
using System.Threading;

using ExcelDna.Integration.Rtd;


namespace OracRTD
{
    class OracData : ExcelRtdServer
    {
        private static readonly List<Topic> topics = new List<Topic>();
        private static readonly Dictionary<Topic, string> topicInfos = new Dictionary<Topic, string>();
        private static Timer timer;

        public OracData()
        {
            timer = new Timer(Callback);
        }

        public static void UpdateCurves(string curveData)
        {
            foreach (Topic topic in topics)
            {
                String name;
                if (topicInfos.TryGetValue(topic, out name))
                {
                    if (name.Equals("CURVE"))
                    {
                        topic.UpdateValue(curveData);
                    }
                }
            }
        }

        private static void Start()
        {
            timer.Change(500, 500);
        }

        private static void Stop()
        {
            timer.Change(-1, -1);
        }

        private static void Callback(object o)
        {
            Stop();
            foreach (Topic topic in topics)
            {
                String name;
                if (topicInfos.TryGetValue(topic, out name))
                {
                    if (name.Equals("NOW"))
                    {
                        topic.UpdateValue(GetTime());
                    }
                }
            }
            Start();
        }

        private static string GetTime()
        {
            return DateTime.Now.ToString("HH:mm:ss.fff");
        }

        protected override void ServerTerminate()
        {
            timer.Dispose();
            timer = null;
        }

        protected override object ConnectData(Topic topic, IList<string> topicInfo, ref bool newValues)
        {
            topics.Add(topic);
            topicInfos.Add(topic, topicInfo[0]);
            Start();
            return "Querying...";
        }

        protected override void DisconnectData(Topic topic)
        {
            topics.Remove(topic);
            topicInfos.Remove(topic);
            if (topics.Count == 0)
                Stop();
        }
    }
}
