namespace InkySharp.Driver.SpiDeviceWrapper

open System
open System.Device.Spi
open System.Diagnostics.CodeAnalysis

type ISpiDeviceWrapper =
    abstract member ConnectionSettings : SpiConnectionSettings with get
    abstract member Read : buffer : byte array -> unit
    abstract member ReadByte : unit -> byte
    abstract member TransferFullDuplex : writeBuffer : byte array * readBuffer : byte array -> unit
    // NOTE: The Write method here is a byte array instead of a ReadOnlySpan<byte> because of a crazy bug I ran into when unit testing.
    // When I used a ReadOnlySpan<byte> here, the unit test would fail with an InvalidProgramException when the
    // mocking libraries tried to proxy this call. I have no idea why.
    // Luckily there's an implicit conversion from byte array to ReadOnlySpan<byte> so it's not a big deal. But it's still weird.
    abstract member Write : buffer : byte array -> unit
    abstract member WriteByte : value : byte -> unit
    abstract member Dispose : unit -> unit

[<ExcludeFromCodeCoverage>]
type SpiDeviceWrapper (spiDevice : SpiDevice) =
    interface ISpiDeviceWrapper with

        member this.ConnectionSettings with get() = spiDevice.ConnectionSettings
        member this.Read (buffer : byte array) = spiDevice.Read(buffer)
        member this.ReadByte () = spiDevice.ReadByte ()
        member this.TransferFullDuplex (writeBuffer : byte array, readBuffer : byte array) =
            spiDevice.TransferFullDuplex (writeBuffer, readBuffer)
        member this.Write (buffer : byte array) = spiDevice.Write buffer
        member this.WriteByte (value : byte) = spiDevice.WriteByte value
        member this.Dispose () = spiDevice.Dispose()
