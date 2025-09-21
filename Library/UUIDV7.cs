using System.Security.Cryptography;

namespace HostTool.Library
{
    public static class UUIDV7
    {
        public static Guid NewUuid7()
        {
            Span<byte> bytes = stackalloc byte[16];
            var unixTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Copy timestamp vào 48 bit đầu
            bytes[0] = (byte)((unixTimeMs >> 40) & 0xFF);
            bytes[1] = (byte)((unixTimeMs >> 32) & 0xFF);
            bytes[2] = (byte)((unixTimeMs >> 24) & 0xFF);
            bytes[3] = (byte)((unixTimeMs >> 16) & 0xFF);
            bytes[4] = (byte)((unixTimeMs >> 8) & 0xFF);
            bytes[5] = (byte)(unixTimeMs & 0xFF);

            // Sinh random cho phần còn lại
            RandomNumberGenerator.Fill(bytes.Slice(6));

            // Set version = 7
            bytes[6] &= 0x0F;
            bytes[6] |= 0x70;

            // Set variant = RFC 4122
            bytes[8] &= 0x3F;
            bytes[8] |= 0x80;

            return new Guid(bytes);
        }
    }
}
