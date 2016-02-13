namespace Nine.Application
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IAppBundle
    {
        /// <summary>
        /// Retrive files that are shipped with the application.
        /// </summary>
        Task<Stream> Open(string filename);
    }
}
