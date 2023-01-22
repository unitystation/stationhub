using System;
using System.Globalization;
using FluentAssertions;
using UnitystationLauncher.ValueConverters;
using Xunit;

namespace UnitystationLauncher.UnitTests.ValueConverters;

public class HumanizerValueConverterTests
{
    private HumanizerValueConverter _humanizerValueConverter;

    public HumanizerValueConverterTests()
    {
        _humanizerValueConverter = new();
    }

    #region HumanizerValueConverter.Convert
    [Fact]
    public void Convert_ShouldHandleNullValues()
    {
        Action act = () => _humanizerValueConverter.Convert(null, null, null, null);
        act.Should().NotThrow();
    }

    [Fact]
    public void Convert_ShouldConvertDateTimeValues()
    {
        DateTime dateTime = DateTime.Now;

        string expected = "now";
        object? actual = _humanizerValueConverter.Convert(dateTime, typeof(DateTime), "what is this for?",
            CultureInfo.InvariantCulture);

        actual.Should().NotBeNull().And.Be(expected);
    }

    [Fact]
    public void Convert_ShouldConvertDateTimeOffsetValues()
    {
        int offsetHours = 5;
        DateTimeOffset dateTimeOffset = DateTimeOffset.Now.AddHours(offsetHours);

        string expected = $"{offsetHours - 1} hours from now";
        object? actual = _humanizerValueConverter.Convert(dateTimeOffset, typeof(DateTimeOffset),
            "what is this for?", CultureInfo.InvariantCulture);

        actual.Should().NotBeNull().And.Be(expected);
    }

    [Fact]
    public void Convert_ShouldConvertTimeSpanValues()
    {
        int timeSpanHours = 5;
        TimeSpan timeSpan = TimeSpan.FromHours(timeSpanHours);

        string expected = $"{timeSpanHours} hours";
        object? actual = _humanizerValueConverter.Convert(timeSpan, typeof(TimeSpan),
            "what is this for?", CultureInfo.InvariantCulture);

        actual.Should().NotBeNull().And.Be(expected);
    }
    #endregion

    // TODO: Remember to un-skip these once this method is implemented
    #region HumanizerValueConverter.ConvertBack
    [Fact(Skip = "Method not implemented")]
    public void ConvertBack_ShouldHandleNullValues()
    {
        Action act = () => _humanizerValueConverter.ConvertBack(null, null, null, null);
        act.Should().NotThrow();
    }
    #endregion
}