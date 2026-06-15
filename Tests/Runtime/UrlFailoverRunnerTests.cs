using System;
using System.Collections;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using GameFrameX.Startup.Runtime;

using NUnit.Framework;

using UnityEngine.TestTools;

namespace GameFrameX.Startup.Runtime.Tests
{
    [TestFixture]
    internal sealed class UrlFailoverRunnerTests
    {
        [UnityTest]
        public IEnumerator ExecuteAsync_SingleUrlSucceeds_ReturnsSuccessImmediately() => UniTask.ToCoroutine(async () =>
        {
            var attempts = 0;

            var result = await UrlFailoverRunner.ExecuteAsync(
                new[] { "http://a" },
                3,
                0,
                url =>
                {
                    attempts++;
                    return UniTask.FromResult(UrlAttemptResult.Succeed());
                });

            Assert.IsTrue(result.Success);
            Assert.AreEqual(string.Empty, result.FailedUrl);
            Assert.AreEqual(string.Empty, result.ErrorMessage);
            Assert.AreEqual(1, attempts);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_RetriesCurrentUrlBeforeSuccess_UsesOneBasedProgress() => UniTask.ToCoroutine(async () =>
        {
            var attempts = 0;
            var progress = new List<int>();

            var result = await UrlFailoverRunner.ExecuteAsync(
                new[] { "http://a" },
                3,
                0,
                url =>
                {
                    attempts++;
                    return UniTask.FromResult(attempts == 3
                        ? UrlAttemptResult.Succeed()
                        : UrlAttemptResult.Fail("temporary"));
                },
                (url, attempt, total) => progress.Add(attempt));

            Assert.IsTrue(result.Success);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, progress);
            Assert.AreEqual(3, attempts);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_FirstUrlFailsSecondSucceeds_FailoversInOrder() => UniTask.ToCoroutine(async () =>
        {
            var attemptsByUrl = new Dictionary<string, int>();

            var result = await UrlFailoverRunner.ExecuteAsync(
                new[] { "http://a", "http://b" },
                2,
                0,
                url =>
                {
                    attemptsByUrl.TryGetValue(url, out var count);
                    attemptsByUrl[url] = count + 1;

                    return UniTask.FromResult(url == "http://b"
                        ? UrlAttemptResult.Succeed()
                        : UrlAttemptResult.Fail("down"));
                });

            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, attemptsByUrl["http://a"]);
            Assert.AreEqual(1, attemptsByUrl["http://b"]);
        });

        [UnityTest]
        public IEnumerator ExecuteAsync_AllUrlsFail_ReturnsLastFailureWithoutThrowing() => UniTask.ToCoroutine(async () =>
        {
            var attempts = 0;
            var progressCalls = 0;

            var result = await UrlFailoverRunner.ExecuteAsync(
                new[] { "http://a", "http://b" },
                3,
                0,
                url =>
                {
                    attempts++;
                    return UniTask.FromResult(UrlAttemptResult.Fail("failed " + url));
                },
                (url, attempt, total) => progressCalls++);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("http://b", result.FailedUrl);
            Assert.AreEqual("failed http://b", result.ErrorMessage);
            Assert.AreEqual(6, attempts);
            Assert.AreEqual(6, progressCalls);
        });

        [Test]
        public void ExecuteAsync_InvalidArgs_ThrowSynchronously()
        {
            Assert.Throws<ArgumentNullException>(() =>
                UrlFailoverRunner.ExecuteAsync(null, 1, 0, url => UniTask.FromResult(UrlAttemptResult.Succeed())));

            Assert.Throws<ArgumentException>(() =>
                UrlFailoverRunner.ExecuteAsync(new string[0], 1, 0, url => UniTask.FromResult(UrlAttemptResult.Succeed())));

            Assert.Throws<ArgumentException>(() =>
                UrlFailoverRunner.ExecuteAsync(new[] { "http://a" }, 0, 0, url => UniTask.FromResult(UrlAttemptResult.Succeed())));

            Assert.Throws<ArgumentException>(() =>
                UrlFailoverRunner.ExecuteAsync(new[] { "http://a" }, 1, -1, url => UniTask.FromResult(UrlAttemptResult.Succeed())));

            Assert.Throws<ArgumentNullException>(() =>
                UrlFailoverRunner.ExecuteAsync(new[] { "http://a" }, 1, 0, null));
        }

        [UnityTest]
        public IEnumerator ExecuteAsync_MaxAttemptsOne_DoesNotDelayOrRetry() => UniTask.ToCoroutine(async () =>
        {
            var attempts = 0;

            var result = await UrlFailoverRunner.ExecuteAsync(
                new[] { "http://a" },
                1,
                1000,
                url =>
                {
                    attempts++;
                    return UniTask.FromResult(UrlAttemptResult.Fail("once"));
                });

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, attempts);
        });
    }
}
