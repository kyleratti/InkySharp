using FakeItEasy;
using InkySharp.Driver.InkyImpression.GpioControllerWrapper;
using InkySharp.Driver.InkyImpression.InkyGpioWrapper;
using InkySharp.Driver.InkyImpression.InkyImpressionDriver;
using InkySharp.Driver.InkyImpression.SpiDeviceWrapper;

namespace InkySharp.Tests.InkyImpressionTests;

public class InkyImpressionWrapperTests
{
	private ISpiDeviceWrapper _fakeSpiDevice = null!;
	private IGpioControllerWrapper _fakeGpio = null!;
	private IInkyImpressionDriver _fakeInkyDriver = null!;
	private IInkyImpressionWrapper _inkyImpression = null!;

	[SetUp]
	public void SetUp()
	{
		_fakeSpiDevice = A.Fake<ISpiDeviceWrapper>();
		_fakeGpio = A.Fake<IGpioControllerWrapper>();
		_fakeInkyDriver = A.Fake<IInkyImpressionDriver>();

		_inkyImpression = new InkyImpressionWrapper(
			isHorizontalFlipped: false,
			isVerticalFlipped: false,
			inkyImpressionDriver: _fakeInkyDriver);
	}

	[TearDown]
	public void TearDown()
	{
		_fakeSpiDevice.Dispose();
		_fakeGpio.Dispose();
	}

	[Test]
	public void TestSetPixel()
	{
		var startingBuffer = _inkyImpression.GetPixels();
		Assert.That(startingBuffer[1, 1], Is.Not.EqualTo(DisplayColor.Orange));

		_inkyImpression.SetPixel(1, 1, DisplayColor.Orange);

		var newBuffer = _inkyImpression.GetPixels();

		Assert.That(newBuffer, Is.Not.EqualTo(startingBuffer));
		Assert.That(newBuffer[1, 1], Is.EqualTo(DisplayColor.Orange));
	}

	[TestCase(-1)]
	[TestCase(928)]
	public void TestSetPixel_ThrowsIfOutOfBounds_X(int x)
	{
		var ex = Assert.Throws<Exception>(() =>
			_inkyImpression.SetPixel(x, 0, DisplayColor.Black));

		Assert.That(ex, Is.Not.Null);
		Assert.That(ex!.Message, Is.EqualTo($"x must be >= 0 and < 600, but was {x}"));
	}

	[TestCase(-1)]
	[TestCase(928)]
	public void TestSetPixel_ThrowsIfOutOfBounds_Y(int y)
	{
		var ex = Assert.Throws<Exception>(() =>
			_inkyImpression.SetPixel(0, y,  DisplayColor.Black));

		Assert.That(ex, Is.Not.Null);
		Assert.That(ex!.Message, Is.EqualTo($"y must be >= 0 and < 448, but was {y}"));
	}

	private const int WIDTH = 600;
	private const int HEIGHT = 448;

	[Test]
	public async Task TestShow()
	{
		_inkyImpression.SetPixel(0, 0, DisplayColor.Red);

		var expectedBuffer = new DisplayColor[WIDTH, HEIGHT];
		for (var i = 0; i < WIDTH; i++)
			for (var j = 0; j < HEIGHT; j++)
				expectedBuffer[i,j] = DisplayColor.Black;

		expectedBuffer[0, 0] = DisplayColor.Red;
		var expectedBufferFlat = expectedBuffer
			.Cast<DisplayColor>()
			.Select(x => x.ToByte())
			.ToArray();
		var resultBuf = new byte[expectedBufferFlat.Length / 2];
		for (var i = 0; i < resultBuf.Length; i++)
		{
			resultBuf[i] = (byte)(((expectedBufferFlat[2 * i] << 4) & 0xF0) | (expectedBufferFlat[2 * i + 1] & 0x0F));
		}

		await _inkyImpression.Show();

		A.CallTo(() => _fakeInkyDriver.FlushBufferToDisplay(resultBuf))
			.MustHaveHappenedOnceExactly();
	}

	[Test]
	public async Task TestShow_FlipHorizontal()
	{
		_inkyImpression = new InkyImpressionWrapper(
			isHorizontalFlipped: true,
			isVerticalFlipped: false,
			inkyImpressionDriver: _fakeInkyDriver);

		_inkyImpression.SetPixel(0, 0, DisplayColor.Red);

		var expectedBuffer = new DisplayColor[HEIGHT, WIDTH];
		for (var i = 0; i < HEIGHT; i++)
			for (var j = 0; j < WIDTH; j++)
				expectedBuffer[i,j] = DisplayColor.Black;

		expectedBuffer[expectedBuffer.GetLength(0) - 1,0] = DisplayColor.Red;
		var expectedBufferFlat = expectedBuffer
			.Cast<DisplayColor>()
			.Select(x => x.ToByte())
			.ToArray();
		var resultBuf = new byte[expectedBufferFlat.Length / 2];
		for (var i = 0; i < resultBuf.Length; i++)
		{
			resultBuf[i] = (byte)(((expectedBufferFlat[2 * i] << 4) & 0xF0) | (expectedBufferFlat[2 * i + 1] & 0x0F));
		}

		await _inkyImpression.Show();

		A.CallTo(() => _fakeInkyDriver.FlushBufferToDisplay(resultBuf))
			.MustHaveHappenedOnceExactly();
	}

	[Test]
	public async Task TestShow_FlipVertical()
	{
		_inkyImpression = new InkyImpressionWrapper(
			isHorizontalFlipped: false,
			isVerticalFlipped: true,
			inkyImpressionDriver: _fakeInkyDriver);

		_inkyImpression.SetPixel(0, 0, DisplayColor.Red);

		var expectedBuffer = new DisplayColor[HEIGHT, WIDTH];
		for (var i = 0; i < HEIGHT; i++)
		for (var j = 0; j < WIDTH; j++)
			expectedBuffer[i,j] = DisplayColor.Black;

		expectedBuffer[0,expectedBuffer.GetLength(1) - 1] = DisplayColor.Red;
		var expectedBufferFlat = expectedBuffer
			.Cast<DisplayColor>()
			.Select(x => x.ToByte())
			.ToArray();
		var resultBuf = new byte[expectedBufferFlat.Length / 2];
		for (var i = 0; i < resultBuf.Length; i++)
		{
			resultBuf[i] = (byte)(((expectedBufferFlat[2 * i] << 4) & 0xF0) | (expectedBufferFlat[2 * i + 1] & 0x0F));
		}

		await _inkyImpression.Show();

		A.CallTo(() => _fakeInkyDriver.FlushBufferToDisplay(resultBuf))
			.MustHaveHappenedOnceExactly();
	}

	[Test]
	public async Task TestShow_FlipHorizontalAndFlipVertical()
	{
		_inkyImpression = new InkyImpressionWrapper(
			isHorizontalFlipped: true,
			isVerticalFlipped: true,
			inkyImpressionDriver: _fakeInkyDriver);

		_inkyImpression.SetPixel(0, 0, DisplayColor.Red);

		var expectedBuffer = new DisplayColor[WIDTH, HEIGHT];
		for (var i = 0; i < WIDTH; i++)
		for (var j = 0; j < HEIGHT; j++)
			expectedBuffer[i,j] = DisplayColor.Black;

		expectedBuffer[expectedBuffer.GetLength(0) - 1,expectedBuffer.GetLength(1) - 1] = DisplayColor.Red;
		var expectedBufferFlat = expectedBuffer
			.Cast<DisplayColor>()
			.Select(x => x.ToByte())
			.ToArray();
		var resultBuf = new byte[expectedBufferFlat.Length / 2];
		for (var i = 0; i < resultBuf.Length; i++)
		{
			resultBuf[i] = (byte)(((expectedBufferFlat[2 * i] << 4) & 0xF0) | (expectedBufferFlat[2 * i + 1] & 0x0F));
		}

		await _inkyImpression.Show();

		A.CallTo(() => _fakeInkyDriver.FlushBufferToDisplay(resultBuf))
			.MustHaveHappenedOnceExactly();
	}
}
