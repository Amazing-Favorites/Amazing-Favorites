using System.Linq;
using Autofac.Extras.Moq;
using FluentAssertions;
using Newbe.BookmarkManager.Services;
using NUnit.Framework;

namespace Newbe.BookmarkManager.Tests
{
    public class SmallCacheTest
    {
        [Test]
        public void Set()
        {
            using var mocker = AutoMock.GetLoose();
            var smallCache = mocker.Create<SmallCache>();
            smallCache.Set("a", "b");
            smallCache.Cache.Keys.Single().Should().Be("a");
            smallCache.Cache.Values.Single().Should().Be("b");
        }

        [Test]
        public void Get()
        {
            using var mocker = AutoMock.GetLoose();
            var smallCache = mocker.Create<SmallCache>();
            smallCache.Set("a", "b");
            smallCache.TryGetValue<string>("a", out var value).Should().BeTrue();
            value.Should().Be("b");
        }

        [Test]
        public void Get_NotExist()
        {
            using var mocker = AutoMock.GetLoose();
            var smallCache = mocker.Create<SmallCache>();
            smallCache.TryGetValue<string>("a", out var value).Should().BeFalse();
            value.Should().BeNull();
        }

        [Test]
        public void Remove()
        {
            using var mocker = AutoMock.GetLoose();
            var smallCache = mocker.Create<SmallCache>();
            smallCache.Set("a", "b");
            smallCache.Remove("a");
            smallCache.Cache.Keys.Should().BeEmpty();
        }

        [Test]
        public void Remove_NotExist()
        {
            using var mocker = AutoMock.GetLoose();
            var smallCache = mocker.Create<SmallCache>();
            smallCache.Remove("a");
            smallCache.Cache.Keys.Should().BeEmpty();
        }
    }
}