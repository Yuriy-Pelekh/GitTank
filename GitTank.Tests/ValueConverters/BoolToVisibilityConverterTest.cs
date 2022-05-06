using System.Globalization;
using System.Windows;
using GitTank.ValueConverters;
using NUnit.Framework;

namespace GitTank.Tests.ValueConverters
{
    [TestFixture]
    public class BoolToVisibilityConverterTest
    {
        private BoolToVisibilityConverter _target = null!;

        [SetUp]
        public void SetUp()
        {
            _target = new BoolToVisibilityConverter();
        }

        [Test]
        //[TestCase(true, null, ExpectedResult = Visibility.Visible)]
        //[TestCase(false, null, ExpectedResult = Visibility.Collapsed)]
        //[TestCase(true, "INVERSE", ExpectedResult = Visibility.Collapsed)]
        //[TestCase(false, "INVERSE", ExpectedResult = Visibility.Visible)]
        //[TestCase(true, "inverse", ExpectedResult = Visibility.Collapsed)]
        [TestCase(false, "inverse", ExpectedResult = Visibility.Visible)]
        [TestCase(true, "true", ExpectedResult = Visibility.Visible)]
        [TestCase(false, "true", ExpectedResult = Visibility.Collapsed)]
        [TestCase(true, "random string", ExpectedResult = Visibility.Visible)]
        [TestCase(false, "random string", ExpectedResult = Visibility.Collapsed)]
        public Visibility ConvertTest(bool value, string parameter)
        {
            var actual = _target.Convert(value, typeof(Visibility), parameter, CultureInfo.CurrentCulture);
            return (Visibility)actual;
        }

        [Test]
        [TestCase(Visibility.Visible, null, ExpectedResult = true)]
        [TestCase(Visibility.Collapsed, null, ExpectedResult = false)]
        [TestCase(Visibility.Visible, "INVERSE", ExpectedResult = false)]
        [TestCase(Visibility.Collapsed, "INVERSE", ExpectedResult = true)]
        [TestCase(Visibility.Visible, "inverse", ExpectedResult = false)]
        [TestCase(Visibility.Collapsed, "inverse", ExpectedResult = true)]
        [TestCase(Visibility.Visible, "true", ExpectedResult = true)]
        [TestCase(Visibility.Collapsed, "true", ExpectedResult = false)]
        [TestCase(Visibility.Visible, "random string", ExpectedResult = true)]
        [TestCase(Visibility.Collapsed, "random string", ExpectedResult = false)]
        public bool ConvertBackTest(Visibility value, string parameter)
        {
            var actual = _target.ConvertBack(value, typeof(bool), parameter, CultureInfo.CurrentCulture);
            return (bool)actual;
        }
    }
}
