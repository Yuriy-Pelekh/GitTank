using System;
using System.Globalization;
using System.IO;
using GitTank.ValueConverters;
using NUnit.Framework;

namespace GitTank.Tests.ValueConverters
{
    [TestFixture]
    public class RelativeToFullPathValueConverterTest
    {
        private RelativeToFullPathValueConverter _target = null!;

        [SetUp]
        public void SetUp()
        {
            _target = new RelativeToFullPathValueConverter();
        }

        [Test]
        public void ConvertTest()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var pathRoot = Path.GetPathRoot(currentDirectory);
            var relativePath = Path.GetRelativePath(currentDirectory, pathRoot!);
            var rawActual = _target.Convert(relativePath, typeof(string), null, CultureInfo.CurrentCulture);
            Assert.NotNull(rawActual);
            var actual = (string)rawActual;
            Assert.False(actual.Contains(".."), actual);
        }

        [Test]
        public void ConvertEmptyValueTest()
        {
            var relativePath = string.Empty;
            Assert.Throws<ArgumentException>(() => _target.Convert(relativePath, typeof(string), null, CultureInfo.CurrentCulture));
        }

        [Test]
        public void ConvertNullValueTest()
        {
            string? relativePath = null;
            Assert.Throws<ArgumentNullException>(() => _target.Convert(relativePath, typeof(string), null, CultureInfo.CurrentCulture));
        }

        [Test]
        public void ConvertBackTest()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var absolutePath = Path.GetFullPath("..", currentDirectory);
            var rawActual = _target.ConvertBack(absolutePath, typeof(string), null, CultureInfo.CurrentCulture);
            Assert.NotNull(rawActual);
            var actual = (string) rawActual;
            Assert.True(actual.Contains(".."), actual);
        }

        [Test]
        public void ConvertBackEmptyValueTest()
        {
            var relativePath = string.Empty;
            Assert.Throws<ArgumentException>(() => _target.ConvertBack(relativePath, typeof(string), null, CultureInfo.CurrentCulture));
        }

        [Test]
        public void ConvertBackNullValueTest()
        {
            string? relativePath = null;
            Assert.Throws<ArgumentNullException>(() => _target.ConvertBack(relativePath, typeof(string), null, CultureInfo.CurrentCulture));
        }
    }
}
