namespace LiteraWorker.Core.Models;

/// <summary>
/// An exception that is thrown when the app cannot find persistent user identity
/// in the local storage
/// </summary>
public sealed class IdentityNotFoundException : Exception
{
    private static string _message = "Persistent user identity not found. User must log in again.";

    public IdentityNotFoundException() : base(_message)
    {
    }

    public IdentityNotFoundException(string message) : base(message)
    {
    }
}