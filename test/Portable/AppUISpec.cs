namespace Nine.Application
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public abstract class AppUISpec<TData> : ITestData<IAppUI> where TData : ITestData<IAppUI>, new()
    {
        public static IEnumerable<object[]> Data = new TestDimension<TData, IAppUI>();

        public abstract IEnumerable<IAppUI> GetData();

        private readonly IMediaLibrary media;

        public AppUISpec() : this(null) { }
        public AppUISpec(IMediaLibrary media) { this.media = media ?? new MediaLibrary(); }

        [Theory, MemberData("Data")]
        public async Task confirm_yes(IAppUI ui)
        {
            var mock = new AppUIMock(ui, media);

            await mock.Confirm(
                "Are you sure you want to participate in this test with a really really long title ???",
                "Sed eu vero dolor postulant, delenit sententiae mel at, decore facete placerat ad eam. " +
                "Vis odio mazim eu. Duo tota nominati no, maiorum perpetua has id, soleat ullamcorper intellegebat vis at." +
                "His mundi eleifend no, ei eligendi delectus invenire cum. Dolor placerat euripidis an mei. An meis laudem civibus usu.",
                "Ad vis epicurei scripserit complectitur", "Quem lobortis id quo");

            await mock.Confirm("This is the title of the confirm", "This is the message of the confirm", "yes text", "no text");
            await mock.Confirm("This is the title of the confirm", "This is the message of the confirm", "yes text");
        }
    }
}