using SRVEmu.DirtySocks.Messages;

namespace SRVEmu.DirtySocks.Model
{
    public class RoomUserCollection : UserCollection
    {
        public Room Room;

        public RoomUserCollection(Room parent)
        {
            Room = parent;
        }

        public override bool AddUser(User user)
        {
            lock (Users)
            {
                if (Users.Count >= Room.Max) return false;
                base.AddUser(user);
            }

            //send move to this user
            MoveOut move;
            lock (Users)
            {
                move = new MoveOut()
                {
                    IDENT = Room.ID.ToString(),
                    NAME = Room.Name,
                    COUNT = Users.Count.ToString()
                };
            }
            user.Connection?.SendMessage(move);

            //send who to this user to tell them who they are

            PlusUser info = user.GetInfo();
            PlusWho who = new()
            {
                I = info.I ?? string.Empty,
                N = info.N,
                M = info.M,
                A = info.A ?? string.Empty,
                X = info.X,
                R = Room.Name,
                RI = Room.ID.ToString()
            };

            user.Connection?.SendMessage(who);
            RefreshUser(user);
            ListToUser(user);
            Room.BroadcastPopulation();
            return true;
        }

        public void RefreshUser(User target)
        {
            PlusUser info = target.GetInfo();
            Broadcast(info);
        }

        public void ListToUser(User target)
        {
            List<PlusUser> infos = new();
            lock (Users)
            {
                foreach (var user in Users)
                {
                    infos.Add(user.GetInfo());
                }
            }
            foreach (var info in infos) target.Connection?.SendMessage(info);
        }

        public override void RemoveUser(User user)
        {
            base.RemoveUser(user);
            if (Room.Users != null)
            {
                Broadcast(new PlusUser()
                {
                    I = user.ID.ToString(),
                    T = Room.Users.Count().ToString(),
                    F = null,
                    P = null,
                    S = null
                });
            }
            else
            {
                Broadcast(new PlusUser()
                {
                    I = user.ID.ToString(),
                    T = "0",
                    F = null,
                    P = null,
                    S = null
                });
            }

            Broadcast(new PlusMesg()
            {
                F = "C",
                T = "\"has left the room\"",
                N = user.PersonaName
            });

            Room.BroadcastPopulation();
            Room.RemoveChallenges(user);
        }
    }
}
