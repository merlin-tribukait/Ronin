﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ronin.Network.Cryptography
{
    public class Blowfish
    {
        public static byte[] StaticKey = {
            (byte) 0x6b,
            (byte) 0x60,
            (byte) 0xcb,
            (byte) 0x5b,
            (byte) 0x82,
            (byte) 0xce,
            (byte) 0x90,
            (byte) 0xb1,
            (byte) 0xcc,
            (byte) 0x2b,
            (byte) 0x6c,
            (byte) 0x55,
            (byte) 0x6c,
            (byte) 0x6c,
            (byte) 0x6c,
            (byte) 0x6c};

        private static int[] KP =
       {
            0x243F6A88,
            unchecked((int)0x85A308D3),
            0x13198A2E,
            0x03707344,
            unchecked((int)0xA4093822),
            0x299F31D0,
            0x082EFA98,
            unchecked((int)0xEC4E6C89),
            0x452821E6,
            0x38D01377,
            unchecked((int)0xBE5466CF),
            0x34E90C6C,
            unchecked((int)0xC0AC29B7),
            unchecked((int)0xC97C50DD),
            0x3F84D5B5,
            unchecked((int)0xB5470917),
            unchecked((int)0x9216D5D9),
            unchecked((int)0x8979FB1B),
        };

        private static int[] KS0 =
        {
        unchecked((int)0xD1310BA6),
        unchecked((int)0x98DFB5AC),
        0x2FFD72DB,
        unchecked((int)0xD01ADFB7),
        unchecked((int)0xB8E1AFED),
        0x6A267E96,
        unchecked((int)0xBA7C9045),
        unchecked((int)0xF12C7F99),
        0x24A19947,
        unchecked((int)0xB3916CF7),
        0x0801F2E2,
        unchecked((int)0x858EFC16),
        0x636920D8,
        0x71574E69,
        unchecked((int)0xA458FEA3),
        unchecked((int)0xF4933D7E),
        0x0D95748F,
        0x728EB658,
        0x718BCD58,
        unchecked((int)0x82154AEE),
        0x7B54A41D,
        unchecked((int)0xC25A59B5),
        unchecked((int)0x9C30D539),
        0x2AF26013,
        unchecked((int)0xC5D1B023),
        0x286085F0,
        unchecked((int)0xCA417918),
        unchecked((int)0xB8DB38EF),
        unchecked((int)0x8E79DCB0),
        0x603A180E,
        0x6C9E0E8B,
        unchecked((int)0xB01E8A3E),
        unchecked((int)0xD71577C1),
        unchecked((int)0xBD314B27),
        0x78AF2FDA,
        0x55605C60,
        unchecked((int)0xE65525F3),
       unchecked((int) 0xAA55AB94),
        0x57489862,
        0x63E81440,
        0x55CA396A,
        0x2AAB10B6,
        unchecked((int)0xB4CC5C34),
        0x1141E8CE,
        unchecked((int)0xA15486AF),
        0x7C72E993,
        unchecked((int)0xB3EE1411),
        0x636FBC2A,
        0x2BA9C55D,
        0x741831F6,
        unchecked((int)0xCE5C3E16),
        unchecked((int)0x9B87931E),
        unchecked((int)0xAFD6BA33),
        0x6C24CF5C,
        0x7A325381,
        0x28958677,
        0x3B8F4898,
        0x6B4BB9AF,
        unchecked((int)0xC4BFE81B),
        0x66282193,
        0x61D809CC,
        unchecked((int)0xFB21A991),
        0x487CAC60,
        0x5DEC8032,
        unchecked((int)0xEF845D5D),
        unchecked((int)0xE98575B1),
        unchecked((int)0xDC262302),
        unchecked((int)0xEB651B88),
        0x23893E81,
        unchecked((int)0xD396ACC5),
        0x0F6D6FF3,
        unchecked((int)0x83F44239),
        0x2E0B4482,
        unchecked((int)0xA4842004),
        0x69C8F04A,
        unchecked((int)0x9E1F9B5E),
        0x21C66842,
        unchecked((int)0xF6E96C9A),
        0x670C9C61,
        unchecked((int)0xABD388F0),
        0x6A51A0D2,
        unchecked((int)0xD8542F68),
        unchecked((int)0x960FA728),
        unchecked((int)0xAB5133A3),
        0x6EEF0B6C,
        0x137A3BE4,
        unchecked((int)0xBA3BF050),
        0x7EFB2A98,
       unchecked((int) 0xA1F1651D),
        0x39AF0176,
        0x66CA593E,
        unchecked((int)0x82430E88),
        unchecked((int)0x8CEE8619),
        0x456F9FB4,
        0x7D84A5C3,
        0x3B8B5EBE,
        unchecked((int)0xE06F75D8),
        unchecked((int)0x85C12073),
        0x401A449F,
        0x56C16AA6,
        0x4ED3AA62,
        0x363F7706,
        0x1BFEDF72,
        0x429B023D,
        0x37D0D724,
        unchecked((int)0xD00A1248),
        unchecked((int)0xDB0FEAD3),
        0x49F1C09B,
        0x075372C9,
        unchecked((int)0x80991B7B),
        0x25D479D8,
        unchecked((int)0xF6E8DEF7),
        unchecked((int)0xE3FE501A),
        unchecked((int)0xB6794C3B),
        unchecked((int)0x976CE0BD),
        0x04C006BA,
        unchecked((int)0xC1A94FB6),
        0x409F60C4,
        0x5E5C9EC2,
        0x196A2463,
        0x68FB6FAF,
        0x3E6C53B5,
        0x1339B2EB,
        0x3B52EC6F,
        0x6DFC511F,
        unchecked((int)0x9B30952C),
        unchecked((int)0xCC814544),
        unchecked((int)0xAF5EBD09),
        unchecked((int)0xBEE3D004),
        unchecked((int)0xDE334AFD),
        0x660F2807,
        0x192E4BB3,
        unchecked((int)0xC0CBA857),
        0x45C8740F,
       unchecked((int) 0xD20B5F39),
        unchecked((int)0xB9D3FBDB),
        0x5579C0BD,
        0x1A60320A,
        unchecked((int)0xD6A100C6),
        0x402C7279,
        0x679F25FE,
        unchecked((int)0xFB1FA3CC),
        unchecked((int)0x8EA5E9F8),
        unchecked((int)0xDB3222F8),
        0x3C7516DF,
        unchecked((int)0xFD616B15),
        0x2F501EC8,
        unchecked((int)0xAD0552AB),
        0x323DB5FA,
        unchecked((int)0xFD238760),
        0x53317B48,
        0x3E00DF82,
        unchecked((int)0x9E5C57BB),
        unchecked((int)0xCA6F8CA0),
        0x1A87562E,
        unchecked((int)0xDF1769DB),
        unchecked((int)0xD542A8F6),
        0x287EFFC3,
        unchecked((int)0xAC6732C6),
        unchecked((int)0x8C4F5573),
        0x695B27B0,
        unchecked((int)0xBBCA58C8),
        unchecked((int)0xE1FFA35D),
        unchecked((int)0xB8F011A0),
        0x10FA3D98,
        unchecked((int)0xFD2183B8),
        0x4AFCB56C,
        0x2DD1D35B,
        unchecked((int)0x9A53E479),
        unchecked((int)0xB6F84565),
        unchecked((int)0xD28E49BC),
        0x4BFB9790,
        unchecked((int)0xE1DDF2DA),
        unchecked((int)0xA4CB7E33),
        0x62FB1341,
        unchecked((int)0xCEE4C6E8),
       unchecked((int) 0xEF20CADA),
        0x36774C01,
        unchecked((int)0xD07E9EFE),
        0x2BF11FB4,
        unchecked((int)0x95DBDA4D),
        unchecked((int)0xAE909198),
        unchecked((int)0xEAAD8E71),
        0x6B93D5A0,
        unchecked((int)0xD08ED1D0),
       unchecked((int) 0xAFC725E0),
        unchecked((int)0x8E3C5B2F),
        unchecked((int)0x8E7594B7),
        unchecked((int)0x8FF6E2FB),
        unchecked((int)0xF2122B64),
        unchecked((int)0x8888B812),
        unchecked((int)0x900DF01C),
        0x4FAD5EA0,
        0x688FC31C,
       unchecked((int) 0xD1CFF191),
        unchecked((int)0xB3A8C1AD),
        0x2F2F2218,
       unchecked((int) 0xBE0E1777),
        unchecked((int)0xEA752DFE),
        unchecked((int)0x8B021FA1),
        unchecked((int)0xE5A0CC0F),
        unchecked((int)0xB56F74E8),
        0x18ACF3D6,
       unchecked((int) 0xCE89E299),
        unchecked((int)0xB4A84FE0),
        unchecked((int)0xFD13E0B7),
        0x7CC43B81,
        unchecked((int)0xD2ADA8D9),
        0x165FA266,
        unchecked((int)0x80957705),
        unchecked((int)0x93CC7314),
        0x211A1477,
        unchecked((int)0xE6AD2065),
        0x77B5FA86,
        unchecked((int)0xC75442F5),
        unchecked((int)0xFB9D35CF),
        unchecked((int)0xEBCDAF0C),
        0x7B3E89A0,
        unchecked((int)0xD6411BD3),
        unchecked((int)0xAE1E7E49),
        0x00250E2D,
        0x2071B35E,
        0x226800BB,
        0x57B8E0AF,
        0x2464369B,
        unchecked((int)0xF009B91E),
        0x5563911D,
        0x59DFA6AA,
        0x78C14389,
        unchecked((int)0xD95A537F),
        0x207D5BA2,
        0x02E5B9C5,
        unchecked((int)0x83260376),
        0x6295CFA9,
        0x11C81968,
        0x4E734A41,
        unchecked((int)0xB3472DCA),
        0x7B14A94A,
        0x1B510052,
        unchecked((int)0x9A532915),
        unchecked((int)0xD60F573F),
        unchecked((int)0xBC9BC6E4),
        0x2B60A476,
        unchecked((int)0x81E67400),
        0x08BA6FB5,
        0x571BE91F,
        unchecked((int)0xF296EC6B),
        0x2A0DD915,
        unchecked((int)0xB6636521),
        unchecked((int)0xE7B9F9B6),
        unchecked((int)0xFF34052E),
        unchecked((int)0xC5855664),
        0x53B02D5D,
        unchecked((int)0xA99F8FA1),
        0x08BA4799,
        0x6E85076A
    };

        private static int[] KS1 =
        {
        0x4B7A70E9,
        unchecked((int)0xB5B32944),
        unchecked((int)0xDB75092E),
        unchecked((int)0xC4192623),
        unchecked((int)0xAD6EA6B0),
        0x49A7DF7D,
        unchecked((int)0x9CEE60B8),
        unchecked((int)0x8FEDB266),
        unchecked((int)0xECAA8C71),
        0x699A17FF,
        0x5664526C,
        unchecked((int)0xC2B19EE1),
        0x193602A5,
        0x75094C29,
        unchecked((int)0xA0591340),
        unchecked((int)0xE4183A3E),
        0x3F54989A,
        0x5B429D65,
        0x6B8FE4D6,
        unchecked((int)0x99F73FD6),
        unchecked((int)0xA1D29C07),
        unchecked((int)0xEFE830F5),
        0x4D2D38E6,
        unchecked((int)0xF0255DC1),
        0x4CDD2086,
        unchecked((int)0x8470EB26),
        0x6382E9C6,
        0x021ECC5E,
        0x09686B3F,
        0x3EBAEFC9,
        0x3C971814,
        0x6B6A70A1,
        0x687F3584,
        0x52A0E286,
        unchecked((int)0xB79C5305),
        unchecked((int)0xAA500737),
        0x3E07841C,
        0x7FDEAE5C,
        unchecked((int)0x8E7D44EC),
        0x5716F2B8,
        unchecked((int)0xB03ADA37),
        unchecked((int)0xF0500C0D),
        unchecked((int)0xF01C1F04),
        0x0200B3FF,
        unchecked((int)0xAE0CF51A),
        0x3CB574B2,
        0x25837A58,
        unchecked((int)0xDC0921BD),
        unchecked((int)0xD19113F9),
        0x7CA92FF6,
        unchecked((int)0x94324773),
        0x22F54701,
        0x3AE5E581,
        0x37C2DADC,
        unchecked((int)0xC8B57634),
        unchecked((int)0x9AF3DDA7),
        unchecked((int)0xA9446146),
        0x0FD0030E,
        unchecked((int)0xECC8C73E),
        unchecked((int)0xA4751E41),
        unchecked((int)0xE238CD99),
        0x3BEA0E2F,
        0x3280BBA1,
        0x183EB331,
        0x4E548B38,
        0x4F6DB908,
        0x6F420D03,
        unchecked((int)0xF60A04BF),
        0x2CB81290,
        0x24977C79,
        0x5679B072,
        unchecked((int)0xBCAF89AF),
        unchecked((int)0xDE9A771F),
        unchecked((int)0xD9930810),
        unchecked((int)0xB38BAE12),
        unchecked((int)0xDCCF3F2E),
        0x5512721F,
        0x2E6B7124,
        0x501ADDE6,
        unchecked((int)0x9F84CD87),
        0x7A584718,
        0x7408DA17,
        unchecked((int)0xBC9F9ABC),
        unchecked((int)0xE94B7D8C),
        unchecked((int)0xEC7AEC3A),
        unchecked((int)0xDB851DFA),
        0x63094366,
        unchecked((int)0xC464C3D2),
        unchecked((int)0xEF1C1847),
        0x3215D908,
        unchecked((int)0xDD433B37),
        0x24C2BA16,
        0x12A14D43,
        0x2A65C451,
        0x50940002,
        0x133AE4DD,
        0x71DFF89E,
        0x10314E55,
        unchecked((int)0x81AC77D6),
        0x5F11199B,
        0x043556F1,
        unchecked((int)0xD7A3C76B),
        0x3C11183B,
        0x5924A509,
        unchecked((int)0xF28FE6ED),
        unchecked((int)0x97F1FBFA),
        unchecked((int)0x9EBABF2C),
        0x1E153C6E,
        unchecked((int)0x86E34570),
        unchecked((int)0xEAE96FB1),
        unchecked((int)0x860E5E0A),
        0x5A3E2AB3,
        0x771FE71C,
        0x4E3D06FA,
        0x2965DCB9,
        unchecked((int)0x99E71D0F),
        unchecked((int)0x803E89D6),
        0x5266C825,
        0x2E4CC978,
        unchecked((int)0x9C10B36A),
        unchecked((int)0xC6150EBA),
        unchecked((int)0x94E2EA78),
        unchecked((int)0xA5FC3C53),
        0x1E0A2DF4,
       unchecked((int) 0xF2F74EA7),
        0x361D2B3D,
        0x1939260F,
        0x19C27960,
        0x5223A708,
        unchecked((int)0xF71312B6),
        unchecked((int)0xEBADFE6E),
        unchecked((int)0xEAC31F66),
        unchecked((int)0xE3BC4595),
        unchecked((int)0xA67BC883),
        unchecked((int)0xB17F37D1),
        0x018CFF28,
       unchecked((int) 0xC332DDEF),
        unchecked((int)0xBE6C5AA5),
        0x65582185,
        0x68AB9802,
        unchecked((int)0xEECEA50F),
        unchecked((int)0xDB2F953B),
        0x2AEF7DAD,
        0x5B6E2F84,
        0x1521B628,
        0x29076170,
        unchecked((int)0xECDD4775),
        0x619F1510,
        0x13CCA830,
        unchecked((int)0xEB61BD96),
        0x0334FE1E,
        unchecked((int)0xAA0363CF),
        unchecked((int)0xB5735C90),
        0x4C70A239,
        unchecked((int)0xD59E9E0B),
        unchecked((int)0xCBAADE14),
        unchecked((int)0xEECC86BC),
        0x60622CA7,
        unchecked((int)0x9CAB5CAB),
        unchecked((int)0xB2F3846E),
        0x648B1EAF,
        0x19BDF0CA,
        unchecked((int)0xA02369B9),
        0x655ABB50,
        0x40685A32,
        0x3C2AB4B3,
        0x319EE9D5,
        unchecked((int)0xC021B8F7),
        unchecked((int)0x9B540B19),
        unchecked((int)0x875FA099),
        unchecked((int)0x95F7997E),
        0x623D7DA8,
        unchecked((int)0xF837889A),
        unchecked((int)0x97E32D77),
        0x11ED935F,
        0x16681281,
        0x0E358829,
        unchecked((int)0xC7E61FD6),
        unchecked((int)0x96DEDFA1),
        0x7858BA99,
        0x57F584A5,
        0x1B227263,
        unchecked((int)0x9B83C3FF),
        0x1AC24696,
        unchecked((int)0xCDB30AEB),
        0x532E3054,
        unchecked((int)0x8FD948E4),
        0x6DBC3128,
        0x58EBF2EF,
        0x34C6FFEA,
        unchecked((int)0xFE28ED61),
        unchecked((int)0xEE7C3C73),
        0x5D4A14D9,
        unchecked((int)0xE864B7E3),
        0x42105D14,
        0x203E13E0,
        0x45EEE2B6,
        unchecked((int)0xA3AAABEA),
        unchecked((int)0xDB6C4F15),
        unchecked((int)0xFACB4FD0),
        unchecked((int)0xC742F442),
        unchecked((int)0xEF6ABBB5),
        0x654F3B1D,
        0x41CD2105,
        unchecked((int)0xD81E799E),
        unchecked((int)0x86854DC7),
        unchecked((int)0xE44B476A),
        0x3D816250,
        unchecked((int)0xCF62A1F2),
        0x5B8D2646,
        unchecked((int)0xFC8883A0),
        unchecked((int)0xC1C7B6A3),
        0x7F1524C3,
        0x69CB7492,
        0x47848A0B,
        0x5692B285,
        0x095BBF00,
        unchecked((int)0xAD19489D),
        0x1462B174,
        0x23820E00,
        0x58428D2A,
        0x0C55F5EA,
        0x1DADF43E,
        0x233F7061,
        0x3372F092,
        unchecked((int)0x8D937E41),
        unchecked((int)0xD65FECF1),
        0x6C223BDB,
        0x7CDE3759,
        unchecked((int)0xCBEE7460),
        0x4085F2A7,
        unchecked((int)0xCE77326E),
        unchecked((int)0xA6078084),
        0x19F8509E,
        unchecked((int)0xE8EFD855),
        0x61D99735,
        unchecked((int)0xA969A7AA),
        unchecked((int)0xC50C06C2),
        0x5A04ABFC,
        unchecked((int)0x800BCADC),
        unchecked((int)0x9E447A2E),
        unchecked((int)0xC3453484),
        unchecked((int)0xFDD56705),
        0x0E1E9EC9,
        unchecked((int)0xDB73DBD3),
        0x105588CD,
        0x675FDA79,
        unchecked((int)0xE3674340),
        unchecked((int)0xC5C43465),
        0x713E38D8,
        0x3D28F89E,
        unchecked((int)0xF16DFF20),
        0x153E21E7,
        unchecked((int)0x8FB03D4A),
        unchecked((int)0xE6E39F2B),
        unchecked((int)0xDB83ADF7)
    };

        private static int[] KS2 =
        {
        unchecked((int)0xE93D5A68),
        unchecked((int)0x948140F7),
        unchecked((int)0xF64C261C),
        unchecked((int)0x94692934),
        0x411520F7,
        0x7602D4F7,
        unchecked((int)0xBCF46B2E),
        unchecked((int)0xD4A20068),
        unchecked((int)0xD4082471),
        0x3320F46A,
        0x43B7D4B7,
        0x500061AF,
        0x1E39F62E,
        unchecked((int)0x97244546),
        0x14214F74,
        unchecked((int)0xBF8B8840),
        0x4D95FC1D,
        unchecked((int)0x96B591AF),
        0x70F4DDD3,
        0x66A02F45,
        unchecked((int)0xBFBC09EC),
        0x03BD9785,
        0x7FAC6DD0,
        0x31CB8504,
        unchecked((int)0x96EB27B3),
        0x55FD3941,
        unchecked((int)0xDA2547E6),
        unchecked((int)0xABCA0A9A),
        0x28507825,
        0x530429F4,
        0x0A2C86DA,
        unchecked((int)0xE9B66DFB),
        0x68DC1462,
        unchecked((int)0xD7486900),
        0x680EC0A4,
        0x27A18DEE,
        0x4F3FFEA2,
        unchecked((int)0xE887AD8C),
        unchecked((int)0xB58CE006),
        0x7AF4D6B6,
        unchecked((int)0xAACE1E7C),
        unchecked((int)0xD3375FEC),
        unchecked((int)0xCE78A399),
        0x406B2A42,
        0x20FE9E35,
        unchecked((int)0xD9F385B9),
        unchecked((int)0xEE39D7AB),
        0x3B124E8B,
        0x1DC9FAF7,
        0x4B6D1856,
        0x26A36631,
        unchecked((int)0xEAE397B2),
        0x3A6EFA74,
        unchecked((int)0xDD5B4332),
        0x6841E7F7,
        unchecked((int)0xCA7820FB),
        unchecked((int)0xFB0AF54E),
        unchecked((int)0xD8FEB397),
        0x454056AC,
        unchecked((int)0xBA489527),
        0x55533A3A,
        0x20838D87,
        unchecked((int)0xFE6BA9B7),
        unchecked((int)0xD096954B),
        0x55A867BC,
        unchecked((int)0xA1159A58),
        unchecked((int)0xCCA92963),
        unchecked((int)0x99E1DB33),
        unchecked((int)0xA62A4A56),
        0x3F3125F9,
        0x5EF47E1C,
        unchecked((int)0x9029317C),
        unchecked((int)0xFDF8E802),
        0x04272F70,
        unchecked((int)0x80BB155C),
        0x05282CE3,
        unchecked((int)0x95C11548),
        unchecked((int)0xE4C66D22),
        0x48C1133F,
        unchecked((int)0xC70F86DC),
        0x07F9C9EE,
        0x41041F0F,
        0x404779A4,
        0x5D886E17,
        0x325F51EB,
        unchecked((int)0xD59BC0D1),
        unchecked((int)0xF2BCC18F),
        0x41113564,
        0x257B7834,
        0x602A9C60,
        unchecked((int)0xDFF8E8A3),
        0x1F636C1B,
        0x0E12B4C2,
        0x02E1329E,
        unchecked((int)0xAF664FD1),
        unchecked((int)0xCAD18115),
        0x6B2395E0,
        0x333E92E1,
        0x3B240B62,
        unchecked((int)0xEEBEB922),
        unchecked((int)0x85B2A20E),
        unchecked((int)0xE6BA0D99),
        unchecked((int)0xDE720C8C),
        0x2DA2F728,
        unchecked((int)0xD0127845),
        unchecked((int)0x95B794FD),
        0x647D0862,
        unchecked((int)0xE7CCF5F0),
        0x5449A36F,
        unchecked((int)0x877D48FA),
        unchecked((int)0xC39DFD27),
        unchecked((int)0xF33E8D1E),
        0x0A476341,
        unchecked((int)0x992EFF74),
        0x3A6F6EAB,
        unchecked((int)0xF4F8FD37),
        unchecked((int)0xA812DC60),
        unchecked((int)0xA1EBDDF8),
        unchecked((int)0x991BE14C),
        unchecked((int)0xDB6E6B0D),
        unchecked((int)0xC67B5510),
        0x6D672C37,
        0x2765D43B,
        unchecked((int)0xDCD0E804),
        unchecked((int)0xF1290DC7),
        unchecked((int)0xCC00FFA3),
        unchecked((int)0xB5390F92),
        0x690FED0B,
        0x667B9FFB,
        unchecked((int)0xCEDB7D9C),
        unchecked((int)0xA091CF0B),
        unchecked((int)0xD9155EA3),
        unchecked((int)0xBB132F88),
        0x515BAD24,
        0x7B9479BF,
        0x763BD6EB,
        0x37392EB3,
        unchecked((int)0xCC115979),
        unchecked((int)0x8026E297),
        unchecked((int)0xF42E312D),
        0x6842ADA7,
        unchecked((int)0xC66A2B3B),
        0x12754CCC,
        0x782EF11C,
        0x6A124237,
        unchecked((int)0xB79251E7),
        0x06A1BBE6,
        0x4BFB6350,
        0x1A6B1018,
        0x11CAEDFA,
        0x3D25BDD8,
        unchecked((int)0xE2E1C3C9),
        0x44421659,
        0x0A121386,
        unchecked((int)0xD90CEC6E),
        unchecked((int)0xD5ABEA2A),
        0x64AF674E,
        unchecked((int)0xDA86A85F),
        unchecked((int)0xBEBFE988),
        0x64E4C3FE,
        unchecked((int)0x9DBC8057),
        unchecked((int)0xF0F7C086),
        0x60787BF8,
        0x6003604D,
        unchecked((int)0xD1FD8346),
        unchecked((int)0xF6381FB0),
        0x7745AE04,
        unchecked((int)0xD736FCCC),
        unchecked((int)0x83426B33),
        unchecked((int)0xF01EAB71),
        unchecked((int)0xB0804187),
        0x3C005E5F,
        0x77A057BE,
        unchecked((int)0xBDE8AE24),
        0x55464299,
        unchecked((int)0xBF582E61),
        0x4E58F48F,
        unchecked((int)0xF2DDFDA2),
        unchecked((int)0xF474EF38),
        unchecked((int)0x8789BDC2),
        0x5366F9C3,
        unchecked((int)0xC8B38E74),
        unchecked((int)0xB475F255),
        0x46FCD9B9,
        0x7AEB2661,
        unchecked((int)0x8B1DDF84),
        unchecked((int)0x846A0E79),
        unchecked((int)0x915F95E2),
        0x466E598E,
        0x20B45770,
        unchecked((int)0x8CD55591),
        unchecked((int)0xC902DE4C),
        unchecked((int)0xB90BACE1),
        unchecked((int)0xBB8205D0),
        0x11A86248,
        0x7574A99E,
        unchecked((int)0xB77F19B6),
        unchecked((int)0xE0A9DC09),
        0x662D09A1,
        unchecked((int)0xC4324633),
        unchecked((int)0xE85A1F02),
        0x09F0BE8C,
        0x4A99A025,
        0x1D6EFE10,
        0x1AB93D1D,
        0x0BA5A4DF,
        unchecked((int)0xA186F20F),
        0x2868F169,
        unchecked((int)0xDCB7DA83),
        0x573906FE,
        unchecked((int)0xA1E2CE9B),
        0x4FCD7F52,
        0x50115E01,
        unchecked((int)0xA70683FA),
        unchecked((int)0xA002B5C4),
        0x0DE6D027,
        unchecked((int)0x9AF88C27),
        0x773F8641,
        unchecked((int)0xC3604C06),
        0x61A806B5,
        unchecked((int)0xF0177A28),
        unchecked((int)0xC0F586E0),
        0x006058AA,
        0x30DC7D62,
        0x11E69ED7,
        0x2338EA63,
        0x53C2DD94,
        unchecked((int)0xC2C21634),
        unchecked((int)0xBBCBEE56),
        unchecked((int)0x90BCB6DE),
        unchecked((int)0xEBFC7DA1),
        unchecked((int)0xCE591D76),
        0x6F05E409,
        0x4B7C0188,
        0x39720A3D,
        0x7C927C24,
        unchecked((int)0x86E3725F),
        0x724D9DB9,
        0x1AC15BB4,
        unchecked((int)0xD39EB8FC),
        unchecked((int)0xED545578),
        0x08FCA5B5,
        unchecked((int)0xD83D7CD3),
        0x4DAD0FC4,
        0x1E50EF5E,
        unchecked((int)0xB161E6F8),
        unchecked((int)0xA28514D9),
        0x6C51133C,
        0x6FD5C7E7,
        0x56E14EC4,
        0x362ABFCE,
        unchecked((int)0xDDC6C837),
       unchecked((int) 0xD79A3234),
        unchecked((int)0x92638212),
        unchecked((int)0x670EFA8E),
        0x406000E0
    };

        private static int[] KS3 =
        {
        0x3A39CE37,
        unchecked((int)0xD3FAF5CF),
        unchecked((int)0xABC27737),
        0x5AC52D1B,
        0x5CB0679E,
        0x4FA33742,
        unchecked((int)0xD3822740),
        unchecked((int)0x99BC9BBE),
        unchecked((int)0xD5118E9D),
        unchecked((int)0xBF0F7315),
        unchecked((int)0xD62D1C7E),
        unchecked((int)0xC700C47B),
        unchecked((int)0xB78C1B6B),
        0x21A19045,
        unchecked((int)0xB26EB1BE),
        0x6A366EB4,
        0x5748AB2F,
        unchecked((int)0xBC946E79),
        unchecked((int)0xC6A376D2),
        0x6549C2C8,
        0x530FF8EE,
        0x468DDE7D,
        unchecked((int)0xD5730A1D),
        0x4CD04DC6,
        0x2939BBDB,
        unchecked((int)0xA9BA4650),
        unchecked((int)0xAC9526E8),
        unchecked((int)0xBE5EE304),
        unchecked((int)0xA1FAD5F0),
        0x6A2D519A,
        0x63EF8CE2,
        unchecked((int)0x9A86EE22),
        unchecked((int)0xC089C2B8),
        0x43242EF6,
        unchecked((int)0xA51E03AA),
        unchecked((int)0x9CF2D0A4),
        unchecked((int)0x83C061BA),
        unchecked((int)0x9BE96A4D),
        unchecked((int)0x8FE51550),
        unchecked((int)0xBA645BD6),
        0x2826A2F9,
        unchecked((int)0xA73A3AE1),
        0x4BA99586,
        unchecked((int)0xEF5562E9),
        unchecked((int)0xC72FEFD3),
        unchecked((int)0xF752F7DA),
        0x3F046F69,
        0x77FA0A59,
        unchecked((int)0x80E4A915),
        unchecked((int)0x87B08601),
        unchecked((int)0x9B09E6AD),
        0x3B3EE593,
        unchecked((int)0xE990FD5A),
        unchecked((int)0x9E34D797),
        0x2CF0B7D9,
        0x022B8B51,
        unchecked((int)0x96D5AC3A),
        0x017DA67D,
        unchecked((int)0xD1CF3ED6),
        0x7C7D2D28,
        0x1F9F25CF,
        unchecked((int)0xADF2B89B),
        0x5AD6B472,
        0x5A88F54C,
        unchecked((int)0xE029AC71),
        unchecked((int)0xE019A5E6),
        0x47B0ACFD,
        unchecked((int)0xED93FA9B),
        unchecked((int)0xE8D3C48D),
        0x283B57CC,
        unchecked((int)0xF8D56629),
        0x79132E28,
        0x785F0191,
        unchecked((int)0xED756055),
        unchecked((int)0xF7960E44),
        unchecked((int)0xE3D35E8C),
        0x15056DD4,
        unchecked((int)0x88F46DBA),
        0x03A16125,
        0x0564F0BD,
        unchecked((int)0xC3EB9E15),
        0x3C9057A2,
        unchecked((int)0x97271AEC),
        unchecked((int)0xA93A072A),
        0x1B3F6D9B,
        0x1E6321F5,
        unchecked((int)0xF59C66FB),
        0x26DCF319,
        0x7533D928,
        unchecked((int)0xB155FDF5),
        0x03563482,
        unchecked((int)0x8ABA3CBB),
        0x28517711,
        unchecked((int)0xC20AD9F8),
        unchecked((int)0xABCC5167),
        unchecked((int)0xCCAD925F),
        0x4DE81751,
        0x3830DC8E,
        0x379D5862,
        unchecked((int)0x9320F991),
        unchecked((int)0xEA7A90C2),
        unchecked((int)0xFB3E7BCE),
        0x5121CE64,
        0x774FBE32,
        unchecked((int)0xA8B6E37E),
        unchecked((int)0xC3293D46),
        0x48DE5369,
        0x6413E680,
        unchecked((int)0xA2AE0810),
        unchecked((int)0xDD6DB224),
        0x69852DFD,
        0x09072166,
        unchecked((int)0xB39A460A),
        0x6445C0DD,
        0x586CDECF,
        0x1C20C8AE,
        0x5BBEF7DD,
        0x1B588D40,
        unchecked((int)0xCCD2017F),
        0x6BB4E3BB,
        unchecked((int)0xDDA26A7E),
        0x3A59FF45,
        0x3E350A44,
        unchecked((int)0xBCB4CDD5),
        0x72EACEA8,
        unchecked((int)0xFA6484BB),
        unchecked((int)0x8D6612AE),
        unchecked((int)0xBF3C6F47),
        unchecked((int)0xD29BE463),
        0x542F5D9E,
        unchecked((int)0xAEC2771B),
        unchecked((int)0xF64E6370),
        0x740E0D8D,
        unchecked((int)0xE75B1357),
        unchecked((int)0xF8721671),
        unchecked((int)0xAF537D5D),
        0x4040CB08,
        0x4EB4E2CC,
        0x34D2466A,
        0x0115AF84,
        unchecked((int)0xE1B00428),
        unchecked((int)0x95983A1D),
        0x06B89FB4,
        unchecked((int)0xCE6EA048),
        0x6F3F3B82,
        0x3520AB82,
        0x011A1D4B,
        0x277227F8,
        0x611560B1,
        unchecked((int)0xE7933FDC),
        unchecked((int)0xBB3A792B),
        0x344525BD,
        unchecked((int)0xA08839E1),
        0x51CE794B,
        0x2F32C9B7,
        unchecked((int)0xA01FBAC9),
        unchecked((int)0xE01CC87E),
        unchecked((int)0xBCC7D1F6),
        unchecked((int)0xCF0111C3),
        unchecked((int)0xA1E8AAC7),
        0x1A908749,
        unchecked((int)0xD44FBD9A),
        unchecked((int)0xD0DADECB),
        unchecked((int)0xD50ADA38),
        0x0339C32A,
        unchecked((int)0xC6913667),
        unchecked((int)0x8DF9317C),
        unchecked((int)0xE0B12B4F),
        unchecked((int)0xF79E59B7),
        0x43F5BB3A,
        unchecked((int)0xF2D519FF),
        0x27D9459C,
        unchecked((int)0xBF97222C),
        0x15E6FC2A,
        0x0F91FC71,
        unchecked((int)0x9B941525),
        unchecked((int)0xFAE59361),
        unchecked((int)0xCEB69CEB),
        unchecked((int)0xC2A86459),
        0x12BAA8D1,
        unchecked((int)0xB6C1075E),
        unchecked((int)0xE3056A0C),
        0x10D25065,
        unchecked((int)0xCB03A442),
        unchecked((int)0xE0EC6E0E),
        0x1698DB3B,
        0x4C98A0BE,
        0x3278E964,
        unchecked((int)0x9F1F9532),
        unchecked((int)0xE0D392DF),
        unchecked((int)0xD3A0342B),
        unchecked((int)0x8971F21E),
        0x1B0A7441,
        0x4BA3348C,
        unchecked((int)0xC5BE7120),
        unchecked((int)0xC37632D8),
        unchecked((int)0xDF359F8D),
        unchecked((int)0x9B992F2E),
        unchecked((int)0xE60B6F47),
        0x0FE3F11D,
        unchecked((int)0xE54CDA54),
        0x1EDAD891,
        unchecked((int)0xCE6279CF),
        unchecked((int)0xCD3E7E6F),
        0x1618B166,
        unchecked((int)0xFD2C1D05),
        unchecked((int)0x848FD2C5),
        unchecked((int)0xF6FB2299),
        unchecked((int)0xF523F357),
        unchecked((int)0xA6327623),
        unchecked((int)0x93A83531),
        0x56CCCD02,
        unchecked((int)0xACF08162),
        0x5A75EBB5,
        0x6E163697,
        unchecked((int)0x88D273CC),
        unchecked((int)0xDE966292),
        unchecked((int)0x81B949D0),
        0x4C50901B,
        0x71C65614,
        unchecked((int)0xE6C6C7BD),
        0x327A140A,
        0x45E1D006,
        unchecked((int)0xC3F27B9A),
        unchecked((int)0xC9AA53FD),
        0x62A80F00,
        unchecked((int)0xBB25BFE2),
        0x35BDD2F6,
        0x71126905,
        unchecked((int)0xB2040222),
        unchecked((int)0xB6CBCF7C),
        unchecked((int)0xCD769C2B),
        0x53113EC0,
        0x1640E3D3,
        0x38ABBD60,
        0x2547ADF0,
        unchecked((int)0xBA38209C),
        unchecked((int)0xF746CE76),
        0x77AFA1C5,
        0x20756060,
        unchecked((int)0x85CBFE4E),
        unchecked((int)0x8AE88DD8),
        0x7AAAF9B0,
        0x4CF9AA7E,
        0x1948C25C,
        0x02FB8A8C,
        0x01C36AE4,
        unchecked((int)0xD6EBE1F9),
        unchecked((int)0x90D4F869),
        unchecked((int)0xA65CDEA0),
        0x3F09252D,
        unchecked((int)0xC208E69F),
        unchecked((int)0xB74E6132),
        unchecked((int)0xCE77E25B),
        0x578FDFE3,
        0x3AC372E6
    };

        private const int ROUNDS = 16;
        private const int BLOCK_SIZE = 8; // bytes = 64 bits
        private const int SBOX_SK = 256;
        private const int P_SZ = ROUNDS + 2;
        private int[] S0, S1, S2, S3; // the s-boxes
        private int[] P; // the p-array
        private byte[] workingKey = null;

        public static long CalculateChecksum(byte[] buffer, int size)
        {
            long CheckSum = 0;
            for (int i = 2; i < size; i += 4)
            {
                int num = buffer[i] | (buffer[i + 1] << 8) | (buffer[i + 2] << 16) | (buffer[i + 3] << 24);
                CheckSum ^= num;
            }

            return CheckSum;
        }

        private int func(int x)
        {
            return (((S0[(x >> 24 & 0xff)]
                + S1[(x >> 16) & 0xff])
                ^ S2[(x >> 8) & 0xff])
                + S3[x & 0xff]);
        }

        public Blowfish()
        {
            S0 = new int[SBOX_SK];
            S1 = new int[SBOX_SK];
            S2 = new int[SBOX_SK];
            S3 = new int[SBOX_SK];
            P = new int[P_SZ];
        }

        private void processTable(int xl, int xr, int[] table)
        {
            int size = table.Length;
            for (int s = 0; s < size; s += 2)
            {
                xl ^= P[0];
                xr ^= func(xl) ^ P[1];
                xl ^= func(xr) ^ P[2];
                xr ^= func(xl) ^ P[3];
                xl ^= func(xr) ^ P[4];
                xr ^= func(xl) ^ P[5];
                xl ^= func(xr) ^ P[6];
                xr ^= func(xl) ^ P[7];
                xl ^= func(xr) ^ P[8];
                xr ^= func(xl) ^ P[9];
                xl ^= func(xr) ^ P[10];
                xr ^= func(xl) ^ P[11];
                xl ^= func(xr) ^ P[12];
                xr ^= func(xl) ^ P[13];
                xl ^= func(xr) ^ P[14];
                xr ^= func(xl) ^ P[15];
                xl ^= func(xr) ^ P[16];
                xr ^= P[17];

                table[s] = xr;
                table[s + 1] = xl;
                xr = xl; // end of cycle swap
                xl = table[s];
            }
        }

        public void setKey(byte[] key)
        {
            /**
             * - comments are from _Applied Crypto_, Schneier, p338.<br>
             * Please be careful comparing the two, AC numbers the arrays from 1, the enclosed code from 0.<br>
             * (1) Initialize the S-boxes and the P-array, with a fixed string This string contains the hexadecimal digits of pi (3.141...)
             */
            Array.Copy(KS0, 0, S0, 0, SBOX_SK);
            Array.Copy(KS1, 0, S1, 0, SBOX_SK);
            Array.Copy(KS2, 0, S2, 0, SBOX_SK);
            Array.Copy(KS3, 0, S3, 0, SBOX_SK);
            Array.Copy(KP, 0, P, 0, P_SZ);

            /**
             * (2) Now, XOR P[0] with the first 32 bits of the key, XOR P[1] with the second 32-bits of the key, and so on for all bits of the key (up to P[17]).<br>
             * Repeatedly cycle through the key bits until the entire P-array has been XOR-ed with the key bits
             */
            int keyLength = key.Length;
            int keyIndex = 0;
            for (int i = 0; i < P_SZ; i++)
            {
                // get the 32 bits of the key, in 4 * 8 bit chunks
                int data = 0x0000000;
                for (int j = 0; j < 4; j++)
                {
                    // create a 32 bit block
                    data = (int)((data << 8) | (key[keyIndex++] & 0xff));

                    // wrap when we get to the end of the key
                    if (keyIndex >= keyLength)
                    {
                        keyIndex = 0;
                    }
                }

                // XOR the newly created 32 bit chunk onto the P-array
                P[i] ^= data;
            }

            /**
             * (3) Encrypt the all-zero string with the Blowfish algorithm, using the subkeys described in (1) and (2)<br>
             * (4) Replace P1 and P2 with the output of step (3)<br>
             * (5) Encrypt the output of step(3) using the Blowfish algorithm, with the modified subkeys.<br>
             * (6) Replace P3 and P4 with the output of step (5)<br>
             * (7) Continue the process, replacing all elements of the P-array and then all four S-boxes in order, with the output of the continuously changing Blowfish algorithm
             */
            processTable(0, 0, P);
            processTable(P[P_SZ - 2], P[P_SZ - 1], S0);
            processTable(S0[SBOX_SK - 2], S0[SBOX_SK - 1], S1);
            processTable(S1[SBOX_SK - 2], S1[SBOX_SK - 1], S2);
            processTable(S2[SBOX_SK - 2], S2[SBOX_SK - 1], S3);
        }

        private static int bytesTo32bits(byte[] src, int srcIndex)
        {
            return (int)(((src[srcIndex + 3] & 0xff) << 24) | ((src[srcIndex + 2] & 0xff) << 16) | ((src[srcIndex + 1] & 0xff) << 8) | ((src[srcIndex] & 0xff)));
        }

        public void encryptBlock(byte[] src, int srcIndex, byte[] dst, int dstIndex)
        {
            int xl = bytesTo32bits(src, srcIndex);
            int xr = bytesTo32bits(src, srcIndex + 4);

            xl ^= P[0];
            xr ^= func(xl) ^ P[1];
            xl ^= func(xr) ^ P[2];
            xr ^= func(xl) ^ P[3];
            xl ^= func(xr) ^ P[4];
            xr ^= func(xl) ^ P[5];
            xl ^= func(xr) ^ P[6];
            xr ^= func(xl) ^ P[7];
            xl ^= func(xr) ^ P[8];
            xr ^= func(xl) ^ P[9];
            xl ^= func(xr) ^ P[10];
            xr ^= func(xl) ^ P[11];
            xl ^= func(xr) ^ P[12];
            xr ^= func(xl) ^ P[13];
            xl ^= func(xr) ^ P[14];
            xr ^= func(xl) ^ P[15];
            xl ^= func(xr) ^ P[16];
            xr ^= P[17];

            bits32ToBytes(xr, ref dst, dstIndex);
            bits32ToBytes(xl, ref dst, dstIndex + 4);
        }

        private static void decrXORPass(ref byte[] raw, int offset, int size, int key)
        {
            int stop = 4 + offset;
            int pos = size - 8 - 4 + offset;
            int edx;
            int ecx = key; // Initial xor key

            while (pos > stop)
            {
                edx = (raw[pos] & 0xFF);
                edx |= (raw[pos + 1] & 0xFF) << 8;
                edx |= (raw[pos + 2] & 0xFF) << 16;
                edx |= (raw[pos + 3] & 0xFF) << 24;

                edx ^= ecx;
                ecx -= edx ^ ecx;

                raw[pos++] = (byte)(edx & 0xFF);
                raw[pos++] = (byte)((edx >> 8) & 0xFF);
                raw[pos++] = (byte)((edx >> 16) & 0xFF);
                raw[pos++] = (byte)((edx >> 24) & 0xFF);
                pos -= 8;
            }
        }

        public void decryptBlock(byte[] src, int srcIndex, byte[] dst, int dstIndex)
        {
            int xl = bytesTo32bits(src, srcIndex);
            int xr = bytesTo32bits(src, srcIndex + 4);
            xl ^= P[17];
            xr ^= func(xl) ^ P[16];
            xl ^= func(xr) ^ P[15];
            xr ^= func(xl) ^ P[14];
            xl ^= func(xr) ^ P[13];
            xr ^= func(xl) ^ P[12];
            xl ^= func(xr) ^ P[11];
            xr ^= func(xl) ^ P[10];
            xl ^= func(xr) ^ P[9];
            xr ^= func(xl) ^ P[8];
            xl ^= func(xr) ^ P[7];
            xr ^= func(xl) ^ P[6];
            xl ^= func(xr) ^ P[5];
            xr ^= func(xl) ^ P[4];
            xl ^= func(xr) ^ P[3];
            xr ^= func(xl) ^ P[2];
            xl ^= func(xr) ^ P[1];
            xr ^= P[0];

            bits32ToBytes(xr, ref dst, dstIndex);
            bits32ToBytes(xl, ref dst, dstIndex + 4);
        }

        private static void bits32ToBytes(int input, ref byte[] dst, int dstIndex)
        {
            dst[dstIndex] = (byte)input;
            dst[dstIndex + 1] = (byte)(input >> 8);
            dst[dstIndex + 2] = (byte)(input >> 16);
            dst[dstIndex + 3] = (byte)(input >> 24);
        }

        public static void RemoveXorMask(byte[] buffer, ref int xorKey)
        {
            //remove xor mask
            int stop = 6;
            int pos = buffer.Length - 8 - 4;
            int edx;
            int ecx = xorKey; // final xor key

            while (pos >= stop)
            {
                edx = (buffer[pos] & 0xFF);
                edx |= (buffer[pos + 1] & 0xFF) << 8;
                edx |= (buffer[pos + 2] & 0xFF) << 16;
                edx |= (buffer[pos + 3] & 0xFF) << 24;

                edx ^= ecx;
                ecx -= edx;

                buffer[pos++] = (byte)(edx & 0xFF);
                buffer[pos++] = (byte)((edx >> 8) & 0xFF);
                buffer[pos++] = (byte)((edx >> 16) & 0xFF);
                buffer[pos++] = (byte)((edx >> 24) & 0xFF);
                pos -= 8;
            }
        }

        //NOT WORKING
        //public static void ApplyXorMask(byte[] buffer, ref int xorKey)
        //{
        //    int stop = buffer.Length - 8;
        //    int pos = 4;
        //    int edx;
        //    int ecx = xorKey; // Initial xor key

        //    while (pos < stop)
        //    {
        //        edx = (buffer[pos] & 0xFF);
        //        edx |= (buffer[pos + 1] & 0xFF) << 8;
        //        edx |= (buffer[pos + 2] & 0xFF) << 16;
        //        edx |= (buffer[pos + 3] & 0xFF) << 24;

        //        ecx += edx;

        //        edx ^= ecx;

        //        buffer[pos++] = (byte)(edx & 0xFF);
        //        buffer[pos++] = (byte)((edx >> 8) & 0xFF);
        //        buffer[pos++] = (byte)((edx >> 16) & 0xFF);
        //        buffer[pos++] = (byte)((edx >> 24) & 0xFF);
        //    }
        //    buffer[pos++] = (byte)(ecx & 0xFF);
        //    buffer[pos++] = (byte)((ecx >> 8) & 0xFF);
        //    buffer[pos++] = (byte)((ecx >> 16) & 0xFF);
        //    buffer[pos++] = (byte)((ecx >> 24) & 0xFF);
        //}
    }
}
