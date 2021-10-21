﻿#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Runtime.InteropServices;
using CafeLib.BsvSharp.Scripting;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Scripts
{
    public class KzScriptNumTests
    {
        Sntv[] tvs = new[] {
            new Sntv(0L, 0, ""),
            new Sntv(2L, 2, "02"),
            new Sntv(128L, 128, "0080"),
            new Sntv(256L, 256, "0100"),
            new Sntv(32768L, 32768, "008000"),
            new Sntv(65536L, 65536, "010000"),
            new Sntv(8388608L, 8388608, "00800000"),
            new Sntv(16777216L, 16777216, "01000000"),
            //new Sntv(2147483648L, 2147483647, "0080000000"),
            //new Sntv(4294967296L, 2147483647, "0100000000"),
            //new Sntv(549755813888L, 2147483647, "008000000000"),
            //new Sntv(1099511627776L, 2147483647, "010000000000"),
            new Sntv(0L, 0, ""),
            new Sntv(-2L, -2, "82"),
            new Sntv(-128L, -128, "8080"),
            new Sntv(-256L, -256, "8100"),
            new Sntv(-32768L, -32768, "808000"),
            new Sntv(-65536L, -65536, "810000"),
            new Sntv(-8388608L, -8388608, "80800000"),
            new Sntv(-16777216L, -16777216, "81000000"),
            //new Sntv(-2147483648L, -2147483648, "8080000000"),
            //new Sntv(-4294967296L, -2147483648, "8100000000"),
            //new Sntv(-549755813888L, -2147483648, "808000000000"),
            //new Sntv(-1099511627776L, -2147483648, "810000000000"),
            new Sntv(-1L, -1, "81"),
            new Sntv(1L, 1, "01"),
            new Sntv(127L, 127, "7f"),
            new Sntv(255L, 255, "00ff"),
            new Sntv(32767L, 32767, "7fff"),
            new Sntv(65535L, 65535, "00ffff"),
            new Sntv(8388607L, 8388607, "7fffff"),
            new Sntv(16777215L, 16777215, "00ffffff"),
            new Sntv(2147483647L, 2147483647, "7fffffff"),
            //new Sntv(4294967295L, 2147483647, "00ffffffff"),
            //new Sntv(549755813887L, 2147483647, "7fffffffff"),
            //new Sntv(1099511627775L, 2147483647, "00ffffffffff"),
            new Sntv(1L, 1, "01"),
            new Sntv(-1L, -1, "81"),
            new Sntv(-127L, -127, "ff"),
            new Sntv(-255L, -255, "80ff"),
            new Sntv(-32767L, -32767, "ffff"),
            new Sntv(-65535L, -65535, "80ffff"),
            new Sntv(-8388607L, -8388607, "ffffff"),
            new Sntv(-16777215L, -16777215, "80ffffff"),
            new Sntv(-2147483647L, -2147483647, "ffffffff"),
            //new Sntv(-4294967295L, -2147483648, "80ffffffff"),
            //new Sntv(-549755813887L, -2147483648, "ffffffffff"),
            //new Sntv(-1099511627775L, -2147483648, "80ffffffffff"),
            new Sntv(1L, 1, "01"),
            new Sntv(3L, 3, "03"),
            new Sntv(129L, 129, "0081"),
            new Sntv(257L, 257, "0101"),
            new Sntv(32769L, 32769, "008001"),
            new Sntv(65537L, 65537, "010001"),
            new Sntv(8388609L, 8388609, "00800001"),
            new Sntv(16777217L, 16777217, "01000001"),
            //new Sntv(2147483649L, 2147483647, "0080000001"),
            //new Sntv(4294967297L, 2147483647, "0100000001"),
            //new Sntv(549755813889L, 2147483647, "008000000001"),
            //new Sntv(1099511627777L, 2147483647, "010000000001"),
            new Sntv(-1L, -1, "81"),
            new Sntv(-3L, -3, "83"),
            new Sntv(-129L, -129, "8081"),
            new Sntv(-257L, -257, "8101"),
            new Sntv(-32769L, -32769, "808001"),
            new Sntv(-65537L, -65537, "810001"),
            new Sntv(-8388609L, -8388609, "80800001"),
            new Sntv(-16777217L, -16777217, "81000001"),
            //new Sntv(-2147483649L, -2147483648, "8080000001"),
            //new Sntv(-4294967297L, -2147483648, "8100000001"),
            //new Sntv(-549755813889L, -2147483648, "808000000001"),
            //new Sntv(-1099511627777L, -2147483648, "810000000001"),
        };
        
        [Theory]
        [InlineData(0L, 0, "")]
        [InlineData(2L, 2, "02")]
        [InlineData(4L, 4, "04")]
        [InlineData(16L, 16, "10")]
        [InlineData(128L, 128, "0080")]
        [InlineData(256L, 256, "0100")]
        [InlineData(32768L, 32768, "008000")]
        [InlineData(65536L, 65536, "010000")]
        [InlineData(8388608L, 8388608, "00800000")]
        [InlineData(16777216L, 16777216, "01000000")]
        public void Test_Power_Of_Two_Encoding(long v64, int v32, string hex)
        {
            var sn = new ScriptNum(v64);
            Assert.Equal(v64, sn.GetValue());
            Assert.Equal(v32, sn.GetInt());
            Assert.Equal(hex, sn.GetHex());
            sn = new ScriptNum(hex);
            Assert.Equal(v64, sn.GetValue());
            Assert.Equal(v32, sn.GetInt());
            Assert.Equal(hex, sn.GetHex());
        }
        
        [Theory]
        [InlineData(0L, 0, "")]
        [InlineData(-2L, -2, "82")]
        [InlineData(-128L, -128, "8080")]
        [InlineData(-256L, -256, "8100")]
        [InlineData(-32768L, -32768, "808000")]
        [InlineData(-65536L, -65536, "810000")]
        [InlineData(-8388608L, -8388608, "80800000")]
        [InlineData(-16777216L, -16777216, "81000000")]
        [InlineData(16777216L, 16777216, "01000000")]
        public void Test_Negative_Power_Of_Two_Encoding(long v64, int v32, string hex)
        {
            var sn = new ScriptNum(v64);
            Assert.Equal(v64, sn.GetValue());
            Assert.Equal(v32, sn.GetInt());
            Assert.Equal(hex, sn.GetHex());
            sn = new ScriptNum(hex);
            Assert.Equal(v64, sn.GetValue());
            Assert.Equal(v32, sn.GetInt());
            Assert.Equal(hex, sn.GetHex());
        }
        
        

        [Theory]
        [InlineData(-1L, -1, "81")]
        [InlineData(-127L, -127, "ff")]
        [InlineData(-255L, -255, "80ff")]
        [InlineData(-32767L, -32767, "ffff")]
        [InlineData(-65535L, -65535, "80ffff")]
        [InlineData(-8388607L, -8388607, "ffffff")]
        [InlineData(-16777215L, -16777215, "80ffffff")]
        [InlineData(-2147483647L, -2147483647, "ffffffff")]
        public void Test_High_Bit_Encoding(long v64, int v32, string hex)
        {
            var sn = new ScriptNum(v64);
            Assert.Equal(v64, sn.GetValue());
            Assert.Equal(v32, sn.GetInt());
            Assert.Equal(hex, sn.GetHex());
            sn = new ScriptNum(hex);
            Assert.Equal(v64, sn.GetValue());
            Assert.Equal(v32, sn.GetInt());
            Assert.Equal(hex, sn.GetHex());
        }
        
        [Theory]
        [InlineData(-1L, -1, "81")]
        [InlineData(-127L, -127, "ff")]
        [InlineData(-255L, -255, "80ff")]
        [InlineData(-32767L, -32767, "ffff")]
        [InlineData(-65535L, -65535, "80ffff")]
        [InlineData(-8388607L, -8388607, "ffffff")]
        [InlineData(-16777215L, -16777215, "80ffffff")]
        [InlineData(-2147483647L, -2147483647, "ffffffff")]
        public void Test_Low_Bit_Encoding(long v64, int v32, string hex)
        {
            var sn = new ScriptNum(v64);
            Assert.Equal(v64, sn.GetValue());
            Assert.Equal(v32, sn.GetInt());
            Assert.Equal(hex, sn.GetHex());
            sn = new ScriptNum(hex);
            Assert.Equal(v64, sn.GetValue());
            Assert.Equal(v32, sn.GetInt());
            Assert.Equal(hex, sn.GetHex());
        }
        
        
        [Theory]
        [InlineData(-3L, -3, "83")]
        [InlineData(-129L, -129, "8081")]
        [InlineData(-257L, -257, "8101")]
        [InlineData(-32769L, -32769, "808001")]
        [InlineData(-65537L, -65537, "810001")]
        [InlineData(-8388609L, -8388609, "80800001")]
        [InlineData(-16777217L, -16777217, "81000001")]
        public void Test_Negative_Encoding(long v64, int v32, string hex)
        {
            var sn = new ScriptNum(v64);
            Assert.Equal(v64, sn.GetValue());
            Assert.Equal(v32, sn.GetInt());
            Assert.Equal(hex, sn.GetHex());
            sn = new ScriptNum(hex);
            Assert.Equal(v64, sn.GetValue());
            Assert.Equal(v32, sn.GetInt());
            Assert.Equal(hex, sn.GetHex());
        }

        [Fact]
        public void TestValueEncoding()
        {
            foreach (var tv in tvs) {
                var sn = new ScriptNum(tv.v64);
                Assert.Equal(tv.v64, sn.GetValue());
                Assert.Equal(tv.v32, sn.GetInt());
                Assert.Equal(tv.hex, sn.GetHex());
                sn = new ScriptNum(tv.hex);
                Assert.Equal(tv.v64, sn.GetValue());
                Assert.Equal(tv.v32, sn.GetInt());
                Assert.Equal(tv.hex, sn.GetHex());
            }
        }
    }

    class Sntv
    {
        public Int64 v64;
        public Int32 v32;
        public string hex;
        public Sntv(Int64 v64, Int32 v32, string hex)
        {
            this.v64 = v64;
            this.v32 = v32;
            this.hex = hex;
        }

        public override string ToString()
        {
            return $"{v64}L, {v32}, \"{hex}\"";
        }
    }
}
