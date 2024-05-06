namespace COPA.APIManager.BOL.Interfaces
{
    public interface ICredentials
    {
        string Username { get; }
        string Password { get; }
        string Token { get; }
    }
}
