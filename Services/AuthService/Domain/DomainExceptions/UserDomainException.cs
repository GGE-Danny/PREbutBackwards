namespace AuthService.Domain.DomainExceptions
{
    public class UserDomainException : Exception
    {
        public UserDomainException(string message) : base(message) { }
    }
}
