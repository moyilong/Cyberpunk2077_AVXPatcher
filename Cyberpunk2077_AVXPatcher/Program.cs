﻿using System;
using System.IO;
using System.Reflection;

namespace Cyberpunk2077_AVXPatcher
{
	public static class Program
	{
        static void Main(string[] args)
        {
			string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string cp77exe = "Cyberpunk2077.exe";
			if (File.Exists(cp77exe + ".bak"))
			{
				Console.WriteLine("Backup found. Already patched?");
				Console.ReadKey();
			}
			File.Copy(assemblyPath + Path.DirectorySeparatorChar + cp77exe, assemblyPath + Path.DirectorySeparatorChar + cp77exe + ".bak", false);
			Console.WriteLine("Backup created");
            byte[] sourceBytes = StringHexToByteArray("554881ECA00000000F2970E8");
            byte[] targetBytes = StringHexToByteArray("C34881ECA00000000F2970E8");
            BinaryReplace(cp77exe + ".bak", sourceBytes, cp77exe, targetBytes);
            Console.WriteLine("AVX Pattern found and replaced. Cyberpunk 2077 patched successful.");
            Console.ReadKey();
        }

        public static void BinaryReplace(string sourceFile, byte[] sourceSeq, string targetFile, byte[] targetSeq)
        {
            FileStream sourceStream = File.OpenRead(sourceFile);
            FileStream targetStream = File.Create(targetFile);

            try
            {
                int b;
                long foundSeqOffset = -1;
                int searchByteCursor = 0;

                while ((b = sourceStream.ReadByte()) != -1)
                {
                    if (sourceSeq[searchByteCursor] == b)
                    {
                        if (searchByteCursor == sourceSeq.Length - 1)
                        {
                            targetStream.Write(targetSeq, 0, targetSeq.Length);
                            searchByteCursor = 0;
                            foundSeqOffset = -1;
                        }
                        else
                        {
                            if (searchByteCursor == 0)
                            {
                                foundSeqOffset = sourceStream.Position - 1;
                            }

                            ++searchByteCursor;
                        }
                    }
                    else
                    {
                        if (searchByteCursor == 0)
                        {
                            targetStream.WriteByte((byte)b);
                        }
                        else
                        {
                            targetStream.WriteByte(sourceSeq[0]);
                            sourceStream.Position = foundSeqOffset + 1;
                            searchByteCursor = 0;
                            foundSeqOffset = -1;
                        }
                    }
                }
            }
            finally
            {
                sourceStream.Dispose();
                targetStream.Dispose();
            }
        }

        public static byte[] StringHexToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
