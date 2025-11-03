namespace AuthService.Domain.ValueObjects
{
    public record EmailAddress
    {
        public string Value { get; }
        public EmailAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
                throw new ArgumentException("Invalid email address");
            Value = value;
        }
        public override string ToString() => Value;
    }
}
