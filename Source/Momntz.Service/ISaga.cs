namespace Momntz.Service
{
    public interface ISaga
    {
        /// <summary>
        /// Gets the type. In Windows Azure Service Bus this is the same as a Subscription
        /// </summary>
        /// <value>The type.</value>
        string Type { get; }
    }
}
