using MultiSocks.DirtySocks.Messages;
using System.Collections.Concurrent;

namespace MultiSocks.DirtySocks.Model
{
    public class UserCollection
    {
        protected ConcurrentQueue<User> Users = new(); // This data type follows the FIFO order, necessary for players list.

        public List<User> GetAll()
        {
            lock (Users)
            {
                return Users.ToList();
            }
        }

        public virtual bool AddUser(User? user, string VERS = "")
        {
            if (user == null)
                return false;

            lock (Users)
            {
                Users.Enqueue(user);
            }
            return true;
        }

        public virtual bool AddUserWithRoomMesg(User? user, string VERS = "")
        {
            if (user == null)
                return false;

            lock (Users)
            {
                Users.Enqueue(user);
            }
            return true;
        }

        public virtual void RemoveUser(User user)
        {
            lock (Users)
            {
                // Dequeue and re-enqueue all users except the one to be removed
                Users = new ConcurrentQueue<User>(Users.Where(u => u != user));
            }
        }

        public User? GetUserByName(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            lock (Users)
            {
                return Users.FirstOrDefault(x => x.Username == name);
            }
        }

        public User? GetUserByPersonaName(string name)
        {
            lock (Users)
            {
                return Users.FirstOrDefault(x => x.PersonaName == name);
            }
        }

        public int Count()
        {
            lock (Users)
            {
                return Users.Count;
            }
        }

        public void Broadcast(AbstractMessage msg)
        {
            byte[] data = msg.GetData();
            lock (Users)
            {
                foreach (var user in Users)
                {
                    user.Connection?.SendMessage(data);
                }
            }
        }
    }
}
