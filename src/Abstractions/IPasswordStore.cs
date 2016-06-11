namespace Nine.Application
{
    public interface IPasswordStore
    {
        string Retrieve(string key);

        string[] RetrieveAllKeys();

        void Store(string key, string value);

        void Remove(string key);
    }
}
