using CyberBackendLibrary.Extension;
using System;
using System.Linq;
using System.Text;

namespace CyberBackendLibrary.HTTP
{
    public class UniqueIDStore : IDisposable
    {
        private bool disposedValue;
        private ConcurrentList<string> uniqueIDs = null;

        public UniqueIDStore()
        {
            uniqueIDs = new ConcurrentList<string>();
        }

        public string GetOrCreate(string uniqueID = null)
        {
            if (!string.IsNullOrEmpty(uniqueID) && uniqueIDs.Contains(uniqueID))
                return uniqueID;

            StringBuilder builder = new StringBuilder();

            Enumerable
               .Range(65, 26)
                .Select(e => ((char)e).ToString())
                .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
                .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
                .OrderBy(e => Guid.NewGuid())
                .Take(11)
                .ToList().ForEach(e => builder.Append(e));

            uniqueID = builder.ToString();

            if (!uniqueIDs.Contains(uniqueID))
                uniqueIDs.Add(uniqueID);

            return uniqueID;
        }

        public void TryRemoveID(string uniqueID)
        {
            if (uniqueIDs.Contains(uniqueID))
                uniqueIDs.Remove(uniqueID);
        }

        public void Clear()
        {
            uniqueIDs.Clear();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    uniqueIDs = null;

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~UniqueIDStore()
        // {
        //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
