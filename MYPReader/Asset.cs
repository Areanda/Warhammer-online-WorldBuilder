using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csharp.WorldBuilder
{
   public class Asset
   {
      public string Name { get; set; }
      public string Filename { get; set; }
      public MythicPackage Packet { get; set; }
      public long Hash { get; set; }
      public Asset Parent { get; set; }
      internal Dictionary<string, Asset> _Assets = new Dictionary<string, Asset>();

      public String[] AssetNames
      {
         get { return InteropUtils.GetStringKeys(_Assets); }
      }

      public Asset getAsset(string name)
      {
         return _Assets[name];
      }

      public override string ToString()
      {
         return Name;
      }

      public string DeHashedName
      {
         get
         {
            if (MYPHash._DeHash.ContainsKey(Hash))
               return MYPHash._DeHash[Hash];
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
               if (MYPHash._Hashes.ContainsKey(MYP.HashWAR(Path)))
               {
                  List<MythicPackage> p = MYPHash._Hashes[MYP.HashWAR(Path)];
                  using (var stream = new FileStream(System.IO.Path.Combine(MYPHash._WarFolder, p[0].ToString() + ".myp"), FileMode.Open, FileAccess.ReadWrite))
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
            return path.Remove(path.Length - 1);
         }
      }

      public string AppearsIn
      {
         get
         {
            string result = "";
            long hash = MYP.HashWAR(Path);
            if (MYPHash._Hashes.ContainsKey(hash))
            {
               foreach (var p in MYPHash._Hashes[MYP.HashWAR(Path)])
               {
                  result += p.ToString() + " ";
               }
            }
            return result.Trim();
         }
      }
   }


}
