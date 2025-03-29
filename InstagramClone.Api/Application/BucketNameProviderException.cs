namespace InstagramClone.Api.Application;

public class BucketNameProviderException : Exception
{
    public BucketNameProviderException(string message) : base(message)
    {
    }

    public BucketNameProviderException(string message, Exception inner) : base(message, inner)
    {
    }
}
