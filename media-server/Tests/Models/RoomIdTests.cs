using MediaServer.Core.Models;
using System.Collections.Generic;
using Xunit;

namespace Tests.Models
{
    public class RoomIdTests
    {
        [Theory]
        [InlineData("my channel", "my-channel")]
        [InlineData("MY_CHANNEL", "my-channel")]
        [InlineData("m", "m")]
        [InlineData("m ", "m")]
        [InlineData(" m", "m")]
        [InlineData(" m ", "m")]
        public void FromString_ValidInput_OnlyValidOnesAccepted(string input, string expected)
        {
            Assert.Equal(RoomId.FromString(input).ToString(), expected);
        }

        [Fact]
        public void CanBeUsedInADictionary()
        {
            var tmp = new Dictionary<RoomId, object>();

            tmp[RoomId.FromString("My Channel")] = true;

            Assert.True(tmp.ContainsKey(RoomId.FromString("my channel")));
            Assert.True(tmp.ContainsKey(RoomId.FromString("my_channel")));
            Assert.True(tmp.ContainsKey(RoomId.FromString("my-channel")));
            Assert.True(tmp.ContainsKey(RoomId.FromString(" my-channel ")));
            Assert.False(tmp.ContainsKey(RoomId.FromString("other channel")));
        }
    }
}
