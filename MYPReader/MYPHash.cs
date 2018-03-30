using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;


namespace Csharp.WorldBuilder
{
   public class MYPHash
   {
      internal static Dictionary<string, Asset> _Assets = new Dictionary<string, Asset>();
      internal static Dictionary<long, List<MythicPackage>> _Hashes = new Dictionary<long, List<MythicPackage>>();
      internal static Dictionary<long, string> _DeHash = new Dictionary<long, string>();
      internal static string _WarFolder;

      public static string WarFolder { get { return _WarFolder; } set { _WarFolder = value; } }

      public static String[] AssetNames
      {
         get { return InteropUtils.GetStringKeys(_Assets); }
      }

      public static Asset getAsset(string name)
      {
         return _Assets[name];
      }

      public static MemoryStream UnzipFromStream(Stream zipStream)
      {

         ZipInputStream zipInputStream = new ZipInputStream(zipStream);
         ZipEntry zipEntry = zipInputStream.GetNextEntry();
         while (zipEntry != null)
         {


            byte[] buffer = new byte[4096];     // 4K is optimum

            MemoryStream ms = new MemoryStream();
            StreamUtils.Copy(zipInputStream, ms, buffer);
            zipEntry = zipInputStream.GetNextEntry();
            return ms;
         }

         return null;
      }

      public static void Intialize(string warfolder, Stream hashes, ILog log)
      {
         MemoryStream ms = UnzipFromStream(hashes);
         ms.Position = 0;
         GetDehashes(ms);
         _Hashes = GetMythicHashList(warfolder);
      }

      public static Dictionary<long, string> GetDehashes(Stream stream)
      {
         StreamReader reader = new StreamReader(stream);
         Dictionary<long, string> deHashedCache = new Dictionary<long, string>();

         while (!reader.EndOfStream)
         {
            string line = reader.ReadLine();
            long hash1 = long.Parse(line.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
            long hash2 = long.Parse(line.Substring(9, 8), System.Globalization.NumberStyles.HexNumber);
            long key = ((long)hash1 << 32) + hash2;

            string[] tokens = line.Split(new char[] { '#' });
            deHashedCache[key] = tokens[2];
         }

         foreach (string hash in deHashedCache.Values)
         {
            string[] tokens = hash.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length > 0)
               BuildPath(new List<string>(tokens));
         }

         return deHashedCache;

      }
      public static byte[] GetAssetData(string warfolder, long key, int mypIndex)
      {
         byte[] data = null;
         if (MYPHash._Hashes.ContainsKey(key))
         {
            var myp = new MYP();
            using (var stream = new FileStream(Path.Combine(warfolder, _Hashes[key][mypIndex].ToString() + ".myp"), FileMode.Open, FileAccess.ReadWrite))
            {
               myp.Load(stream);
               var mypFile = myp.GetByHash(key);
               if (mypFile != null)
               {
                  data = myp.ReadFile(stream, mypFile);
               }

            }
         }

         return data;
      }
      public static byte[] GetAssetData(string warfolder, Asset asset, int mypIndex)
      {
         string path = asset.Path;
         uint ph = 0, sh = 0;
         MYP.HashWAR(path, 0xDEADBEEF, out ph, out sh);
         long key = ((long)ph << 32) + sh;
         byte[] data = null;
         if (MYPHash._Hashes.ContainsKey(key))
         {
            var myp = new MYP();
            using (var stream = new FileStream(Path.Combine(warfolder, _Hashes[key][mypIndex].ToString() + ".myp"), FileMode.Open, FileAccess.ReadWrite))
            {
               myp.Load(stream);
               var mypFile = myp.GetByFilename(asset.Path);
               if (mypFile != null)
               {
                  data = myp.ReadFile(stream, mypFile);
               }

            }
         }

         return data;
      }

      private static Asset BuildPath(List<string> tokens, Asset parent = null)
      {
         if (parent == null)
         {
            if (!_Assets.ContainsKey(tokens[0]))
            {
               _Assets[tokens[0]] = new Asset()
               {
                  Name = tokens[0]
               };
            }
            parent = _Assets[tokens[0]];
            _DeHash[MYP.HashWAR(parent.Path)] = parent.Path;

            BuildPath(tokens.GetRange(1, tokens.Count - 1), parent);
         }
         else
         {
            if (tokens.Count > 1)
            {
               Asset asset = null;
               if (!parent._Assets.ContainsKey(tokens[0]))
               {
                  parent._Assets[tokens[0]] = new Asset()
                  {
                     Name = tokens[0]
                  };
                  asset = parent._Assets[tokens[0]];
                  asset.Parent = parent;
                  _DeHash[MYP.HashWAR(asset.Path)] = asset.Path;
               }
               else
               {
                  asset = parent._Assets[tokens[0]];
               }

               BuildPath(tokens.GetRange(1, tokens.Count - 1), asset);
            }
            else
            {
               parent._Assets[tokens[0]] = new Asset()
               {
                  Name = tokens[0],
                  Parent = parent
               };
               var asset = parent._Assets[tokens[0]];

               _DeHash[MYP.HashWAR(asset.Path)] = asset.Path;
            }
         }
         return null;
      }
      public static void PrintStats(ILog log)
      {
         //log.Append("MYP Hashes Counts");
         //foreach (var p in Hashes.Keys)
         //{
         //    log.Append(" |--" + p.ToString() + " = " + Hashes[p].Count.ToString());
         //}
         //log.Append("");
         //  log.Append("De-Hashes = " + DeHashedCache.Count.ToString());
      }

      public static void SaveAsset(string warfolder, Asset asset, byte[] data, int mypIndex)
      {
         List<MythicPackage> p = _Hashes[MYP.HashWAR(asset.Path)];
         using (var stream = new FileStream(Path.Combine(warfolder, p[mypIndex].ToString() + ".myp"), FileMode.Open, FileAccess.ReadWrite))
         {
            var myp = new MYP();
            myp.Load(stream);
            var mypFile = myp.GetByFilename(asset.Path);
            if (mypFile != null)
            {
               myp.WriteFile(stream, mypFile, data);
            }

         }

      }
      public static Dictionary<long, List<MythicPackage>> GetMythicHashList(string warfolder)
      {

         Dictionary<long, List<MythicPackage>> results = new Dictionary<long, List<MythicPackage>>();

         foreach (string file in Directory.GetFiles(warfolder, "*.MYP"))
         {
            MythicPackage p;
            if (Enum.TryParse<MythicPackage>(Path.GetFileNameWithoutExtension(file).ToUpper(), out p))
            {

               var myp = new MYP();
               using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
               {
                  myp.Load(stream);
                  foreach (long key in myp.Files.Keys)
                  {
                     if (!results.ContainsKey(key))
                        results[key] = new List<MythicPackage>();
                     results[key].Add(p);
                  }
               }
            }
         }

         return results;
      }
   }
}