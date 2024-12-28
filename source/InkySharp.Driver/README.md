# `InkySharp.Driver.InkyImpression`

This is an unofficial .NET library for the [Inky Impression 5.7" 7-color e-ink display](https://shop.pimoroni.com/products/inky-impression-5-7?variant=32298701324371) produced by Pimoroni. The library is largely reverse engineered from [Pimoroni's Python library](https://github.com/pimoroni/inky).

# Inky Impression

The only supported Inky display is the 5.7" display as this is the only hardware I currently have.

# Usage

```csharp
using var gpioController = new System.Device.Gpio.GpioController(System.Device.Gpio.PinNumberingScheme.Logical);
var gpioControllerWrapper = new InkySharp.Driver.GpioControllerWrapper.GpioControllerWrapper(gpioController);

var spiConnectionSettings = new System.Device.Spi.SpiConnectionSettings(busId: 0);
using var spiDevice = System.Device.Spi.SpiDevice.Create(spiConnectionSettings);
var spiDeviceWrapper = new InkySharp.Driver.InkiImpression.SpiDeviceWrapper.SpiDeviceWrapper(spiDevice);

IInkyImpressionWrapper inky = InkyImpressionWrapper.Create(
  isHorizontalFlipped: false,
  isVerticalFlipped: false,
  spiBus: spiDeviceWrapepr,
  gpio: gpioControllerWrapper);

var colors = inky.GetPaletteBlendFromSaturation(saturation: 0.7f)
  .Select(x => new SixLabors.ImageSharp.PixelFormats.Rgba32(packed: x))
  .Select(x => new SixLabors.ImageSharp.Color(x))
  .ToArray();

inky.SetBorderColor(InkySharp.Driver.InkyImpressionWrapper.DisplayColor.White);

const string imagePath = "image.jpg";
await using var imageStream = System.IO.File.OpenRead(imagePath);
using var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(imageStream, CancellationToken.None);

// Image mutations to fix rotation and resize while maintaining aspect ratio
image.Mutate(x => x
  .AutoOrient() // apply any orientation based on EXIF data
  .Resize(new SixLabors.ImageSharp.Processing.ResizeOptions
  {
    Mode = SixLabors.ImageSharp.Processing.ResizeMode.Max, // This will maintain the aspect ratio
    Size = new SixLabors.ImageSharp.Size(width: inky.Width, height: inky.Height),
  })
  .Contrast(1f);

// Dither the image using the available color palette
// This will alter the colors of the image so they are close to the colors available on the actual display.
var paletteQuantizer = new SixLabors.ImageSharp.Processing.Processors.Quantization.PaletteQuantizer(
  colors,
  new SixLabors.ImageSharp.Processing.Processors.Quantization.QuantizerOptions
  {
    MaxColors = colors.Length,
    DitherScale = 0.5f,
    Dither = SixLabors.ImageSharp.Processing.KnownDitherings.FloydSteinberg,
  });
image.Muate(x => x.Quantize(paletteQuantizer);

for (var x = 0; x < inky.Width; x++)
{
  for (var y = 0; y < inky.Height; y++)
  {
    var pixel = image[x, y];

    if (!FindClosestColor(pixel).Try(out var displayColor))
      throw new InvalidOperationException($"Could not find a display color for pixel {x},{y} with color {displayColor}.");

    inky.SetPixel(x, y, displayColor);
  }
}

await inky.Show();

// We need to map the colors from the image to the colors that the Inky Impression can display.
// This technique was adapted from https://github.com/KodeMunkie/inky-impression-slideshow/blob/0abc51cebab64f98aa7a2600c8183ce141c17c2e/image_processor.py#L5-L12
private static readonly IReadOnlyDictionary<Rgba32, DisplayColor> DisplayColors =
  new Dictionary<Rgba32, DisplayColor>
  {
    { new Rgba32(0x0c, 0x0c, 0x0e), DisplayColor.Black },
    { new Rgba32(0xd2, 0xd2, 0xd0), DisplayColor.White },
    { new Rgba32(0x1e, 0x60, 0x1f), DisplayColor.Green },
    { new Rgba32(0x1d, 0x1e, 0xaa), DisplayColor.Blue },
    { new Rgba32(0x8c, 0x1b, 0x1d), DisplayColor.Red },
    { new Rgba32(0xd3, 0xc9, 0x3d), DisplayColor.Yellow },
    { new Rgba32(0xc1, 0x71, 0x2a), DisplayColor.Orange },
  };

private static Maybe<DisplayColor> FindClosestColor(Rgba32 target)
{
  var minDistance = double.MaxValue;
  var closestDisplayColor = Maybe.Empty<DisplayColor>();

  foreach (var color in DisplayColors)
  {
    var distance = CalculateEuclideanDistance(target, color.Key);

    if (distance < minDistance)
    {
      minDistance = distance;
      closestDisplayColor = color.Value;
    }
  }

  return closestDisplayColor;

  static double CalculateEuclideanDistance(Rgba32 colorOne, Rgba32 colorTwo)
  {
    var deltaR = colorTwo.R - colorOne.R;
    var deltaG = colorTwo.G - colorOne.G;
    var deltaB = colorTwo.B - colorOne.B;

    return Math.Sqrt((deltaR * deltaR) + (deltaG * deltaG) + (deltaB * deltaB));
  }
}
```
