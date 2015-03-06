namespace Nine.Application
{
    using System.Collections.Generic;

    public class AppUITest : AppUISpec<AppUITest>
    {
        public override IEnumerable<IAppUI> GetData() => new[] { new AppUI() };
    }
}