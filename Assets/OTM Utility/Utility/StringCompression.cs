using System.IO;
using System.IO.Compression;
using System.Text;

namespace com.otm.utility
{
	public static class StringCompression
	{
		public static byte[] CompressString(this string text)
		{
			byte[] inputBytes = Encoding.UTF8.GetBytes(text);
			using (var outputStream = new MemoryStream())
			{
				using (var gzip = new GZipStream(outputStream, System.IO.Compression.CompressionLevel.Optimal))
				{
					gzip.Write(inputBytes, 0, inputBytes.Length);
				}
				return outputStream.ToArray();
			}
		}

		public static string DecompressString(this byte[] compressedData)
		{
			using (var inputStream = new MemoryStream(compressedData))
			using (var gzip = new GZipStream(inputStream, CompressionMode.Decompress))
			using (var outputStream = new MemoryStream())
			{
				gzip.CopyTo(outputStream);
				byte[] outputBytes = outputStream.ToArray();
				return Encoding.UTF8.GetString(outputBytes);
			}
		}
	}
}
