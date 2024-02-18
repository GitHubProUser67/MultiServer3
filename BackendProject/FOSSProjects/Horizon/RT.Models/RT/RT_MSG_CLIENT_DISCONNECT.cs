using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.LIBRARY.Common.Stream;

namespace BackendProject.Horizon.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_CLIENT_DISCONNECT)]
    public class RT_MSG_CLIENT_DISCONNECT : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_CLIENT_DISCONNECT;

        public override void Deserialize(MessageReader reader)
        {

        }

        public override void Serialize(MessageWriter writer)
        {

        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}