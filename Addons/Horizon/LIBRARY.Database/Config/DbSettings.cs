namespace MultiServer.Addons.Horizon.LIBRARY.Database.Config
{
    public class DbSettings
    {
        /// <summary>
        /// When true, the controller will simulate a database.
        /// Data is not persistent in simulated mode.
        /// </summary>
        public bool SimulatedMode { get; set; } = true;

        /// <summary>
        /// Database url.
        /// </summary>
        public string DatabaseUrl { get; set; } = null;

        /// <summary>
        /// Database username.
        /// </summary>
        public string DatabaseUsername { get; set; } = null;

        /// <summary>
        /// Database password.
        /// </summary>
        public string DatabasePassword { get; set; } = null;
    }
}
