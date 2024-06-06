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

        [Test]
        public void CheckForUpdates_ReturnsTrue_WithPreviewTag_WhenUpToDate()
        {
            bool result = UpdateChecker.IsUpToDate("1.0.0-PREVIEW43", "1.0.0-PREVIEW44");

            Assert.That(result, Is.True);
        }

        [Test]
        public void CheckForUpdates_ReturnsFalse_WithPreviewTag_WhenNotUpToDate() 
        {
            bool result = UpdateChecker.IsUpToDate("1.0.0-PREVIEW50", "1.0.0-PREVIEW44");

            Assert.That(result, Is.False);
        }

        [Test]
        public void CheckForUpdates_ReturnsFalse_WithPreviewTagAndNormal_WhenNotUpToDate() 
        {
            bool result = UpdateChecker.IsUpToDate("1.0.1", "1.0.0-PREVIEW44");

            Assert.That(result, Is.False);
        }

        [Test]
        public void CheckForUpdates_ReturnsFalse_WithPreviewTagAndNormal_WhenNotUpToDate2()
        {
            bool result = UpdateChecker.IsUpToDate("1.0.0", "1.0.0-PREVIEW44");

            Assert.That(result, Is.False);
        }

        [Test]
        public void CheckForUpdates_ReturnsTrue_WithPreviewTagAndNormal_WhenUpToDate()
        {
            bool result = UpdateChecker.IsUpToDate("1.0.0-PREVIEW44", "1.0.0");

            Assert.That(result, Is.True);
        }

        [Test]
        public void CheckForUpdates_ReturnsFalse_WithPreviewTagAndNormal_WhenUpNotToDate() 
        {
            bool result = UpdateChecker.IsUpToDate("1.0.0-PREVIEW44", "0.4.0");

            Assert.That(result, Is.False);
        }
    }
}
