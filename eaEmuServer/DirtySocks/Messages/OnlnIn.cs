namespace SRVEmu.DirtySocks.Messages
{
    class OnlnIn : AbstractMessage
    {
        public override string _Name { get => "onln"; }

        public string? PERS { get; set; }
        public string ROOM { get; set; } = "Room-A";

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            Model.User? user = client.User;
            if (user == null) return;

            Model.Room? Room = user.CurrentRoom;

            PlusUser info = user.GetInfo();

            client.SendMessage(info);
            client.SendMessage(this);
            //client.SendMessage(new OnlnImst());
        }
    }

    class OnlnImst : AbstractMessage
    {
        public override string _Name { get => "onlnimst"; }
    }
}
