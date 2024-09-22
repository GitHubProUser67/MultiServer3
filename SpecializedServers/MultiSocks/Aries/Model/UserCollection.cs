using MultiSocks.Aries.Messages;
using System.Collections.Concurrent;

namespace MultiSocks.Aries.Model
{
    public class UserCollection
    {
        protected ConcurrentDictionary<int, AriesUser> Users = new();
        protected Queue<int> UserIdsQueue = new(); // To maintain insertion order

        public List<AriesUser> GetAll()
        {
            return UserIdsQueue.Select(id => Users[id]).ToList();
        }

        public virtual bool AddUser(AriesUser? user, string VERS = "")
        {
            if (user == null)
                return false;

            lock (Users)
            {
                Users.TryAdd(user.ID, user);
                UserIdsQueue.Enqueue(user.ID);
            }

            return true;
        }

        public virtual bool AddUserWithRoomMesg(AriesUser? user, string VERS = "")
        {
            if (user == null)
                return false;

            lock (Users)
            {
                Users.TryAdd(user.ID, user);
                UserIdsQueue.Enqueue(user.ID);
            }

            return true;
        }

        public virtual void RemoveUser(AriesUser? user)
        {
            if (user == null)
                return;

            if (Users.ContainsKey(user.ID))
            {
                Users.Remove(user.ID, out _);
                UserIdsQueue = new Queue<int>(UserIdsQueue.Where(id => id != user.ID));
            }
        }

        public virtual void UpdateUser(AriesUser user, AriesUser updatedUser)
        {
            if (Users.ContainsKey(user.ID))
                Users[user.ID] = updatedUser;
        }

        public AriesUser? GetUserByName(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return Users.FirstOrDefault(x => x.Value.Username == name).Value;
        }

        public AriesUser? GetUserByPersonaName(string name)
        {
            return Users.FirstOrDefault(x => x.Value.PersonaName == name).Value;
        }

        public int Count()
        {
            return Users.Count;
        }

        public void Broadcast(AbstractMessage msg)
        {
            byte[] data = msg.GetData();
            foreach (var user in Users)
            {
                user.Value.Connection?.SendMessage(data);
            }
        }
    }
}
