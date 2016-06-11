namespace Nine.Application
{
    using System;
    using System.Threading;
    using Foundation;
    using Security;

    public class PasswordStore : IPasswordStore
    {
        private readonly SynchronizationContext _syncContext = SynchronizationContext.Current;

        public string Retrieve(string key)
        {
            var rec = new SecRecord(SecKind.GenericPassword) { Generic = NSData.FromString(key) };

            SecStatusCode res;
            var match = SecKeyChain.QueryAsRecord(rec, out res);
            if (res == SecStatusCode.Success) return match.ValueData.ToString();
            return null;
        }

        public string[] RetrieveAllKeys()
        {
            throw new NotImplementedException();
        }

        public void Store(string key, string password)
        {
            var rec = new SecRecord(SecKind.GenericPassword)
            {
                ValueData = NSData.FromString(password),
                Generic = NSData.FromString(key)
            };

            var status = SecKeyChain.Add(rec);

            if (status == SecStatusCode.DuplicateItem)
            {
                SecKeyChain.Update(rec, rec);
            }
        }

        public void Remove(string key)
        {
            var rec = new SecRecord(SecKind.GenericPassword) { Generic = NSData.FromString(key) };

            SecKeyChain.Remove(rec);
        }
    }
}
