﻿namespace CavemanTcp
{
    using System;

    /// <summary>
    /// Event arguments for when an exception is encountered. 
    /// </summary>
    public class ExceptionEventArgs
    {
        internal ExceptionEventArgs(Exception e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            Exception = e;
        }

        /// <summary>
        /// Exception.
        /// </summary>
        public Exception Exception { get; } = null;
    }
}