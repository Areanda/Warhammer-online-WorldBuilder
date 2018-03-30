using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorldBuilder
{
    public class MYPHash
    {
        public static Dictionary<string, Asset> Assets = new Dictionary<string, Asset>();
        public static Dictionary<long, List<MythicPackage>> Hashes = new Dictionary<long, List<MythicPackage>>();
        public static Dictionary<long, string> DeHash = new Dictionary<long, string>();
        public class Asset
        {
            public string Name { get; set; }
            public string Filename { get; set; }
            public MythicPackage Packet { get; set; }
            public long Hash { get; set; }
            public Asset Parent { get; set; }
            public Dictionary<string, Asset> Assets = new Dictionary<string, Asset>();
            public override string ToString()
            {
                return Name;
            }

            public string DeHashedName
            {
                get
                {
                    if(DeHash.ContainsKey(Hash))
                        return DeHash[Hash];
                    return "";
                }
            }
            public FileInArchive File { get; set; }

            public FileInArchiveDescriptor Info
            {
                get
                {
                    if (Filename == null)
                    {
                        if (Hashes.ContainsKey(MYP.HashWAR(Path)))
                        {
                            List<MythicPackage> p = Hashes[MYP.HashWAR(Path)];
                            using (var stream = new FileStream(System.IO.Path.Combine(frmBuilder.WarFolder, p[0].ToString() + ".myp"), FileMode.Open, FileAccess.ReadWrite))
                            {
                                var myp = new MYP();
                                myp.Load(stream);
                                var mypFile = myp.GetByFilename(Path);
                                return mypFile.Descriptor;
                            }
                        }
                    }
                    else if (File != null)
                        return File.Descriptor;


                    return null;

                }
                set
                {

                }

            }
            public string Path
            {
                get
                {
                    string path = "";
                    Asset current = this;

                    while (current != null)
                    {
                        path = current.Name + "/" + path;
                        current = current.Parent;
                    }
                    return path.Remove(path.Length-1);
                }
            }

            public string AppearsIn
            {
                get
                {
                    string result = "";
                    long hash = MYP.HashWAR(Path);
                    if (Hashes.ContainsKey(hash))
                    {
                        foreach (var p in Hashes[MYP.HashWAR(Path)])
                        {
                            result += p.ToString() + " ";
                        }
                    }
                    return result.Trim();
                }
            }
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

        public static async Task Intialize(string warfolder, Stream hashes, ILog log)
        {
            MemoryStream ms = UnzipFromStream(hashes);
            ms.Position = 0;
            await GetDehashes(ms);
            Hashes = await GetMythicHashList(warfolder);
        }

        public static async Task<Dictionary<long, string>> GetDehashes(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            Dictionary<long, string> deHashedCache = new Dictionary<long, string>();
            await Task.Run(() =>
             {
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
                    if(tokens.Length > 0)
                        BuildPath(new List<string>(tokens));
                 }
             });

            return deHashedCache;

        }
        public static byte[] GetAssetData(string warfolder, long key, int mypIndex)
        {
            byte[] data = null;
            if (MYPHash.Hashes.ContainsKey(key))
            {
                var myp = new MYP();
                using (var stream = new FileStream(Path.Combine(warfolder, Hashes[key][mypIndex].ToString() + ".myp"), FileMode.Open, FileAccess.ReadWrite))
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
            if (MYPHash.Hashes.ContainsKey(key))
            {
                var myp = new MYP();
                using (var stream = new FileStream(Path.Combine(warfolder, Hashes[key][mypIndex].ToString() + ".myp"), FileMode.Open, FileAccess.ReadWrite))
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

        private static Asset BuildPath( List<string> tokens, Asset parent = null)
        {
            if (parent == null)
            {
                if (!Assets.ContainsKey(tokens[0]))
                {
                    Assets[tokens[0]] = new Asset()
                    {
                        Name = tokens[0]
                    };
                }
                parent = Assets[tokens[0]];
                DeHash[MYP.HashWAR(parent.Path)] = parent.Path;

                BuildPath(tokens.GetRange(1, tokens.Count - 1), parent);
            }
            else
            {
                if (tokens.Count > 1)
                {
                    Asset asset = null;
                    if (!parent.Assets.ContainsKey(tokens[0]))
                    {
                        parent.Assets[tokens[0]] = new Asset()
                        {
                            Name = tokens[0]
                        };
                        asset = parent.Assets[tokens[0]];
                        asset.Parent = parent;
                        DeHash[MYP.HashWAR(asset.Path)] = asset.Path;
                    }
                    else
                    {
                        asset = parent.Assets[tokens[0]];
                    }
                 
                    BuildPath(tokens.GetRange(1, tokens.Count - 1), asset);
                }
                else
                {
                    parent.Assets[tokens[0]] = new Asset()
                    {
                        Name = tokens[0],
                        Parent = parent
                    };
                    var asset = parent.Assets[tokens[0]];

                    DeHash[MYP.HashWAR(asset.Path)] = asset.Path;
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

        public static async Task SaveAsset(string warfolder, Asset asset, byte[] data, int mypIndex)
        {
            List<MythicPackage> p = Hashes[MYP.HashWAR(asset.Path)];
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
        public static async Task<Dictionary<long, List<MythicPackage>>> GetMythicHashList(string warfolder)
        {

            Dictionary<long, List<MythicPackage>> results = new Dictionary<long, List<MythicPackage>>();
            await Task.Run(() =>
                {
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
                });

            return results;
        }
    }
}