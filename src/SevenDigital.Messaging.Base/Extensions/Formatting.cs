namespace SevenDigital.Messaging.Base
{
	/// <summary>
	/// Helper for formatting file sizes
	/// </summary>
	public static class Formatting
	{
		/// <summary>
		/// Render a size in byte to a human readable string.
		/// </summary>
		public static string ReadableFileSize(double size, int unit = 0)
		{
			string[] units = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };

			while (size >= 1024)
			{
				size /= 1024;
				++unit;
			}

			return string.Format("{0:G4} {1}", size, units[unit]);
		}
	}
}