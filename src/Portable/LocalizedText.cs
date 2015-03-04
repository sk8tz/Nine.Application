namespace Nine.Application
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    class LocalizedText
    {
        // https://github.com/fulldecent/FDTake/tree/master/FDTakeExample
        private static readonly Dictionary<string, Dictionary<string, string>> all = new Dictionary<string, Dictionary<string, string>>
        {
            { "Cancel", new Dictionary<string, string> 
                {
                    { "en", "Cancel" },
                    { "en", "Cancel" },
                    { "en", "Cancel" },
                    { "en", "Cancel" },
                    { "en", "Cancel" },
                    { "en", "Cancel" },
                    { "en", "Cancel" },
                    { "en", "Cancel" },
                } }
        };
    }
}
