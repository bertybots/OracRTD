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
            timer = new Timer(UpdateTime);
            timer.Change(500, 500);
        }

        private static void UpdateTopic(string name, string value)
        {
            foreach (Topic topic in topics)
            {
                String item;
                if (topicInfos.TryGetValue(topic, out item))
                {
                    if (item.Equals(name))
                    {
                        topic.UpdateValue(value);
                    }
                }
            }
        }
        private static void UpdateTime(object timer)
        {
            UpdateTopic("NOW", GetTime());
        }

        public static void UpdateCurves(string curveName, string value)
        {
            UpdateTopic(curveName, value);
        }

        public static void UpdateConnected(string value)
        {
            UpdateTopic("CONNECTION", value);
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
            return "Querying...";
        }

        protected override void DisconnectData(Topic topic)
        {
            topics.Remove(topic);
            topicInfos.Remove(topic);
        }
    }
}
