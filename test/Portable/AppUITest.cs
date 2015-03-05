namespace Nine.Application
{
    using System.Collections.Generic;

    public class AppUITest : AppUISpec<AppUITest>
    {
        public override IEnumerable<IAppUI> GetData()
        {
            yield return new AppUI();
        }
    }
}