using Google.Cloud.Functions.Hosting;
using System;

namespace Google.Cloud.Functions.Testing
{
    /// <summary>
    /// Class attribute to specify a <see cref="FunctionsStartup"/> to use
    /// for a test server 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class FunctionTestStartupAttribute : Attribute
    {
        /// <summary>
        /// The Type of the <see cref="FunctionsStartup"/> class to register.
        /// </summary>
        public Type StartupType { get; set; }

        /// <summary>
        /// The ordering of application of the provider type relative to others
        /// specified by other attributes. Configurers specified in attributes
        /// with lower order numbers are invoked before those with higher order numbers.
        /// Defaults to 0.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="startupType">The Type of the <see cref="FunctionsStartup"/> class to register.</param>
        public FunctionTestStartupAttribute(Type startupType) =>
            StartupType = startupType;
    }
}
