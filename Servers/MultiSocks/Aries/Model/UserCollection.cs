using MultiSocks.Aries.Messages;
using System.Collections.Concurrent;

namespace MultiSocks.Aries.Model
{
    public class UserCollection
    {
        protected ConcurrentDictionary<int, AriesUser> Users = new();

        public List<AriesUser> GetAll()
        {
            return Users.Values.ToList();
        }

        public virtual bool AddUser(AriesUser? user, string VERS = "")
        {
            if (user == null)
                return false;

            return Users.TryAdd(user.ID, user);
        }

        public virtual bool AddUserWithRoomMesg(AriesUser? user, string VERS = "")
        {
            if (user == null)
                return false;

            return Users.TryAdd(user.ID, user);
        }

        public virtual bool RemoveUser(AriesUser? user)
        {
            if (user == null)
                return false;

            return Users.Remove(user.ID, out _);
        }

        public AriesUser? GetUserByName(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return Users.Values.FirstOrDefault(x => x.Username == name);
        }

        public AriesUser? GetUserByPersonaName(string name)
        {
            return Users.Values.FirstOrDefault(x => x.PersonaName == name);
        }

        public int Count()
        {
            return Users.Count;
        }

        public void Broadcast(AbstractMessage msg)
        {
            foreach (AriesUser user in Users.Values)
            {
                if (user.Connection == null)
                {
                    new Thread(() =>
                    {
                        int retries = 0;
                        while (retries < 5)
                        {
                            if (user.Connection != null)
                            {
                                user.Connection.SendMessage(msg);
                                break;
                            }
                            retries++;
                            Thread.Sleep(500);
                        }
                    }).Start();
                }
                else
                    user.Connection.SendMessage(msg);
            }
        }
    }
}
