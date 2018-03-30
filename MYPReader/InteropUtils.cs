using System;
using System.Collections;
using System.Collections.Generic;

namespace Csharp.WorldBuilder
{
   /// <summary>
   /// Set of utility methods for java - cs interop.
   /// </summary>
   public class InteropUtils
   {

      public static String[] GetStringKeys(IDictionary dic)
      {
         String[] keys = new String[dic.Count];
         int i = 0;
         foreach (String key in dic.Keys)
            keys[i++] = key;
         return keys;
      }

      public static long[] GetLongKeys(IDictionary dic)
      {
         long[] keys = new long[dic.Count];
         int i = 0;
         foreach (long key in dic.Keys)
            keys[i++] = key;
         return keys;
      }

   }
}
