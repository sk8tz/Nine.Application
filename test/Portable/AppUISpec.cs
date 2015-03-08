namespace Nine.Application
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public abstract class AppUISpec<TData> : ITestData<IAppUI> where TData : ITestData<IAppUI>, new()
    {
        public static IEnumerable<object[]> Data = new TestDimension<TData, IAppUI>();

        public abstract IEnumerable<IAppUI> GetData();

        [Theory, MemberData("Data")]
        public async Task capture_screenshot(IAppUI ui)
        {
            var mock = new AppUIMock(ui);
            await mock.CaptureScreenshot();
        }

        [Theory, MemberData("Data")]
        public async Task confirm(IAppUI ui)
        {
            var mock = new AppUIMock(ui);
            mock.Yes();

            Assert.True(await mock.Confirm(
                "Are you sure you want to participate in this test with a really really long title ???",
                "Sed eu vero dolor postulant, delenit sententiae mel at, decore facete placerat ad eam. " +
                "Vis odio mazim eu. Duo tota nominati no, maiorum perpetua has id, soleat ullamcorper intellegebat vis at." +
                "His mundi eleifend no, ei eligendi delectus invenire cum. Dolor placerat euripidis an mei. An meis laudem civibus usu.",
                "Ad vis epicurei scripserit complectitur", "Quem lobortis id quo"));

            mock.No();
            Assert.False(await mock.Confirm("This is the title of the confirm", "This is the message of the confirm", "yes text", "no text"));
            Assert.False(await mock.Confirm("This is the title of the confirm", "This is the message of the confirm", "yes text"));
        }

        [Theory, MemberData("Data")]
        public async Task input(IAppUI ui)
        {
            var mock = new AppUIMock(ui);
            mock.Input("default");
            Assert.Equal("default", await mock.Input("What do you want to input", "default", "yes"));
        }

        [Theory, MemberData("Data")]
        public void toast(IAppUI ui)
        {
            var mock = new AppUIMock(ui);
            mock.Toast("This is the title of the toast", "His mundi eleifend no, ei eligendi delectus invenire cum");
        }

        [Theory, MemberData("Data")]
        public async Task select(IAppUI ui)
        {
            var mock = new AppUIMock(ui);
            mock.Select(1);
            Assert.Equal(1, await mock.Select("Choose", 2, new[]
            {
                "sed eu vero dolor postulant",
                "delenit sententiae mel at",
                "decore facete placerat ad"
            }));
        }

        [Theory, MemberData("Data")]
        public async Task notify(IAppUI ui)
        {
            var mock = new AppUIMock(ui);
            Assert.False(await mock.Notify("Hello", "This is the body of a really really long notification"));
        }
    }
}