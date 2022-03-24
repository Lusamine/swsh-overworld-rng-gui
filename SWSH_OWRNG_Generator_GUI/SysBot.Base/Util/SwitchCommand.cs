﻿// From https://github.com/berichan/PLAWarper
// AGPL-3.0 License

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SysBot.Base
{
    /// <summary>
    /// Encodes commands to be sent as a <see cref="byte"/> array to a Nintendo Switch running a sys-module.
    /// </summary>
    public static class SwitchCommand
    {
        private static readonly Encoding Encoder = Encoding.ASCII;

        private static byte[] Encode(string command, bool crlf = true)
        {
            if (crlf)
                command += "\r\n";
            return Encoder.GetBytes(command);
        }

        /// <summary>
        /// Removes the virtual controller from the bot. Allows physical controllers to control manually.
        /// </summary>
        /// <returns>Encoded command bytes</returns>
        public static byte[] DetachController(bool crlf = true) => Encode("detachController", crlf);

        /// <summary>
        /// Configures the sys-botbase parameter to the specified value.
        /// </summary>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Configure(SwitchConfigureParameter p, int ms, bool crlf = true) => Encode($"configure {p} {ms}", crlf);

        /* 
         *
         * Controller Button Commands
         *
         */

        /* 
         *
         * Memory I/O Commands
         *
         */

        /// <summary>
        /// Requests the Bot to send <see cref="count"/> bytes from <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Address of the data</param>
        /// <param name="count">Amount of bytes</param>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Peek(uint offset, int count, bool crlf = true) => Encode($"peek 0x{offset:X8} {count}", crlf);

        /// <summary>
        /// Requests the Bot to send concat bytes from offsets of sizes in the <see cref="offsetSizeDictionary"/> relative to the heap.
        /// </summary>
        /// <param name="offsetSizeDictionary">Dictionary of offset and sizes to be looked up</param>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PeekMulti(IReadOnlyDictionary<ulong, int> offsetSizeDictionary, bool crlf = true) => Encode($"peekMulti{string.Concat(offsetSizeDictionary.Select(z => $" 0x{z.Key:X16} {z.Value}"))}", crlf);

        /// <summary>
        /// Sends the Bot <see cref="data"/> to be written to <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Address of the data</param>
        /// <param name="data">Data to write</param>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Poke(uint offset, byte[] data, bool crlf = true) => Encode($"poke 0x{offset:X8} 0x{string.Concat(data.Select(z => $"{z:X2}"))}", crlf);

        /// <summary>
        /// Requests the Bot to send <see cref="count"/> bytes from absolute <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Absolute address of the data</param>
        /// <param name="count">Amount of bytes</param>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PeekAbsolute(ulong offset, int count, bool crlf = true) => Encode($"peekAbsolute 0x{offset:X16} {count}", crlf);

        /// <summary>
        /// Requests the Bot to send concat bytes from offsets of sizes in the <see cref="offsetSizeDictionary"/> in absolute space.
        /// </summary>
        /// <param name="offsetSizeDictionary">Dictionary of offset and sizes to be looked up</param>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PeekAbsoluteMulti(IReadOnlyDictionary<ulong, int> offsetSizeDictionary, bool crlf = true) => Encode($"peekAbsoluteMulti{string.Concat(offsetSizeDictionary.Select(z => $" 0x{z.Key:X16} {z.Value}"))}", crlf);

        /// <summary>
        /// Sends the Bot <see cref="data"/> to be written to absolute <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Absolute address of the data</param>
        /// <param name="data">Data to write</param>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PokeAbsolute(ulong offset, byte[] data, bool crlf = true) => Encode($"pokeAbsolute 0x{offset:X16} 0x{string.Concat(data.Select(z => $"{z:X2}"))}", crlf);

        /// <summary>
        /// Requests the Bot to send <see cref="count"/> bytes from main <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Main NSO address of the data</param>
        /// <param name="count">Amount of bytes</param>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PeekMain(ulong offset, int count, bool crlf = true) => Encode($"peekMain 0x{offset:X16} {count}", crlf);

        /// <summary>
        /// Requests the Bot to send concat bytes from offsets of sizes in the <see cref="offsetSizeDictionary"/> relative to the main region.
        /// </summary>
        /// <param name="offsetSizeDictionary">Dictionary of offset and sizes to be looked up</param>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PeekMainMulti(IReadOnlyDictionary<ulong, int> offsetSizeDictionary, bool crlf = true) => Encode($"peekMainMulti{string.Concat(offsetSizeDictionary.Select(z => $" 0x{z.Key:X16} {z.Value}"))}", crlf);

        /// <summary>
        /// Sends the Bot <see cref="data"/> to be written to main <see cref="offset"/>.
        /// </summary>
        /// <param name="offset">Main NSO address of the data</param>
        /// <param name="data">Data to write</param>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PokeMain(ulong offset, byte[] data, bool crlf = true) => Encode($"pokeMain 0x{offset:X16} 0x{string.Concat(data.Select(z => $"{z:X2}"))}", crlf);

        /*
         * 
         * Pointer Commands
         * 
         */

        /// <summary>
        /// Requests the Bot to send <see cref="count"/> bytes from pointer traversals defined by <see cref="jumps"/>
        /// </summary>
        /// <param name="jumps">All traversals in the pointer expression</param>
        /// <param name="count">Amount of bytes</param>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PointerPeek(IEnumerable<long> jumps, int count, bool crlf = true) => Encode($"pointerPeek {count}{string.Concat(jumps.Select(z => $" {z}"))}", crlf);

        /// <summary>
        /// Sends the Bot <see cref="data"/> to be written to the offset at the end pointer traversals defined by <see cref="jumps"/>.
        /// </summary>
        /// <param name="jumps">All traversals in the pointer expression</param>
        /// <param name="data">Data to write</param>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PointerPoke(IEnumerable<long> jumps, byte[] data, bool crlf = true) => Encode($"pointerPoke 0x{string.Concat(data.Select(z => $"{z:X2}"))}{string.Concat(jumps.Select(z => $" {z}"))}", crlf);

        /// <summary>
        /// Requests the Bot to solve the pointer traversals defined by <see cref="jumps"/> and send the final absolute offset.
        /// </summary>
        /// <param name="jumps">All traversals in the pointer expression</param>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PointerAll(IEnumerable<long> jumps, bool crlf = true) => Encode($"pointerAll{string.Concat(jumps.Select(z => $" {z}"))}", crlf);

        /// <summary>
        /// Requests the Bot to solve the pointer traversals defined by <see cref="jumps"/> and send the final offset relative to the heap region.
        /// </summary>
        /// <param name="jumps">All traversals in the pointer expression</param>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] PointerRelative(IEnumerable<long> jumps, bool crlf = true) => Encode($"pointerRelative{string.Concat(jumps.Select(z => $" {z}"))}", crlf);

        /* 
         *
         * Process Info Commands
         *
         */

        /// <summary>
        /// Requests the main NSO base of attached process.
        /// </summary>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] GetMainNsoBase(bool crlf = true) => Encode("getMainNsoBase", crlf);

        /// <summary>
        /// Requests the heap base of attached process.
        /// </summary>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] GetHeapBase(bool crlf = true) => Encode("getHeapBase", crlf);

        /// <summary>
        /// Requests the title id of attached process.
        /// </summary>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] GetTitleID(bool crlf = true) => Encode("getTitleID", crlf);

        /// <summary>
        /// Requests the build id of attached process.
        /// </summary>
        /// <param name="crlf">Line terminator (unused by USB protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] GetBuildID(bool crlf = true) => Encode("getBuildID", crlf);

        /// Advances system network clock to advance time by one day at a time.
        /// </summary>
        /// <param name="resetAfterNSkips">Day advance amount after which we should reset time to initial time (0 if we shouldn't)</param>
        /// <param name="resetNTP">Should we use NTP to sync back time (1/0 for true/false, about to be deprecated)</param>
        /// <param name="crlf">Line terminator (unused by USB's protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] DaySkip(bool crlf = true) => Encode($"daySkip", crlf);

        /// <summary>
        /// Sync system network clock with NTP's server.
        /// </summary>
        /// <param name="crlf">Line terminator (unused by USB's protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] ResetTimeNTP(bool crlf = true) => Encode("resetTimeNTP", crlf);

        /// <summary>
        /// Sync system network clock with the initial day skip's clock.
        /// </summary>
        /// <param name="crlf">Line terminator (unused by USB's protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] ResetTime(bool crlf = true) => Encode("resetTime", crlf);

        /// <summary>
        /// Takes and sends a raw screenshot.
        /// </summary>
        /// <param name="crlf">Line terminator (unused by USB's protocol)</param>
        /// <returns>Encoded command bytes</returns>
        public static byte[] Screengrab(bool crlf = true) => Encode("pixelPeek", crlf);
    }
}
