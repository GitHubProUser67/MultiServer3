using SRVEmu.Messages;

namespace SRVEmu.Model
{
    public class RoomCollection
    {
        public MatchmakerServer? Server;
        private int RoomID = 1;
        private List<Room> Rooms = new();

        public virtual void AddRoom(Room room)
        {
            if (Server != null)
            {
                lock (Rooms)
                {
                    room.Server = Server;
                    room.ID = RoomID++;
                    Rooms.Add(room);
                }
            }
        }

        public virtual void RemoveRoom(Room room)
        {
            lock (Rooms)
            {
                Rooms.Remove(room);
            }
        }

        public Room? GetRoomByName(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            lock (Rooms)
            {
                return Rooms.FirstOrDefault(x => x.Name == name);
            }
        }

        public int Count()
        {
            lock (Rooms)
            {
                return Rooms.Count;
            }
        }

        public void SendRooms(User user)
        {
            var infos = new List<PlusRom>();
            lock (Rooms)
            {
                foreach (var room in Rooms)
                {
                    infos.Add(room.GetInfo());
                }
            }
            foreach (PlusRom info in infos) user.Connection?.SendMessage(info);
        }
    }
}
