// (C) 2004 André Betz
// http://www.andrebetz.de

using System;
using System.IO;

namespace MD5Calculator
{
	public class MD5Algo
	{
		public delegate void Progress(int Step);
		private Progress ProgressCB;
		private UInt32[] MD5;
		private UInt32[] BitsCount;
		private byte[] CalcBuffer;
		private byte[] padd;

		//Constants for Transform routine.
		int MD5_S11 = 7;
		int MD5_S12 = 12;
		int MD5_S13 = 17;
		int MD5_S14 = 22;
		int MD5_S21 = 5;
		int MD5_S22 = 9;
		int MD5_S23 = 14;
		int MD5_S24 = 20;
		int MD5_S31 = 4;
		int MD5_S32 = 11;
		int MD5_S33 = 16;
		int MD5_S34 = 23;
		int MD5_S41 = 6;
		int MD5_S42 = 10;
		int MD5_S43 = 15;
		int MD5_S44 = 21;

		UInt32 MD5_T01  = 0xd76aa478;
		UInt32 MD5_T02  = 0xe8c7b756;
		UInt32 MD5_T03  = 0x242070db;
		UInt32 MD5_T04  = 0xc1bdceee;
		UInt32 MD5_T05  = 0xf57c0faf;
		UInt32 MD5_T06  = 0x4787c62a;
		UInt32 MD5_T07  = 0xa8304613;
		UInt32 MD5_T08  = 0xfd469501;
		UInt32 MD5_T09  = 0x698098d8;
		UInt32 MD5_T10  = 0x8b44f7af;
		UInt32 MD5_T11  = 0xffff5bb1;
		UInt32 MD5_T12  = 0x895cd7be;
		UInt32 MD5_T13  = 0x6b901122;
		UInt32 MD5_T14  = 0xfd987193;
		UInt32 MD5_T15  = 0xa679438e;
		UInt32 MD5_T16  = 0x49b40821;
		UInt32 MD5_T17  = 0xf61e2562;
		UInt32 MD5_T18  = 0xc040b340;
		UInt32 MD5_T19  = 0x265e5a51;
		UInt32 MD5_T20  = 0xe9b6c7aa;
		UInt32 MD5_T21  = 0xd62f105d;
		UInt32 MD5_T22  = 0x02441453;
		UInt32 MD5_T23  = 0xd8a1e681;
		UInt32 MD5_T24  = 0xe7d3fbc8;
		UInt32 MD5_T25  = 0x21e1cde6;
		UInt32 MD5_T26  = 0xc33707d6;
		UInt32 MD5_T27  = 0xf4d50d87;
		UInt32 MD5_T28  = 0x455a14ed;
		UInt32 MD5_T29  = 0xa9e3e905;
		UInt32 MD5_T30  = 0xfcefa3f8;
		UInt32 MD5_T31  = 0x676f02d9;
		UInt32 MD5_T32  = 0x8d2a4c8a;
		UInt32 MD5_T33  = 0xfffa3942;
		UInt32 MD5_T34  = 0x8771f681;
		UInt32 MD5_T35  = 0x6d9d6122;
		UInt32 MD5_T36  = 0xfde5380c;
		UInt32 MD5_T37  = 0xa4beea44;
		UInt32 MD5_T38  = 0x4bdecfa9;
		UInt32 MD5_T39  = 0xf6bb4b60;
		UInt32 MD5_T40  = 0xbebfbc70;
		UInt32 MD5_T41  = 0x289b7ec6;
		UInt32 MD5_T42  = 0xeaa127fa;
		UInt32 MD5_T43  = 0xd4ef3085;
		UInt32 MD5_T44  = 0x04881d05;
		UInt32 MD5_T45  = 0xd9d4d039;
		UInt32 MD5_T46  = 0xe6db99e5;
		UInt32 MD5_T47  = 0x1fa27cf8;
		UInt32 MD5_T48  = 0xc4ac5665;
		UInt32 MD5_T49  = 0xf4292244;
		UInt32 MD5_T50  = 0x432aff97;
		UInt32 MD5_T51  = 0xab9423a7;
		UInt32 MD5_T52  = 0xfc93a039;
		UInt32 MD5_T53  = 0x655b59c3;
		UInt32 MD5_T54  = 0x8f0ccc92;
		UInt32 MD5_T55  = 0xffeff47d;
		UInt32 MD5_T56  = 0x85845dd1;
		UInt32 MD5_T57  = 0x6fa87e4f;
		UInt32 MD5_T58  = 0xfe2ce6e0;
		UInt32 MD5_T59  = 0xa3014314;
		UInt32 MD5_T60  = 0x4e0811a1;
		UInt32 MD5_T61  = 0xf7537e82;
		UInt32 MD5_T62  = 0xbd3af235;
		UInt32 MD5_T63  = 0x2ad7d2bb;
		UInt32 MD5_T64  = 0xeb86d391;

		public MD5Algo(Progress pcbf)
		{
			ProgressCB = pcbf;

			MD5 = new UInt32[4];
			MD5[0] = 0x67452301;
			MD5[1] = 0xefcdab89;
			MD5[2] = 0x98badcfe;
			MD5[3] = 0x10325476;

			BitsCount = new UInt32[2];
			BitsCount[0] = 0;
			BitsCount[1] = 0;

			CalcBuffer = new byte[64];
			for(int i=0;i<CalcBuffer.Length;i++)
			{
				CalcBuffer[i] = 0;
			}

			padd = new byte[64];
			for(int i=0;i<padd.Length;i++)
			{
				padd[i] = 0;
			}
			padd[0] = 0x80;

		}
		public string CalculateMD5(string Filename)
		{
			FileStream Datei = null;
			byte[] buffer = new byte[1024];

			if(!File.Exists(Filename))
			{
				return null;
			}
			
			try
			{
				Datei = new FileStream(Filename,FileMode.Open,FileAccess.Read);
			}
			catch
			{
				return null;
			}

			long len = Datei.Length;
			long offset = 0;
			long readlen;
			int Percentage = 0;

			while(offset<len)
			{
				try
				{
					readlen = Datei.Read(buffer,0,buffer.Length);
				}
				catch
				{
					return null;
				}
				Update(buffer,(uint)readlen);
				offset += readlen;
				Percentage = (int)((double)(readlen *10000)/(double)len);
				if(ProgressCB!=null)
				{
					ProgressCB(Percentage);
				}
			}
			string res = Finalize();
			Datei.Close();

			return res;
		}

		private string Finalize()
		{
			byte[] Bits = new byte[8];
			Bits = UInt322Byte(BitsCount,0);

			uint nIndex = (uint)((BitsCount[0] >> 3) & 0x3f);
			uint nPadLen = (nIndex < 56) ? (56 - nIndex) : (120 - nIndex);

			Update(padd,nPadLen);
			Update(Bits,(uint)Bits.Length);

			byte[] lpszMD5 = new byte[16];
			lpszMD5 = UInt322Byte(MD5,0);

			string strMD5 = "";
			for ( int i=0; i < lpszMD5.Length; i++) 
			{
				string Str = "";
				if (lpszMD5[i] == 0) 
				{
					Str = "00";
				}
				else if (lpszMD5[i] <= 15) 	
				{
					Str = String.Format("{0:x2}",lpszMD5[i]);
				}
				else 
				{
					Str = String.Format("{0:x2}",lpszMD5[i]);
				}
				strMD5 += Str;
			}
			return strMD5;
		}

		private void Update(byte[] Input,uint InputLen)
		{
			uint nIndex = (uint)((BitsCount[0] >> 3) & 0x3F);

			if((BitsCount[0] += InputLen << 3 )  <  ( InputLen << 3) )
			{
				BitsCount[1]++;
			}
			BitsCount[1] += (InputLen >> 29);

			uint i=0;		
			uint nPartLen = 64 - nIndex;
			if (InputLen >= nPartLen) 	
			{
				MyMemCpy(CalcBuffer,nIndex,Input,0,nPartLen);
				Transform( CalcBuffer ,0);
				for (i = nPartLen; i + 63 < InputLen; i += 64) 
				{
					Transform(Input,i);
				}
				nIndex = 0;
			} 
			else 
			{
				i = 0;
			}
			MyMemCpy(CalcBuffer,nIndex,Input,i,InputLen-i);
		}
		
		private void MyMemCpy(byte[] DestBuf,uint DestAtPos, byte[] SrcBuf, uint SrcAtPos,uint CpyLen)
		{
			if((DestBuf.Length<DestAtPos+CpyLen) || (SrcBuf.Length<SrcAtPos+CpyLen))
			{
				return;
			}
			for(uint i=0;i<CpyLen;i++)
			{
				DestBuf[DestAtPos+i]=SrcBuf[SrcAtPos+i];
			}
		}

		private UInt32 RotateLeft(UInt32 x,int n)
		{
			return (x << n) | (x >> (32-n));
		}

		private void Transform_FF(ref UInt32 A,UInt32 B,UInt32 C,UInt32 D,UInt32 X,int S,UInt32 T)
		{
			UInt32 F = (B & C) | (~B & D);
			A += F + X + T;
			A = RotateLeft(A,S);
			A += B;
		}

		private void Transform_GG(ref UInt32 A,UInt32 B,UInt32 C,UInt32 D,UInt32 X,int S,UInt32 T)
		{
			UInt32 G = (B & D) | (C & ~D);
			A += G + X + T;
			A = RotateLeft(A, S);
			A += B;
		}

		private void Transform_HH(ref UInt32 A,UInt32 B,UInt32 C,UInt32 D,UInt32 X,int S,UInt32 T)
		{
			UInt32 H = (B ^ C ^ D);
			A += H + X + T;
			A = RotateLeft(A, S);
			A += B;
		}

		private void Transform_II(ref UInt32 A,UInt32 B,UInt32 C,UInt32 D,UInt32 X,int S,UInt32 T)
		{
			UInt32 I = (C ^ (B | ~D));
			A += I + X + T;
			A = RotateLeft(A, S);
			A += B;
		}

		private UInt32[] Byte2UInt32(byte[] Input,uint AtPos,int len)
		{
			int UInt32Count = len/4;

			UInt32[] Output = new UInt32[UInt32Count];
			for(int i=0;i<UInt32Count;i++)
			{
				Output[i] = (UInt32)Input[AtPos+i*4] | 
					(UInt32)Input[AtPos+i*4+1] << 8	| 
					(UInt32)Input[AtPos+i*4+2] << 16 | 
					(UInt32)Input[AtPos+i*4+3] << 24;
			}
			return Output;
		}

		private byte[] UInt322Byte(UInt32[] Input,uint AtPos)
		{
			int ByteCount = Input.Length*4;

			byte[] Output = new byte[ByteCount];
			for(int i=0;i<Input.Length;i++)
			{
				Output[i*4] =   (byte)(Input[AtPos+i] & 0xff);
				Output[i*4+1] = (byte)((Input[AtPos+i] >> 8) & 0xff);
				Output[i*4+2] = (byte)((Input[AtPos+i] >> 16) & 0xff);
				Output[i*4+3] = (byte)((Input[AtPos+i] >> 24) & 0xff);
			}
			return Output;
		}

		private void Transform(byte[] Block,uint pos)
		{
			UInt32 a = (UInt32)MD5[0];
			UInt32 b = (UInt32)MD5[1];
			UInt32 c = (UInt32)MD5[2];
			UInt32 d = (UInt32)MD5[3];

			UInt32[] X = new UInt32[16];
			X = Byte2UInt32(Block,pos,64);

			Transform_FF (ref a, b, c, d, X[ 0], MD5_S11, MD5_T01); 
			Transform_FF (ref d, a, b, c, X[ 1], MD5_S12, MD5_T02); 
			Transform_FF (ref c, d, a, b, X[ 2], MD5_S13, MD5_T03); 
			Transform_FF (ref b, c, d, a, X[ 3], MD5_S14, MD5_T04); 
			Transform_FF (ref a, b, c, d, X[ 4], MD5_S11, MD5_T05); 
			Transform_FF (ref d, a, b, c, X[ 5], MD5_S12, MD5_T06); 
			Transform_FF (ref c, d, a, b, X[ 6], MD5_S13, MD5_T07); 
			Transform_FF (ref b, c, d, a, X[ 7], MD5_S14, MD5_T08); 
			Transform_FF (ref a, b, c, d, X[ 8], MD5_S11, MD5_T09); 
			Transform_FF (ref d, a, b, c, X[ 9], MD5_S12, MD5_T10); 
			Transform_FF (ref c, d, a, b, X[10], MD5_S13, MD5_T11); 
			Transform_FF (ref b, c, d, a, X[11], MD5_S14, MD5_T12); 
			Transform_FF (ref a, b, c, d, X[12], MD5_S11, MD5_T13); 
			Transform_FF (ref d, a, b, c, X[13], MD5_S12, MD5_T14); 
			Transform_FF (ref c, d, a, b, X[14], MD5_S13, MD5_T15); 
			Transform_FF (ref b, c, d, a, X[15], MD5_S14, MD5_T16); 

			Transform_GG (ref a, b, c, d, X[ 1], MD5_S21, MD5_T17); 
			Transform_GG (ref d, a, b, c, X[ 6], MD5_S22, MD5_T18); 
			Transform_GG (ref c, d, a, b, X[11], MD5_S23, MD5_T19); 
			Transform_GG (ref b, c, d, a, X[ 0], MD5_S24, MD5_T20); 
			Transform_GG (ref a, b, c, d, X[ 5], MD5_S21, MD5_T21); 
			Transform_GG (ref d, a, b, c, X[10], MD5_S22, MD5_T22); 
			Transform_GG (ref c, d, a, b, X[15], MD5_S23, MD5_T23); 
			Transform_GG (ref b, c, d, a, X[ 4], MD5_S24, MD5_T24); 
			Transform_GG (ref a, b, c, d, X[ 9], MD5_S21, MD5_T25); 
			Transform_GG (ref d, a, b, c, X[14], MD5_S22, MD5_T26); 
			Transform_GG (ref c, d, a, b, X[ 3], MD5_S23, MD5_T27); 
			Transform_GG (ref b, c, d, a, X[ 8], MD5_S24, MD5_T28); 
			Transform_GG (ref a, b, c, d, X[13], MD5_S21, MD5_T29); 
			Transform_GG (ref d, a, b, c, X[ 2], MD5_S22, MD5_T30); 
			Transform_GG (ref c, d, a, b, X[ 7], MD5_S23, MD5_T31); 
			Transform_GG (ref b, c, d, a, X[12], MD5_S24, MD5_T32); 

			Transform_HH (ref a, b, c, d, X[ 5], MD5_S31, MD5_T33); 
			Transform_HH (ref d, a, b, c, X[ 8], MD5_S32, MD5_T34); 
			Transform_HH (ref c, d, a, b, X[11], MD5_S33, MD5_T35); 
			Transform_HH (ref b, c, d, a, X[14], MD5_S34, MD5_T36); 
			Transform_HH (ref a, b, c, d, X[ 1], MD5_S31, MD5_T37); 
			Transform_HH (ref d, a, b, c, X[ 4], MD5_S32, MD5_T38); 
			Transform_HH (ref c, d, a, b, X[ 7], MD5_S33, MD5_T39); 
			Transform_HH (ref b, c, d, a, X[10], MD5_S34, MD5_T40); 
			Transform_HH (ref a, b, c, d, X[13], MD5_S31, MD5_T41); 
			Transform_HH (ref d, a, b, c, X[ 0], MD5_S32, MD5_T42); 
			Transform_HH (ref c, d, a, b, X[ 3], MD5_S33, MD5_T43); 
			Transform_HH (ref b, c, d, a, X[ 6], MD5_S34, MD5_T44); 
			Transform_HH (ref a, b, c, d, X[ 9], MD5_S31, MD5_T45); 
			Transform_HH (ref d, a, b, c, X[12], MD5_S32, MD5_T46); 
			Transform_HH (ref c, d, a, b, X[15], MD5_S33, MD5_T47); 
			Transform_HH (ref b, c, d, a, X[ 2], MD5_S34, MD5_T48); 

			Transform_II (ref a, b, c, d, X[ 0], MD5_S41, MD5_T49); 
			Transform_II (ref d, a, b, c, X[ 7], MD5_S42, MD5_T50); 
			Transform_II (ref c, d, a, b, X[14], MD5_S43, MD5_T51); 
			Transform_II (ref b, c, d, a, X[ 5], MD5_S44, MD5_T52); 
			Transform_II (ref a, b, c, d, X[12], MD5_S41, MD5_T53); 
			Transform_II (ref d, a, b, c, X[ 3], MD5_S42, MD5_T54); 
			Transform_II (ref c, d, a, b, X[10], MD5_S43, MD5_T55); 
			Transform_II (ref b, c, d, a, X[ 1], MD5_S44, MD5_T56); 
			Transform_II (ref a, b, c, d, X[ 8], MD5_S41, MD5_T57); 
			Transform_II (ref d, a, b, c, X[15], MD5_S42, MD5_T58); 
			Transform_II (ref c, d, a, b, X[ 6], MD5_S43, MD5_T59); 
			Transform_II (ref b, c, d, a, X[13], MD5_S44, MD5_T60); 
			Transform_II (ref a, b, c, d, X[ 4], MD5_S41, MD5_T61); 
			Transform_II (ref d, a, b, c, X[11], MD5_S42, MD5_T62); 
			Transform_II (ref c, d, a, b, X[ 2], MD5_S43, MD5_T63); 
			Transform_II (ref b, c, d, a, X[ 9], MD5_S44, MD5_T64); 

			MD5[0] += a;
			MD5[1] += b;
			MD5[2] += c;
			MD5[3] += d;
		}
	}
}