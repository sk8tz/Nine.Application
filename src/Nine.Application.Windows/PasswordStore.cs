namespace Nine.Application
{
    using System.Linq;
    using Windows.Security.Credentials;

    public class PasswordStore : IPasswordStore
    {
        private readonly string _resourceName;
        private readonly PasswordVault _vault = new PasswordVault();

        public PasswordStore() : this(null) { }
        public PasswordStore(string resourceName) { _resourceName = resourceName ?? "DefaultResource"; }

        public string Retrieve(string key)
        {
            var password = _vault.Retrieve(_resourceName, key);
            if (password == null) return null;

            password.RetrievePassword();
            return password.Password;
        }

        public string[] RetrieveAllKeys()
        {
            return _vault.RetrieveAll().Select(p => p.UserName).ToArray();
        }

        public void Store(string userName, string password)
        {
            _vault.Add(new PasswordCredential(_resourceName, userName, password));
        }

        public void Remove(string key)
        {
            var password = _vault.Retrieve(_resourceName, key);
            if (password != null) _vault.Remove(password);
        }
    }
}
