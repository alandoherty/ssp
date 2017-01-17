using System;

namespace SimpleService
{
    /// <summary>
    /// A handler entry.
    /// </summary>
    internal struct Handler
    {
        public Visibility Visibility;
        public string Service;
        public Delegate Action;
    }
}
