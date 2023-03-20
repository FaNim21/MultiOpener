using System.IO;

namespace MultiOpener.Utils
{
    public static class Helper
    {
        /// <summary>
        /// Removing Json as extension from end of string
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Name of file from path as string</returns>
        public static string GetFileNameWithoutExtension(string? path)
        {
            string output = Path.GetFileName(path) ?? "";
            if (output.EndsWith(".json"))
                output = output.Remove(output.Length - 5);

            return output;
        }
    }
}
