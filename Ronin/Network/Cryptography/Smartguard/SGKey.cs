using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Network.Cryptography.SmartGuard
{
    public struct PropertyOffsets
    {
        public int Offset;
        public int OutArr;
        public int OutX;
        public int OutY;
        public int InArr;
        public int InX;
        public int InY;

        public PropertyOffsets(int offset, int outArr, int outX, int outY, int inArr, int inX, int inY)
        {
            Offset = offset;
            OutArr = outArr;
            OutX = outX;
            OutY = outY;
            InArr = inArr;
            InX = inX;
            InY = inY;
        }
    }

    public class SGKey
    {
        public static Dictionary<string, PropertyOffsets> OffsetList = new Dictionary<string, PropertyOffsets>();

        public static void Init()
        {
            //old obj
            //OffsetList.Add("Њ‡±‡¦ІP©e\f—•hЯ", 0x78210);

            //new obj
            //OffsetList.Add("©XњW9пNCA¶ Њ‰", 0x7CD80);
            OffsetList.Add("-158024712760456597363609451374869522263", new PropertyOffsets(0x7CD80, -256, 0, 1, -520, -264, -263));
            OffsetList.Add("137556047115920617464773557209726646535", new PropertyOffsets(0x86B60, -256, 0, 1, 8, 264, 265));
            OffsetList.Add("-44218272051621190016364743924653731206", new PropertyOffsets(0x86B60, -256, 0, 1, 8, 264, 265));
            OffsetList.Add("108274751085263590007433671311546817246", new PropertyOffsets(0x91B90, -256, 0, 1, 8, 264, 265));
            OffsetList.Add("40826113043454269487285861380048170709", new PropertyOffsets(0x91B98, -256, 0, 1, 8, 264, 265));
            OffsetList.Add("135003162808678530216129538913552087127", new PropertyOffsets(0x91B98, -256, 0, 1, 8, 264, 265));
            OffsetList.Add("146876934127168681513657879691494095172", new PropertyOffsets(0x91B98, -256, 0, 1, 8, 264, 265));
            OffsetList.Add("142931170525252490611157711547826744879", new PropertyOffsets(0x91B98, -256, 0, 1, 8, 264, 265));
            OffsetList.Add("56605462369309072218205115636181814825", new PropertyOffsets(0x92bc0, -256, 0, 1, 8, 264, 265));
            OffsetList.Add("52877141525845915167529648889702483685", new PropertyOffsets(0x8A820, -256, 0, 1, 16, 272, 273));
        }

        public byte[] ContentBytes = new byte[256];
        public int var1 = 0;
        public int var2 = 0;
    }
}
