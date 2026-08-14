namespace RestaurantMenuPlatform.Infrastructure.Storage;

internal static class ImageContentValidator
{
    public const long DefaultMaxBytes = 5 * 1024 * 1024;

    public static void ValidateFileName(string originalFileName, string contentType)
    {
        var normalizedContentType = contentType.Trim().ToLowerInvariant();
        var fileName = Path.GetFileName(originalFileName);
        if (string.IsNullOrWhiteSpace(originalFileName) ||
            string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(fileName, originalFileName, StringComparison.Ordinal) ||
            fileName.Contains('\0') ||
            fileName.Contains('/') ||
            fileName.Contains('\\'))
            throw new ArgumentException("The image filename is not safe.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var allowed = normalizedContentType switch
        {
            "image/jpeg" => extension is ".jpg" or ".jpeg",
            "image/png" => extension == ".png",
            "image/webp" => extension == ".webp",
            _ => false
        };
        if (!allowed)
            throw new ArgumentException("The image filename extension does not match its content type.");
    }

    public static async Task<MemoryStream> BufferAndValidateAsync(
        Stream content,
        string contentType,
        long length,
        long? configuredMaxBytes,
        CancellationToken cancellationToken)
    {
        var maxBytes = configuredMaxBytes is > 0 ? configuredMaxBytes.Value : DefaultMaxBytes;
        var normalizedContentType = contentType.Trim().ToLowerInvariant();
        if (normalizedContentType is not ("image/jpeg" or "image/png" or "image/webp"))
            throw new ArgumentException("This file type is not supported. Use JPEG, PNG, or WebP.");
        if (length <= 0 || length > maxBytes)
            throw new ArgumentException($"The image exceeds the maximum allowed size of {maxBytes / (1024 * 1024)} MB or is empty.");

        var buffered = new MemoryStream(capacity: checked((int)Math.Min(length, int.MaxValue)));
        await content.CopyToAsync(buffered, cancellationToken);
        if (buffered.Length <= 0 || buffered.Length > maxBytes || buffered.Length != length)
        {
            buffered.Dispose();
            throw new ArgumentException("The uploaded image size is invalid or the file is empty.");
        }

        buffered.Position = 0;
        var header = new byte[12];
        var headerLength = await buffered.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        buffered.Position = 0;
        if (!HasValidSignature(header, headerLength, normalizedContentType) || !HasValidStructure(buffered, normalizedContentType))
        {
            buffered.Dispose();
            throw new ArgumentException("The image file could not be processed because it is corrupted or not a valid image.");
        }

        // Structure inspection seeks through the buffer. Always return it ready
        // for the storage provider to copy from the beginning.
        buffered.Position = 0;
        return buffered;
    }

    public static string ExtensionFor(string contentType) => contentType.Trim().ToLowerInvariant() switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => throw new ArgumentException("Unsupported image type.")
    };

    private static bool HasValidSignature(byte[] header, int length, string contentType) => contentType switch
    {
        "image/png" => length >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
        "image/jpeg" => length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
        "image/webp" => length >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
        _ => false
    };

    private static bool HasValidStructure(Stream stream, string contentType)
    {
        if (contentType == "image/png")
        {
            if (stream.Length < 33)
                return false;
            var header = new byte[33];
            stream.Position = 0;
            _ = stream.Read(header, 0, header.Length);
            var width = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
            var height = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
            var hasIhdr = header.AsSpan(12, 4).SequenceEqual("IHDR"u8);
            var hasIend = false;
            stream.Position = Math.Max(0, stream.Length - 16);
            var tail = new byte[16];
            var read = stream.Read(tail, 0, tail.Length);
            for (var i = 0; i <= read - 4; i++)
                hasIend |= tail.AsSpan(i, 4).SequenceEqual("IEND"u8);
            return width > 0 && height > 0 && hasIhdr && hasIend;
        }

        if (contentType == "image/webp")
        {
            if (stream.Length < 16)
                return false;
            var bytes = new byte[(int)Math.Min(stream.Length, 4096)];
            stream.Position = 0;
            var read = stream.Read(bytes, 0, bytes.Length);
            return read >= 16 && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
                   bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8) &&
                   (bytes.AsSpan(12, read - 12).IndexOf("VP8 "u8) >= 0 ||
                    bytes.AsSpan(12, read - 12).IndexOf("VP8L"u8) >= 0 ||
                    bytes.AsSpan(12, read - 12).IndexOf("VP8X"u8) >= 0);
        }

        if (stream.Length < 4)
            return false;
        stream.Position = stream.Length - 2;
        var end = new byte[2];
        _ = stream.Read(end, 0, 2);
        return end[0] == 0xFF && end[1] == 0xD9;
    }
}
