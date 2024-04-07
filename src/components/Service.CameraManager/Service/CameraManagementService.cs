using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.CameraManager.Service
{
    public class CameraManagementService
    {
        private string _deviceId;
        private bool isCameraMoving = false;

        public CameraManagementService(string deviceId)
        {
            _deviceId = deviceId;
        }

        public bool LookTo(ITarget target)
        {
            if (isCameraMoving)
            {
                return false;
            }

            //TODO: 依据不同源的命令，进入不同的状态

            return true;
        }


    }
}
