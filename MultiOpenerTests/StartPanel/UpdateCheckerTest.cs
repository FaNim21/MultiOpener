using MultiOpener.Utils;

namespace MultiOpenerTests.StartPanel
{
    [TestFixture]
    internal class UpdateCheckerTest
    {
        private UpdateChecker _updateChecker;

        [SetUp]
        public void SetUp()
        {
            _updateChecker = new UpdateChecker();
        }

        [Test]
        public async Task CheckForUpdates_ReturnsTrue_WhenUpToDate()
        {
            bool result = await _updateChecker.CheckForUpdates("0.1.0");

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CheckForUpdates_ReturnsFalse_WhenNotUpToDate()
        {
            bool result = await _updateChecker.CheckForUpdates("10.0.0");

            Assert.That(result, Is.False);
        }
    }
}
