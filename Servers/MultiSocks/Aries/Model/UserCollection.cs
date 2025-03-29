using MultiSocks.Aries.Messages;
using System.Collections.Concurrent;
using System.Threading;

namespace MultiSocks.Aries.Model
{
    public class UserCollection
    {
        private readonly object _QueueLock = new();

        protected ConcurrentDictionary<int, AriesUser> Users = new();
        protected Queue<int> UserIdsQueue = new(); // To maintain insertion order

        public List<AriesUser> GetAll()
        {
            IEnumerable<AriesUser> userQueue;

            lock (_QueueLock)
            {
                userQueue = UserIdsQueue.Select(id => Users[id]);
            }

            return userQueue.ToList();
        }

        public virtual bool AddUser(AriesUser? user, string VERS = "")
        {
            if (user == null)
                return false;

            if (Users.TryAdd(user.ID, user))
            {
                lock (_QueueLock)
                {
                    UserIdsQueue.Enqueue(user.ID);
                }
                return true;
            }

            return false;
        }

        public virtual bool AddUserWithRoomMesg(AriesUser? user, string VERS = "")
        {
            if (user == null)
                return false;

            if (Users.TryAdd(user.ID, user))
            {
                lock (_QueueLock)
                {
                    UserIdsQueue.Enqueue(user.ID);
                }
                return true;
            }

            return false;
        }

        public virtual void RemoveUser(AriesUser? user)
        {
            if (user == null)
                return;

            if (Users.Remove(user.ID, out _))
            {
                lock (_QueueLock)
                {
                    UserIdsQueue = new Queue<int>(UserIdsQueue.Where(id => id != user.ID));
                }
            }
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
