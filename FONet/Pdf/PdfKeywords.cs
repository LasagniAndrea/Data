using System.Collections;

namespace Fonet.Pdf
{
    public enum Keyword
    {
        Obj,
        EndObj,
        R,
        DictionaryBegin,
        DictionaryEnd,
        ArrayBegin,
        ArrayEnd,
        Stream,
        EndStream,
        True,
        False,
        Null,
        XRef,
        Trailer,
        StartXRef,
        Eof,
        BT,
        ET,
        Tf,
        Td,
        Tr,
        Tj
    }

    public sealed class KeywordEntries
    {
        private static readonly IDictionary Entries;

        static KeywordEntries()
        {
            Entries = new Hashtable
            {
                { Keyword.Obj, new byte[] { (byte)'o', (byte)'b', (byte)'j' } },
                { Keyword.EndObj, new byte[] { (byte)'e', (byte)'n', (byte)'d', (byte)'o', (byte)'b', (byte)'j' } },
                { Keyword.R, new byte[] { (byte)'R' } },
                { Keyword.DictionaryBegin, new byte[] { (byte)'<', (byte)'<' } },
                { Keyword.DictionaryEnd, new byte[] { (byte)'>', (byte)'>' } },
                { Keyword.ArrayBegin, new byte[] { (byte)'[' } },
                { Keyword.ArrayEnd, new byte[] { (byte)']' } },
                { Keyword.Stream, new byte[] { (byte)'s', (byte)'t', (byte)'r', (byte)'e', (byte)'a', (byte)'m' } },
                { Keyword.EndStream, new byte[] { (byte)'e', (byte)'n', (byte)'d', (byte)'s', (byte)'t', (byte)'r', (byte)'e', (byte)'a', (byte)'m' } },
                { Keyword.True, new byte[] { (byte)'t', (byte)'r', (byte)'u', (byte)'e' } },
                { Keyword.False, new byte[] { (byte)'f', (byte)'a', (byte)'l', (byte)'s', (byte)'e' } },
                { Keyword.Null, new byte[] { (byte)'n', (byte)'u', (byte)'l', (byte)'l' } },
                { Keyword.XRef, new byte[] { (byte)'x', (byte)'r', (byte)'e', (byte)'f' } },
                { Keyword.Trailer, new byte[] { (byte)'t', (byte)'r', (byte)'a', (byte)'i', (byte)'l', (byte)'e', (byte)'r' } },
                { Keyword.StartXRef, new byte[] { (byte)'s', (byte)'t', (byte)'a', (byte)'r', (byte)'t', (byte)'x', (byte)'r', (byte)'e', (byte)'f' } },
                { Keyword.Eof, new byte[] { (byte)'%', (byte)'%', (byte)'E', (byte)'O', (byte)'F' } },
                { Keyword.BT, new byte[] { (byte)'B', (byte)'T' } },
                { Keyword.ET, new byte[] { (byte)'E', (byte)'T' } },
                { Keyword.Tf, new byte[] { (byte)'T', (byte)'f' } },
                { Keyword.Td, new byte[] { (byte)'T', (byte)'d' } },
                { Keyword.Tr, new byte[] { (byte)'T', (byte)'r' } },
                { Keyword.Tj, new byte[] { (byte)'T', (byte)'j' } }
            };
        }

        private KeywordEntries() { }

        public static byte[] GetKeyword(Keyword keyword)
        {
            return (byte[])Entries[keyword];
        }
    }
}