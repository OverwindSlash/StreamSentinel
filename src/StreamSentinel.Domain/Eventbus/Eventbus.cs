using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamSentinel.Eventbus
{
    public class EventBus
    {
        private static EventBus instance = null;
        private static readonly object padlock = new object();

        private EventBus()
        {
            // 初始化 EventBus
        }

        public static EventBus Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new EventBus();
                    }
                    return instance;
                }
            }
        }

        private ConcurrentDictionary<string, List<Action<NotificationEventArgs>>> subscriptions = new ConcurrentDictionary<string, List<Action<NotificationEventArgs>>>();

        public void Subscribe(string pipelineName, Action<NotificationEventArgs> handler)
        {
            subscriptions.GetOrAdd(pipelineName, new List<Action<NotificationEventArgs>>()).Add(handler);
        }

        public void PublishNotification(NotificationEventArgs e)
        {
            foreach (var subscription in subscriptions)
            {
                if (/*subscription.Key != e.SenderPipeline && */subscription.Key == e.TargetPipeline)
                {
                    foreach (var handler in subscription.Value)
                    {
                        handler(e);
                    }
                }
            }
        }
    }
}
