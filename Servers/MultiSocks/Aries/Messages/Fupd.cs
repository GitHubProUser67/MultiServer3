namespace MultiSocks.Aries.Messages
{
    public class Fupd : AbstractMessage
    {
        public override string _Name { get => "fupd"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            string? TAG = GetInputCacheValue("TAG");
            string? ADD = GetInputCacheValue("ADD");
            string? DELETE = GetInputCacheValue("DELETE");

            if (!string.IsNullOrEmpty(TAG) && client.User != null)
            {
                switch (TAG)
                {
                    case "F":
                        if (!string.IsNullOrEmpty(ADD))
                        {
                            string[] SplitedList = ADD.Split(",");

                            foreach (string SplitedUser in SplitedList)
                            {
                                AriesServer.Database?.AddFriend(client.User.ID, SplitedUser);
                            }
                        }
                        if (!string.IsNullOrEmpty(DELETE))
                        {
                            string[] SplitedList = DELETE.Split(",");

                            foreach (string SplitedUser in SplitedList)
                            {
                                AriesServer.Database?.DeleteFriend(client.User.ID, SplitedUser);
                            }
                        }
                        break;
                    case "R":
                        if (!string.IsNullOrEmpty(ADD))
                        {
                            string[] SplitedList = ADD.Split(",");

                            foreach (string SplitedUser in SplitedList)
                            {
                                AriesServer.Database?.AddRival(client.User.ID, SplitedUser);
                            }
                        }
                        if (!string.IsNullOrEmpty(DELETE))
                        {
                            string[] SplitedList = DELETE.Split(",");

                            foreach (string SplitedUser in SplitedList)
                            {
                                AriesServer.Database?.DeleteRival(client.User.ID, SplitedUser);
                            }
                        }
                        break;
                }
            }

            client.SendMessage(this);
        }
    }
}
