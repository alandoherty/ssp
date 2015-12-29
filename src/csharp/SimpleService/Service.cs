using Newtonsoft.Json.Linq;
using System;

namespace SimpleService
{
    /// <summary>
    /// Handles a message.
    /// </summary>
    /// <param name="obj">The object.</param>
    public delegate void ServiceMessageHandler(JObject obj);

    /// <summary>
    /// Handles a request.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns></returns>
    public delegate JObject ServiceRequestHandler(JObject obj);

    public class Service<T>
    {
        #region Fields
        private Visibility visibility;
        private string name;
        private T handler;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        public string Name {
            get {
                return name;
            }
        }

        /// <summary>
        /// Gets the visibility of the service.
        /// </summary>
        public Visibility Visibility {
            get {
                return visibility;
            }
        }
        #endregion

        #region Methods
        public void InvokeMessage() {

        }

        public void InvokeRequest() {

        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new service instance.
        /// </summary>
        /// <param name="name">The name.</param>
        internal Service(string name, T handler)
            : this(name, Visibility.Private, handler)
            { }

        /// <summary>
        /// Creates a new service instance and specifies it's visibility.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="visibility">The visibility.</param>
        /// <param name="handler">The handler.</param>
        internal Service(string name, Visibility visibility, T handler) {
            this.name = name;
            this.visibility = visibility;
            this.handler = handler;
        }
        #endregion
    }
}
