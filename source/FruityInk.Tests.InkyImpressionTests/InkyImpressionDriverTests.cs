using System.Device.Gpio;
using FakeItEasy;
using FruityInk.Driver.InkyImpression.GpioControllerWrapper;
using FruityInk.Driver.InkyImpression.InkyGpioWrapper;
using FruityInk.Driver.InkyImpression.SpiDeviceWrapper;

namespace FruityInk.Tests.InkyImpressionTests;

public class InkyImpressionDriverTests
{
	private ISpiDeviceWrapper _fakeSpiBus = null!;
	private IGpioControllerWrapper _fakeGpio = null!;
	private IInkyImpressionDriver _inkyImpressionDriver = null!;
	private IGpioHelper _fakeGpioHelper = null!;

	[SetUp]
	public void SetUp()
	{
		_fakeSpiBus = A.Fake<ISpiDeviceWrapper>();
		_fakeGpio = A.Fake<IGpioControllerWrapper>();
		_fakeGpioHelper = A.Fake<IGpioHelper>();
		_inkyImpressionDriver = new InkyImpressionDriver(
			csPin: GpioPinName.Cs0,
			dcPin: GpioPinName.Dc,
			resetPin: GpioPinName.Reset,
			busyPin: GpioPinName.Busy,
			spiBus: _fakeSpiBus,
			gpio: _fakeGpio,
			gpioHelper: _fakeGpioHelper);
	}

	[TearDown]
	public void TearDown()
	{
		_fakeSpiBus.Dispose();
		_fakeGpio.Dispose();
	}

	private const byte _SPI_COMMAND = 0;
	private const byte _SPI_DATA = 1;

	[TestCase(new byte[] { 0x00 })]
	[TestCase(new byte[] { 0xAA })]
	[TestCase(new byte[] { 0x22 })]
	[TestCase(new byte[] { (3 /* DisplayColor.Blue */ << 5) | 0x17 })]
	[TestCase(new byte[] { 0x3C })]
	[TestCase(new byte[]
	{
		(0x06 << 3) | // ??? - not documented in UC8159 datasheet
		(0x01 << 2) | // SOURCE_INTERNAL_DC_DC
		(0x01 << 1) | // GATE_INTERNAL_DC_DC
		0x01, // LV_SOURCE_INTERNAL_DC_DC
		0x00, // VGx_20V
		0x23, // UC8159_7C
		0x23, // UC8159_7C
	})]
	public void TestSendData(byte[] data)
	{
		// clone data to a new array
		var expectedData = data.ToArray();

		TestSpiWrite(_SPI_DATA, data, expectedData);
    }

	[TestCase(_SPI_COMMAND, new byte[] { 0x03 /* UC8159_PFS */ }, new byte[] { 0x03 })]
	[TestCase(_SPI_COMMAND, new byte[] { 0x03 /* UC8159_PWS */ }, new byte[] { 0x03 })]
	[TestCase(_SPI_COMMAND, new byte[] { 0x60 /* UC8159_TCON */}, new byte[] { 0x60 })]
	public void TestSpiWrite(int dc, byte[] data, byte[] expectedData)
	{
		_inkyImpressionDriver.SpiWrite(dc, data);

		// Check if self._gpio.output(self.cs_pin, 0) was called.
        A.CallTo(() =>
			_fakeGpio.Write(A<int>.That.IsEqualTo((int)GpioPinName.Cs0), A<PinValue>.That.IsEqualTo(PinValue.Low)))
		.MustHaveHappened()
	    .Then( // Check if self._gpio.output(self.dc_pin, dc) - dc = _SPI_COMMAND (0) was called
			A.CallTo(() =>
				_fakeGpio.Write(A<int>.That.IsEqualTo((int)GpioPinName.Dc), A<PinValue>.That.IsEqualTo(dc)))
		.MustHaveHappened())
		.Then( // Check if self._spi_bus.xfer3(values) - values = [0x01] was called
			A.CallTo(() =>
				_fakeSpiBus.Write(A<byte[]>.That.IsSameSequenceAs(expectedData)))
		.MustHaveHappened())
		.Then( // Check if self._gpio.output(self.cs_pin, 1) was called
			A.CallTo(() =>
				_fakeGpio.Write(A<int>.That.IsEqualTo((int)GpioPinName.Cs0), A<PinValue>.That.IsEqualTo(1)))
		.MustHaveHappened());
	}

	[Test]
	public void TestSpiWriteThrowsOnEmptyData()
	{
		var data = Array.Empty<byte>();

		var ex = Assert.Throws<Exception>(
			() => _inkyImpressionDriver.SpiWrite(_SPI_DATA, data));

		Assert.That(ex, Is.Not.Null);
		Assert.That(ex!.Message, Is.EqualTo("Cannot write empty data to SPI bus"));
	}

	[Test]
	public void TestInitResolutionSetting()
	{
		_inkyImpressionDriver.ResetResolutionSetting();

		AssertInitResolutionSetting();
	}

	private void AssertInitResolutionSetting()
	{
		var data = new byte[] { 0x01, 0xF4, 0x01, 0xC0 };
		var expectedData = new byte[] { 0x01, 0xF4, 0x01, 0xC0 };

		TestSpiWrite(_SPI_COMMAND, [(byte)UCBytes.UC8159_TRES], expectedData: [0x61]);
		TestSpiWrite(_SPI_DATA, data, expectedData: expectedData);
	}

	[Test]
	public void TestInitPanelSettings()
	{
		_inkyImpressionDriver.ResetPanelSettings();

		AssertInitPanelSettings();
	}

	private void AssertInitPanelSettings()
	{
		const int resolutionAsBinary = 0b11; // setting resolutionAsBinary to 0b11
		var data = new byte[]
		{
			(resolutionAsBinary << 6) | 0x2F, // See above for more magic numbers
			0x08                                        // display_colours == UC8159_7C
		};
		var expectedData = new byte[] { 0xEF, 0x08 };

		TestSpiWrite(_SPI_COMMAND, [(byte)UCBytes.UC8159_PSR], expectedData: [0x00]);
		TestSpiWrite(_SPI_DATA, data, expectedData: expectedData);
	}

	[Test]
	public void TestInitPowerSettings()
	{
		_inkyImpressionDriver.ResetPowerSettings();

		AssertInitPowerSettings();
	}

	private void AssertInitPowerSettings()
	{
		var data = new byte[]
		{
			(0x06 << 3) |  // ??? - not documented in UC8159 datasheet  # noqa: W504
			(0x01 << 2) |  // SOURCE_INTERNAL_DC_DC                     # noqa: W504
			(0x01 << 1) |  // GATE_INTERNAL_DC_DC                       # noqa: W504
			0x01,        // LV_SOURCE_INTERNAL_DC_DC
			0x00,          // VGx_20V
			0x23,          // UC8159_7C
			0x23           // UC8159_7C
		};

		var expectedData = new byte[]
		{
			0x37,
			0x00,
			0x23,
			0x23,
		};

		TestSpiWrite(_SPI_COMMAND, [(byte)UCBytes.UC8159_PWR], expectedData: [0x01]);
		TestSpiWrite(_SPI_DATA, data, expectedData: expectedData);
	}

	[Test]
	public void TestInitSetPllClockFrequency()
	{
		_inkyImpressionDriver.ResetSetPllClockFrequency();

		AssertInitSetPllClockFrequency();
	}

	private void AssertInitSetPllClockFrequency()
	{
		TestSpiWrite(_SPI_COMMAND, [(byte)UCBytes.UC8159_PLL], expectedData: [0x30]);
		TestSpiWrite(_SPI_DATA, [0x1C], expectedData: [0x1C]);
	}

	[Test]
	public void TestInitSendTseRegisterToDisplay()
	{
		_inkyImpressionDriver.ResetSendTseRegisterToDisplay();

		AssertInitSendTseRegisterToDisplay();
	}

	private void AssertInitSendTseRegisterToDisplay()
	{
		TestSpiWrite(_SPI_COMMAND, [(byte)UCBytes.UC8159_TSE], expectedData: [0x41]);
		TestSpiWrite(_SPI_DATA, [0x00], expectedData: [0x00]);
	}

	[Test]
	public void TestInitVcomAndDataIntervalSetting()
	{
		_inkyImpressionDriver.ResetVcomAndDataIntervalSetting();

		AssertInitVcomAndDataIntervalSetting();
	}

	private void AssertInitVcomAndDataIntervalSetting()
	{
		const byte borderColor = 0x37; // BorderColor.White

		TestSpiWrite(_SPI_COMMAND, [(byte)UCBytes.UC8159_CDI], expectedData: [0x50]);
		TestSpiWrite(_SPI_DATA, [borderColor], expectedData: [0x37]);
	}

	[Test]
	public void TestInitGateSourceNonOverlapPeriod()
	{
		_inkyImpressionDriver.ResetGateSourceNonOverlapPeriod();

		AssertInitGateSourceNonOverlapPeriod();
	}

	private void AssertInitGateSourceNonOverlapPeriod()
	{
		TestSpiWrite(_SPI_COMMAND, [(byte)UCBytes.UC8159_TCON], expectedData: [0x60]);
		TestSpiWrite(_SPI_DATA, [0x22], expectedData: [0x22]);
	}

	[Test]
	public void TestInitDisableExternalFlash()
	{
		_inkyImpressionDriver.ResetDisableExternalFlash();

		AssertInitDisableExternalFlash();
	}

	private void AssertInitDisableExternalFlash()
	{
		TestSpiWrite(_SPI_COMMAND, [(byte)UCBytes.UC8159_DAM], expectedData: [0x65]);
		TestSpiWrite(_SPI_DATA, [0x00], expectedData: [0x00]);
	}

	[Test]
	public void TestInitUC8159_7C()
	{
		_inkyImpressionDriver.ResetUC8159_7C();

		AssertInitUC8159_7C();
	}

	private void AssertInitUC8159_7C()
	{
		TestSpiWrite(_SPI_COMMAND, [(byte)UCBytes.UC8159_PWS], expectedData: [0xE3]);
		TestSpiWrite(_SPI_DATA, [0xAA], expectedData: [0xAA]);
	}

	[Test]
	public void TestInitPowerOffSequence()
	{
		_inkyImpressionDriver.ResetPowerOffSequence();

		AssertInitPowerOffSequence();
	}

	private void AssertInitPowerOffSequence()
	{
		TestSpiWrite(_SPI_COMMAND, [(byte)UCBytes.UC8159_PFS], expectedData: [0x03]);
		TestSpiWrite(_SPI_DATA, [0x00], expectedData: [0x00]);
	}

	[Test]
	public void TestInitGpio()
	{
		//
	}

	[Test]
	public void TestResetDisplay()
	{
		AssertInitPowerOffSequence();
		AssertInitPanelSettings();
		AssertInitPowerSettings();
		AssertInitSetPllClockFrequency();
		AssertInitSendTseRegisterToDisplay();
		AssertInitVcomAndDataIntervalSetting();
		AssertInitGateSourceNonOverlapPeriod();
		AssertInitDisableExternalFlash();
		AssertInitUC8159_7C();
		AssertInitPowerOffSequence();
	}

	[Test]
	public void TestResetDisplayBeforeInitializeThrowsException()
	{
		var ex = Assert.Throws<Exception>(() => _inkyImpressionDriver.ResetDisplay());

		Assert.That(ex, Is.Not.Null);
		Assert.That(ex!.Message, Is.EqualTo("You must initialize the driver before resetting the display"));
	}

	[Test]
	public async Task TestFlushBufferToDisplay()
	{
		await _inkyImpressionDriver.Initialize();

		var bytes = new byte[2, 2]
		{
			{ 0x00, 0x01 },
			{ 0x02, 0x03 }
		};
		var flattenedData = bytes.Cast<byte>().ToArray();
		var expectedData = new byte[]
		{
			0x00,
			0x01,
			0x02,
			0x03
		};

		await _inkyImpressionDriver.FlushBufferToDisplay(flattenedData);

		TestResetDisplay();

		TestSpiWrite(_SPI_COMMAND, [(byte)UCBytes.UC8159_DTM1], expectedData: [0x10]);
		TestSpiWrite(_SPI_DATA, flattenedData, expectedData: expectedData);

		TestSpiWrite(_SPI_COMMAND, [(byte)UCBytes.UC8159_PON], expectedData: [0x04]);
		A.CallTo(() => _fakeGpioHelper.WaitForBusyPin(GpioPinName.Busy, TimeSpan.FromMilliseconds(210)))
			.MustHaveHappenedOnceExactly();

		TestSpiWrite(_SPI_COMMAND, [(byte)UCBytes.UC8159_DRF], expectedData: [0x04]);
		A.CallTo(() => _fakeGpioHelper.WaitForBusyPin(GpioPinName.Busy, TimeSpan.FromSeconds(32)))
			.MustHaveHappenedOnceExactly();

		TestSpiWrite(_SPI_COMMAND, [(byte)UCBytes.UC8159_PON], expectedData: [0x04]);
		A.CallTo(() => _fakeGpioHelper.WaitForBusyPin(GpioPinName.Busy, TimeSpan.FromMilliseconds(200)))
			.MustHaveHappenedOnceExactly();
	}

	/*[Test]
	public void TestInitGpioDoesNotRunAgainIfAlreadyInitialized()
	{
		A.CallTo(() => _inkyGpioWrapper.isGpioSetUp)
			.Returns(true);

		_inkyGpioWrapper.Initialize();

		A.CallTo(() => _inkyGpioWrapper.InitResolutionSetting()).MustNotHaveHappened();
		A.CallTo(() => _inkyGpioWrapper.InitPanelSettings()).MustNotHaveHappened();
		A.CallTo(() => _inkyGpioWrapper.InitPowerSettings()).MustNotHaveHappened();
		A.CallTo(() => _inkyGpioWrapper.InitSetPllClockFrequency()).MustNotHaveHappened();
		A.CallTo(() => _inkyGpioWrapper.InitSendTseRegisterToDisplay()).MustNotHaveHappened();
		A.CallTo(() => _inkyGpioWrapper.InitVcomAndDataIntervalSetting()).MustNotHaveHappened();
		A.CallTo(() => _inkyGpioWrapper.InitGateSourceNonOverlapPeriod()).MustNotHaveHappened();
		A.CallTo(() => _inkyGpioWrapper.InitDisableExternalFlash()).MustNotHaveHappened();
		A.CallTo(() => _inkyGpioWrapper.InitUC8159_7C()).MustNotHaveHappened();
		A.CallTo(() => _inkyGpioWrapper.InitPowerOffSequence()).MustNotHaveHappened();
	}*/

	[Test]
	public async Task TestInitialize()
	{
		A.CallTo(() => _fakeGpioHelper.WaitForBusyPin(A<GpioPinName>.That.IsEqualTo(GpioPinName.Busy), A<TimeSpan>.That.IsEqualTo(TimeSpan.FromSeconds(40))))
			.Returns(Task.CompletedTask);

		await _inkyImpressionDriver.Initialize();

		A.CallTo(() => _fakeGpio.OpenPin(A<int>.That.IsEqualTo((int)GpioPinName.Cs0), A<PinMode>.That.IsEqualTo(PinMode.Output)))
			.MustHaveHappened()
			.Then(A.CallTo(() => _fakeGpio.Write(A<int>.That.IsEqualTo((int)GpioPinName.Cs0), A<PinValue>.That.IsEqualTo(PinValue.High)))
				.MustHaveHappened())
			.Then(A.CallTo(() => _fakeGpio.OpenPin(A<int>.That.IsEqualTo((int)GpioPinName.Dc), A<PinMode>.That.IsEqualTo(PinMode.Output)))
				.MustHaveHappened())
			.Then(A.CallTo(() => _fakeGpio.Write(A<int>.That.IsEqualTo((int)GpioPinName.Dc), A<PinValue>.That.IsEqualTo(PinValue.Low)))
				.MustHaveHappened())
			.Then(A.CallTo(() => _fakeGpio.OpenPin(A<int>.That.IsEqualTo((int)GpioPinName.Reset), A<PinMode>.That.IsEqualTo(PinMode.Output)))
				.MustHaveHappened())
			.Then(A.CallTo(() => _fakeGpio.Write(A<int>.That.IsEqualTo((int)GpioPinName.Reset), A<PinValue>.That.IsEqualTo(PinValue.High)))
				.MustHaveHappened())
			.Then(A.CallTo(() => _fakeGpio.OpenPin(A<int>.That.IsEqualTo((int)GpioPinName.Busy), A<PinMode>.That.IsEqualTo(PinMode.Input)))
				.MustHaveHappened())
			.Then(A.CallTo(() => _fakeGpioHelper.WaitForBusyPin(A<GpioPinName>.That.IsEqualTo(GpioPinName.Busy), A<TimeSpan>.That.IsEqualTo(TimeSpan.FromSeconds(40))))
				.MustHaveHappened());
			/*.Then(A.CallTo(() => _fakeSpiBus.OpenDevice(A<SpiConnectionSettings>.That.Matches(settings => settings.ChipSelectLine == (int)GpioPinName.Cs0)))
				.MustHaveHappened())
			.Then(A.CallTo(() => _fakeSpiBus.Write(A<byte[]>.That.IsSameSequenceAs(new byte[] { 0x00 })))
				.MustHaveHappened())
			.Then(A.CallTo(() => _fakeSpiBus.NoChipSelect())
				.MustHaveHappened())
			.Then(A.CallTo(() => _fakeSpiBus.MaxClockFrequency = 3000000)
				.MustHaveHappened())
			.Then(A.CallTo(() => _fakeGpio.Write(A<int>.That*/

		AssertInitPowerOffSequence();
		AssertInitPanelSettings();
		AssertInitPowerSettings();
		AssertInitSetPllClockFrequency();
		AssertInitSendTseRegisterToDisplay();
		AssertInitVcomAndDataIntervalSetting();
		AssertInitGateSourceNonOverlapPeriod();
		AssertInitDisableExternalFlash();
		AssertInitUC8159_7C();
		AssertInitPowerOffSequence();
	}

	[Test]
	public void PinHelper_DisplayButton_ButtonOne_MapsTo_GpioPinName_ButtonOne()
	{
		// Arrange
		var inputButton = DisplayButton.ButtonOne;

		// Act
		var result = PinHelper.mapButtonToPin(inputButton);

		// Assert
		Assert.That(result, Is.EqualTo(GpioPinName.ButtonOne));
	}

	[Test]
	public void PinHelper_DisplayButton_ButtonTwo_MapsTo_GpioPinName_ButtonTwo()
	{
		// Arrange
		var inputButton = DisplayButton.ButtonTwo;

		// Act
		var result = PinHelper.mapButtonToPin(inputButton);

		// Assert
		Assert.That(result, Is.EqualTo(GpioPinName.ButtonTwo));
	}

	[Test]
	public void PinHelper_DisplayButton_ButtonThree_MapsTo_GpioPinName_ButtonThree()
	{
		// Arrange
		var inputButton = DisplayButton.ButtonThree;

		// Act
		var result = PinHelper.mapButtonToPin(inputButton);

		// Assert
		Assert.That(result, Is.EqualTo(GpioPinName.ButtonThree));
	}

	[Test]
	public void PinHelper_DisplayButton_ButtonFour_MapsTo_GpioPinName_ButtonFour()
	{
		// Arrange
		var inputButton = DisplayButton.ButtonFour;

		// Act
		var result = PinHelper.mapButtonToPin(inputButton);

		// Assert
		Assert.That(result, Is.EqualTo(GpioPinName.ButtonFour));
	}
}
