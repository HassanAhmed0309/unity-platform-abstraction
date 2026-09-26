using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace SaveSystem.Tests
{
    [TestFixtureSource(typeof(ServiceFixtureSource), nameof(ServiceFixtureSource.Services))]
    public class SharedServiceContractTests
    {
        readonly Func<IService> _factory;
        IService _service;

        public SharedServiceContractTests(ServiceFactory factory)
        {
            _factory = factory.Create;
        }

        [SetUp]
        public void SetUp()
        {
            _service = _factory();
        }

        [UnityTest]
        public IEnumerator SaveThenLoad_ReturnsSameData() => UniTask.ToCoroutine(async () =>
        {
            string key = Guid.NewGuid().ToString();

            var saveResult = await _service.SaveDataAsync(key, "hello-world");
            Assert.AreEqual(SystemResult.Status.Success, saveResult.Result);

            var loadResult = await _service.LoadDataAsync(key);
            Assert.AreEqual(SystemResult.Status.Success, loadResult.Result);
            Assert.AreEqual("hello-world", loadResult.Data);
        });

        [UnityTest]
        public IEnumerator Load_MissingKey_ReturnsNotFound() => UniTask.ToCoroutine(async () =>
        {
            string key = Guid.NewGuid().ToString();

            var loadResult = await _service.LoadDataAsync(key);
            Assert.AreEqual(SystemResult.Status.NotFound, loadResult.Result);
        });

        [UnityTest]
        public IEnumerator Save_OverwritesPreviousValue_ForSameKey() => UniTask.ToCoroutine(async () =>
        {
            string key = Guid.NewGuid().ToString();

            await _service.SaveDataAsync(key, "first");
            await _service.SaveDataAsync(key, "second");

            var loadResult = await _service.LoadDataAsync(key);
            Assert.AreEqual("second", loadResult.Data);
        });
    }

    public readonly struct ServiceFactory
    {
        readonly string _name;
        public readonly Func<IService> Create;

        public ServiceFactory(string name, Func<IService> create)
        {
            _name = name;
            Create = create;
        }

        public override string ToString() => _name;
    }

    static class ServiceFixtureSource
    {
        public static IEnumerable Services()
        {
            yield return new TestFixtureData(new ServiceFactory("Instant", () => new InstantService()));
            yield return new TestFixtureData(new ServiceFactory("Deferred", () => new DefferedService(() => false)));
            yield return new TestFixtureData(new ServiceFactory("Flaky", () => new FlakyService()));
        }
    }
}
