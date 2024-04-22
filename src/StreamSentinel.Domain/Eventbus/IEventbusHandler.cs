using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamSentinel.Eventbus
{
    public interface IEventbusHandler
    {
        public void HandleNotification(NotificationEventArgs e);

    }
}
