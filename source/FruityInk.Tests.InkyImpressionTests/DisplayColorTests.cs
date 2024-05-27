using System.Diagnostics.CodeAnalysis;
using FruityInk.Driver.InkyImpression.InkyGpioWrapper;

namespace FruityInk.Tests.InkyImpressionTests;

[SuppressMessage("Assertion", "NUnit2021:Incompatible types for EqualTo constraint", Justification = "The analyzer is wrongly flagging a string and const string as being incompatible.")]
public class DisplayColorTests
{
	[Test]
	public void TestDisplayColorFromByte_0_IsBlack()
	{
		// Arrange
		var expected = DisplayColor.Black;

		// Act
		var actual = DisplayColor.FromByte(0);

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorFromByte_1_IsWhite()
	{
		// Arrange
		var expected = DisplayColor.White;

		// Act
		var actual = DisplayColor.FromByte(1);

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorFromByte_2_IsGreen()
	{
		// Arrange
		var expected = DisplayColor.Green;

		// Act
		var actual = DisplayColor.FromByte(2);

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorFromByte_3_IsBlue()
	{
		// Arrange
		var expected = DisplayColor.Blue;

		// Act
		var actual = DisplayColor.FromByte(3);

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorFromByte_4_IsRed()
	{
		// Arrange
		var expected = DisplayColor.Red;

		// Act
		var actual = DisplayColor.FromByte(4);

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorFromByte_5_IsYellow()
	{
		// Arrange
		var expected = DisplayColor.Yellow;

		// Act
		var actual = DisplayColor.FromByte(5);

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorFromByte_6_IsOrange()
	{
		// Arrange
		var expected = DisplayColor.Orange;

		// Act
		var actual = DisplayColor.FromByte(6);

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorFromByte_7_IsClean()
	{
		// Arrange
		var expected = DisplayColor.Clean;

		// Act
		var actual = DisplayColor.FromByte(7);

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[TestCase(8)]
	[TestCase(10)]
	[TestCase(22)]
	[TestCase(byte.MaxValue)]
	public void TestDisplayColorFromByte_OutOfRange_ThrowsException(byte input)
	{
		var ex = Assert.Throws<Exception>(() => DisplayColor.FromByte(input));

		Assert.That(ex, Is.Not.Null);
		Assert.That(ex!.Message, Is.EqualTo($"Invalid byte value for DisplayColor: {input}uy"));
	}

	[Test]
	public void TestDisplayColorToByte_Black_Is0()
	{
		// Arrange
		const byte expected = 0;

		// Act
		var actual = DisplayColor.Black.ToByte();

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorToByte_White_Is1()
	{
		// Arrange
		const byte expected = 1;

		// Act
		var actual = DisplayColor.White.ToByte();

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorToByte_Green_Is2()
	{
		// Arrange
		const byte expected = 2;

		// Act
		var actual = DisplayColor.Green.ToByte();

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorToByte_Blue_Is3()
	{
		// Arrange
		const byte expected = 3;

		// Act
		var actual = DisplayColor.Blue.ToByte();

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorToByte_Red_Is4()
	{
		// Arrange
		const byte expected = 4;

		// Act
		var actual = DisplayColor.Red.ToByte();

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorToByte_Yellow_Is5()
	{
		// Arrange
		const byte expected = 5;

		// Act
		var actual = DisplayColor.Yellow.ToByte();

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorToByte_Orange_Is6()
	{
		// Arrange
		const byte expected = 6;

		// Act
		var actual = DisplayColor.Orange.ToByte();

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorToByte_Clean_Is7()
	{
		// Arrange
		const byte expected = 7;

		// Act
		var actual = DisplayColor.Clean.ToByte();

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorMerge_Black_Black_IsBlack()
	{
		// Arrange
		const string expected = "Black";

		// Act
		var actual = DisplayColor.Black.Merge(
			black: () => "Black",
			white: () => "White",
			blue: () => "Blue",
			green: () => "Green",
			red: () => "Red",
			yellow: () => "Yellow",
			orange: () => "Orange",
			clean: () => "Clean");

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorMerge_White_White_IsWhite()
	{
		// Arrange
		const string expected = "White";

		// Act
		var actual = DisplayColor.White.Merge(
			black: () => "Black",
			white: () => "White",
			blue: () => "Blue",
			green: () => "Green",
			red: () => "Red",
			yellow: () => "Yellow",
			orange: () => "Orange",
			clean: () => "Clean");

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorMerge_Green_Green_IsGreen()
	{
		// Arrange
		const string expected = "Green";

		// Act
		var actual = DisplayColor.Green.Merge(
			black: () => "Black",
			white: () => "White",
			blue: () => "Blue",
			green: () => "Green",
			red: () => "Red",
			yellow: () => "Yellow",
			orange: () => "Orange",
			clean: () => "Clean");

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorMerge_Blue_Blue_IsBlue()
	{
		// Arrange
		const string expected = "Blue";

		// Act
		var actual = DisplayColor.Blue.Merge(
			black: () => "Black",
			white: () => "White",
			blue: () => "Blue",
			green: () => "Green",
			red: () => "Red",
			yellow: () => "Yellow",
			orange: () => "Orange",
			clean: () => "Clean");

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorMerge_Red_Red_IsRed()
	{
		// Arrange
		const string expected = "Red";

		// Act
		var actual = DisplayColor.Red.Merge(
			black: () => "Black",
			white: () => "White",
			blue: () => "Blue",
			green: () => "Green",
			red: () => "Red",
			yellow: () => "Yellow",
			orange: () => "Orange",
			clean: () => "Clean");

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorMerge_Yellow_Yellow_IsYellow()
	{
		// Arrange
		const string expected = "Yellow";

		// Act
		var actual = DisplayColor.Yellow.Merge(
			black: () => "Black",
			white: () => "White",
			blue: () => "Blue",
			green: () => "Green",
			red: () => "Red",
			yellow: () => "Yellow",
			orange: () => "Orange",
			clean: () => "Clean");

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorMerge_Orange_Orange_IsOrange()
	{
		// Arrange
		const string expected = "Orange";

		// Act
		var actual = DisplayColor.Orange.Merge(
			black: () => "Black",
			white: () => "White",
			blue: () => "Blue",
			green: () => "Green",
			red: () => "Red",
			yellow: () => "Yellow",
			orange: () => "Orange",
			clean: () => "Clean");

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TestDisplayColorMerge_Clean_Clean_IsClean()
	{
		// Arrange
		const string expected = "Clean";

		// Act
		var actual = DisplayColor.Clean.Merge(
			black: () => "Black",
			white: () => "White",
			blue: () => "Blue",
			green: () => "Green",
			red: () => "Red",
			yellow: () => "Yellow",
			orange: () => "Orange",
			clean: () => "Clean");

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}
}
