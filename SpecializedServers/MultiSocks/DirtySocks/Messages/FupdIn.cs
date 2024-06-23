namespace MultiSocks.DirtySocks.Messages
{
    public class FupdIn : AbstractMessage
    {
        public override string _Name { get => "fupd"; }
        public string? TAG { get; set; }
        public string? ADD { get; set; }
        public string? DELETE { get; set; }
        public string? SET { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
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
                                DirtySocksServer.Database.AddFriend(client.User.ID, SplitedUser);
                            }
                        }
                        if (!string.IsNullOrEmpty(DELETE))
                        {
                            string[] SplitedList = DELETE.Split(",");

                            foreach (string SplitedUser in SplitedList)
                            {
                                DirtySocksServer.Database.DeleteFriend(client.User.ID, SplitedUser);
                            }
                        }
                        break;
                    case "R":
                        if (!string.IsNullOrEmpty(ADD))
                        {
                            string[] SplitedList = ADD.Split(",");

                            foreach (string SplitedUser in SplitedList)
                            {
                                DirtySocksServer.Database.AddRival(client.User.ID, SplitedUser);
                            }
                        }
                        if (!string.IsNullOrEmpty(DELETE))
                        {
                            string[] SplitedList = DELETE.Split(",");

                            foreach (string SplitedUser in SplitedList)
                            {
                                DirtySocksServer.Database.DeleteRival(client.User.ID, SplitedUser);
                            }
                        }
                        break;
                }
            }

            client.SendMessage(new FupdOut());
        }
    }
}
