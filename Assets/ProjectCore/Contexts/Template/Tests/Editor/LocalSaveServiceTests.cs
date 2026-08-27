using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Zenject;

namespace ProjectCore.Template
{
    public sealed class LocalSaveServiceTests
    {
        private string _testRootPath;
        private LocalSaveService _service;

        [SetUp]
        public void SetUp()
        {
            _testRootPath = Path.Combine(
                Path.GetTempPath(),
                "TroughGame.LocalSave.Tests",
                Guid.NewGuid().ToString("N"));

            var pathProvider = new LocalSavePathProvider(_testRootPath);
            _service = new LocalSaveService(
                new ILocalSaveStorage[]
                {
                    new PlayerPrefsLocalSaveStorage(),
                    new FileLocalSaveStorage(pathProvider),
                    new SecureFileLocalSaveStorage(pathProvider)
                },
                new ILocalSaveSerializer[] { new JsonLocalSaveSerializer() });
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();

            if (Directory.Exists(_testRootPath))
            {
                Directory.Delete(_testRootPath, true);
            }
        }

        [Test]
        public void FeatureRegistersOnlyThePublicLocalSaveService()
        {
            var container = new DiContainer();
            var feature = new LocalSaveFeature();

            // Storages and serializers are conditional implementation bindings,
            // while consumers receive only the service contract through DI.
            feature.InstallBindings(container);

            Assert.IsNotNull(container.Resolve<ILocalSaveService>());
            Assert.Throws<ZenjectException>(() => container.Resolve<ILocalSaveStorage>());
            Assert.Throws<ZenjectException>(() => container.Resolve<ILocalSaveSerializer>());
        }

        [Test]
        public async Task PlayerPrefsSaveSupportsCreateLoadExistenceAndDelete()
        {
            string key = $"tests.playerprefs.{Guid.NewGuid():N}";
            var descriptor = new LocalSaveDescriptor<TestSaveData>(
                key,
                LocalSaveStorageTypes.PlayerPrefs);

            // A missing save is created once and immediately persisted for the next application run.
            var createdData = await _service.LoadOrCreateAsync(
                descriptor,
                () => new TestSaveData(5, "created"),
                CancellationToken.None);

            Assert.IsTrue(createdData.IsSuccess);
            Assert.AreEqual(5, createdData.Value.Value);

            // A later explicit save replaces the stored document and can be loaded
            // through the same definition.
            var expectedData = new TestSaveData(10, "updated");
            Assert.IsTrue((await _service.SaveAsync(
                descriptor,
                expectedData,
                CancellationToken.None)).IsSuccess);
            Assert.IsTrue((await _service.ExistsAsync(descriptor, CancellationToken.None)).Value);

            var loadedData = await _service.LoadAsync(descriptor, CancellationToken.None);

            Assert.IsTrue(loadedData.IsSuccess);
            Assert.AreEqual(expectedData.Value, loadedData.Value.Value);
            Assert.AreEqual(expectedData.Label, loadedData.Value.Label);

            // Delete removes the value completely, so a following load reports a structured not-found result.
            Assert.IsTrue((await _service.DeleteAsync(descriptor, CancellationToken.None)).IsSuccess);
            var missingData = await _service.LoadAsync(descriptor, CancellationToken.None);

            Assert.IsTrue(missingData.IsFailure);
            Assert.IsTrue(LocalSaveErrors.IsNotFound(missingData.FirstError));
        }

        [Test]
        public async Task FileBackendsRoundTripAndMigrateFormerKeys()
        {
            string formerKey = $"tests.former.{Guid.NewGuid():N}";
            string currentKey = $"tests.current.{Guid.NewGuid():N}";
            var formerDescriptor = new LocalSaveDescriptor<TestSaveData>(
                formerKey,
                LocalSaveStorageTypes.File);
            var currentDescriptor = new LocalSaveDescriptor<TestSaveData>(
                currentKey,
                LocalSaveStorageTypes.File,
                formerKeys: new[] { formerKey });
            var secureDescriptor = new LocalSaveDescriptor<TestSaveData>(
                $"tests.secure.{Guid.NewGuid():N}",
                LocalSaveStorageTypes.SecureFile);
            var expectedData = new TestSaveData(25, "file");

            // A definition can retain a prior stable key; the first successful read copies it to the new key.
            Assert.IsTrue((await _service.SaveAsync(
                formerDescriptor,
                expectedData,
                CancellationToken.None)).IsSuccess);
            var migratedData = await _service.LoadAsync(currentDescriptor, CancellationToken.None);

            Assert.IsTrue(migratedData.IsSuccess);
            Assert.AreEqual(expectedData.Value, migratedData.Value.Value);
            Assert.IsFalse((await _service.ExistsAsync(formerDescriptor, CancellationToken.None)).Value);
            Assert.IsTrue((await _service.ExistsAsync(currentDescriptor, CancellationToken.None)).Value);

            // SecureFile uses the same public service contract while keeping the
            // serialized document encrypted at rest.
            Assert.IsTrue((await _service.SaveAsync(
                secureDescriptor,
                expectedData,
                CancellationToken.None)).IsSuccess);
            var secureData = await _service.LoadAsync(secureDescriptor, CancellationToken.None);

            Assert.IsTrue(secureData.IsSuccess);
            Assert.AreEqual(expectedData.Label, secureData.Value.Label);
        }

        private sealed class TestSaveData
        {
            public TestSaveData(int value, string label)
            {
                Value = value;
                Label = label;
            }

            public int Value { get; }

            public string Label { get; }
        }
    }
}
