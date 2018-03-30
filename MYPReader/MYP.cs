using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Csharp.WorldBuilder
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FileInArchiveDescriptor
    {
        public byte[] metadata;
        public byte[] Metadata
        {
            get { return metadata; }
            set { metadata = value; }
        }
        public static int fileDescriptorSize = 34;

        #region Variables

        private long fileTableEntryPosition;

        public long FileTableEntryPosition
        {
            get { return fileTableEntryPosition; }
            set { fileTableEntryPosition = value; }
        }
        private long startingPosition;

        public long StartingPosition
        {
            get { return startingPosition; }
            set { startingPosition = value; }
        }
        private uint fileHeaderSize;

        public uint FileHeaderSize
        {
            get { return fileHeaderSize; }
            set { fileHeaderSize = value; }
        }
        private uint compressedSize;

        public uint CompressedSize
        {
            get { return compressedSize; }
            set { compressedSize = value; }
        }
        private uint uncompressedSize;

        public uint UncompressedSize
        {
            get { return uncompressedSize; }
            set { uncompressedSize = value; }
        }
        private byte compressionMethod;

        public byte CompressionMethod
        {
            get { return compressionMethod; }
            set { compressionMethod = value; }
        }
        private int crc = 0;

        public int Crc
        {
            get { return crc; }
            set { crc = value; }
        }
        private byte[] file_hash = new byte[8];

        public byte[] File_hash
        {
            get { return file_hash; }
            set { file_hash = value; }
        }
        public uint ph, sh;
        public string strUTF8;
        public string strUTF16;
        private bool isCompressed;

        public bool IsCompressed
        {
            get { return isCompressed; }
            set { isCompressed = value; }
        }

        #endregion

        public FileInArchiveDescriptor() { }


        public FileInArchiveDescriptor(byte[] buffer)
        {
            startingPosition = convertLittleEndianBufferToInt(buffer, 0); //Last 32 bits
            long startingPosition65536 = convertLittleEndianBufferToInt(buffer, 4); //Fisrt 32 bits
            startingPosition += startingPosition65536 << 32; //Real starting position

            if (startingPosition == 52698228)
            {
                int i = 0;
            }
            fileHeaderSize = convertLittleEndianBufferToInt(buffer, 8);

            compressedSize = convertLittleEndianBufferToInt(buffer, 12);
            uncompressedSize = convertLittleEndianBufferToInt(buffer, 16);

            Array.Copy(buffer, 20, file_hash, 0, 8);

            sh = convertLittleEndianBufferToInt(buffer, 20);
            ph = convertLittleEndianBufferToInt(buffer, 24);

            crc = BitConverter.ToInt32(buffer, 28);
            compressionMethod = buffer[32];
            isCompressed = (compressionMethod == 0) ? false : true;
        }

        public static uint convertLittleEndianBufferToInt(byte[] intBuffer, long offset)
        {
            uint result = 0;
            for (int i = 3; i >= 0; i--)
            {
                result = result << 8;
                result += intBuffer[offset + i];
            }
            return result;
        }
    }
    public class FileInArchive
    {
        public FileInArchiveDescriptor Descriptor = new FileInArchiveDescriptor();
  
        public byte[] data;
        public byte[] data_start_200 = new byte[200];

        #region Properties
        public long Offset { get { return Descriptor.StartingPosition; } }
        public uint Size { get { return Descriptor.UncompressedSize; } }
        public uint CompressedSize { get { return Descriptor.CompressedSize; } }
        public byte CompressionMethod { get { return Descriptor.CompressionMethod; } }


        #endregion
    }


    public class MYP
    {
      
        private uint _fileCount;
        private Dictionary<long, FileInArchive> _files = new Dictionary<long, FileInArchive>();

        public Dictionary<long, FileInArchive> Files
        {
            get { return _files; }
            set { _files = value; }
        }

        public void Load(Stream stream)
        {
            //read the position of the starting file table
            stream.Seek(0x0C, SeekOrigin.Begin);
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, buffer.Length);
            long tableStart = FileInArchiveDescriptor.convertLittleEndianBufferToInt(buffer, 0);
            tableStart += ((long)FileInArchiveDescriptor.convertLittleEndianBufferToInt(buffer, 4)) << 32;

            //get file count
            stream.Seek(24, SeekOrigin.Begin);
            buffer = new byte[4];
            stream.Read(buffer, 0, buffer.Length);

            _fileCount = FileInArchiveDescriptor.convertLittleEndianBufferToInt(buffer, 0);

            //Init
            long currentReadingPosition;
            uint numberOfFileInTable = 0;
            long endOfTableAddress;
            byte[] bufferTableHeader = new byte[12];
            byte[] bufferFileDesc = new byte[FileInArchiveDescriptor.fileDescriptorSize];
            FileInArchive myArchFile;

            while (tableStart != 0)
            {
                stream.Seek(tableStart, SeekOrigin.Begin);
                stream.Read(bufferTableHeader, 0, bufferTableHeader.Length);

                numberOfFileInTable = FileInArchiveDescriptor.convertLittleEndianBufferToInt(bufferTableHeader, 0); //get number of files

                currentReadingPosition = tableStart + 12;
                endOfTableAddress = tableStart + 12 + (long)FileInArchiveDescriptor.fileDescriptorSize * (long)numberOfFileInTable; // calculates the end address

                tableStart = FileInArchiveDescriptor.convertLittleEndianBufferToInt(bufferTableHeader, 4); //find the next filetable
                tableStart += (long)FileInArchiveDescriptor.convertLittleEndianBufferToInt(bufferTableHeader, 8) << 32; //mostly 0

                while (currentReadingPosition < endOfTableAddress)
                {
                    stream.Seek(currentReadingPosition, SeekOrigin.Begin);
                    stream.Read(bufferFileDesc, 0, bufferFileDesc.Length);

                    myArchFile = new FileInArchive();
                    myArchFile.Descriptor = new FileInArchiveDescriptor(bufferFileDesc);

                    myArchFile.Descriptor.FileTableEntryPosition = currentReadingPosition;

                    if (myArchFile.Descriptor.StartingPosition > 0
                        && myArchFile.Descriptor.CompressedSize > 0
                        && myArchFile.Descriptor.UncompressedSize > 0 //If the compressed size is 0, then there is no file
                        )
                    {

                        _files[((long)myArchFile.Descriptor.ph << 32) + myArchFile.Descriptor.sh] = myArchFile;

                        //Retrieve header
                        myArchFile.Descriptor.metadata = new byte[myArchFile.Descriptor.FileHeaderSize];


                        stream.Seek(myArchFile.Descriptor.StartingPosition, SeekOrigin.Begin);
                        stream.Read(myArchFile.Descriptor.metadata, 0, myArchFile.Descriptor.metadata.Length);
                    }
                    currentReadingPosition += FileInArchiveDescriptor.fileDescriptorSize;
                }
            }
        }



        public byte[] ReadFile(Stream outstream,  FileInArchive file)
        {
            if (file.Descriptor.CompressionMethod == 1) //ZLib compression
            {
                outstream.Position = file.Descriptor.StartingPosition + file.Descriptor.FileHeaderSize;
                byte[] compressedData = new byte[file.Descriptor.CompressedSize];
                outstream.Read(compressedData, 0, (int)file.Descriptor.CompressedSize);

                byte[] output_buffer = new byte[file.Descriptor.UncompressedSize];

                ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
                inf.SetInput(compressedData);
                inf.Inflate(output_buffer);
                return output_buffer;
            }
            else 
            {
                outstream.Position = file.Descriptor.StartingPosition + file.Descriptor.FileHeaderSize;
                byte[] compressedData = new byte[file.Descriptor.CompressedSize];
                outstream.Read(compressedData, 0, (int)file.Descriptor.CompressedSize);

                return compressedData;
            }
        }
        public void WriteFile(Stream outstream, FileInArchive file, byte[] data)
        {
            MemoryStream inputMS = new MemoryStream(data);
            MemoryStream outputMS = new MemoryStream();

            byte[] output_buffer = new byte[0];

            if (file.Descriptor.CompressionMethod == 1) //ZLib compression
            {
                ICSharpCode.SharpZipLib.Zip.Compression.Deflater def = new ICSharpCode.SharpZipLib.Zip.Compression.Deflater();
                ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream defstream = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream(outputMS, def);
                defstream.Write(data, 0, data.Length);
                defstream.Flush();
                defstream.Finish();
                output_buffer = outputMS.GetBuffer();
                file.Descriptor.UncompressedSize = (uint)inputMS.Length;

                Write(outstream, file, outputMS);
            }
            else if (file.Descriptor.CompressionMethod == 0 ) //No compression
            {
                file.Descriptor.CompressionMethod = 0;
                inputMS.CopyTo(outputMS);
                file.Descriptor.UncompressedSize = (uint)inputMS.Length;

                Write(outstream, file, outputMS);
            }
        }
        public static long HashWAR(string s)
        {
            uint ph = 0, sh = 0;
            HashWAR(s, 0xDEADBEEF, out ph, out sh);
            return ((long)ph << 32) + sh;
        }
        public static void HashWAR(string s, uint seed, out uint ph, out uint sh)
        {
            uint edx = 0, eax, esi, ebx = 0;
            uint edi, ecx;


            eax = ecx = edx = ebx = esi = edi = 0;
            ebx = edi = esi = (uint)s.Length + seed;

            int i = 0;

            for (i = 0; i + 12 < s.Length; i += 12)
            {
                edi = (uint)((s[i + 7] << 24) | (s[i + 6] << 16) | (s[i + 5] << 8) | s[i + 4]) + edi;
                esi = (uint)((s[i + 11] << 24) | (s[i + 10] << 16) | (s[i + 9] << 8) | s[i + 8]) + esi;
                edx = (uint)((s[i + 3] << 24) | (s[i + 2] << 16) | (s[i + 1] << 8) | s[i]) - esi;

                edx = (edx + ebx) ^ (esi >> 28) ^ (esi << 4);
                esi += edi;
                edi = (edi - edx) ^ (edx >> 26) ^ (edx << 6);
                edx += esi;
                esi = (esi - edi) ^ (edi >> 24) ^ (edi << 8);
                edi += edx;
                ebx = (edx - esi) ^ (esi >> 16) ^ (esi << 16);
                esi += edi;
                edi = (edi - ebx) ^ (ebx >> 13) ^ (ebx << 19);
                ebx += esi;
                esi = (esi - edi) ^ (edi >> 28) ^ (edi << 4);
                edi += ebx;
            }

            if (s.Length - i > 0)
            {
                switch (s.Length - i)
                {
                    case 12:
                        esi += (uint)s[i + 11] << 24;
                        goto case 11;
                    case 11:
                        esi += (uint)s[i + 10] << 16;
                        goto case 10;
                    case 10:
                        esi += (uint)s[i + 9] << 8;
                        goto case 9;
                    case 9:
                        esi += (uint)s[i + 8];
                        goto case 8;
                    case 8:
                        edi += (uint)s[i + 7] << 24;
                        goto case 7;
                    case 7:
                        edi += (uint)s[i + 6] << 16;
                        goto case 6;
                    case 6:
                        edi += (uint)s[i + 5] << 8;
                        goto case 5;
                    case 5:
                        edi += (uint)s[i + 4];
                        goto case 4;
                    case 4:
                        ebx += (uint)s[i + 3] << 24;
                        goto case 3;
                    case 3:
                        ebx += (uint)s[i + 2] << 16;
                        goto case 2;
                    case 2:
                        ebx += (uint)s[i + 1] << 8;
                        goto case 1;
                    case 1:
                        ebx += (uint)s[i];
                        break;
                }

                esi = (esi ^ edi) - ((edi >> 18) ^ (edi << 14));
                ecx = (esi ^ ebx) - ((esi >> 21) ^ (esi << 11));
                edi = (edi ^ ecx) - ((ecx >> 7) ^ (ecx << 25));
                esi = (esi ^ edi) - ((edi >> 16) ^ (edi << 16));
                edx = (esi ^ ecx) - ((esi >> 28) ^ (esi << 4));
                edi = (edi ^ edx) - ((edx >> 18) ^ (edx << 14));
                eax = (esi ^ edi) - ((edi >> 8) ^ (edi << 24));

                ph = edi;
                sh = eax;
                return;
            }
            ph = esi;
            sh = eax;
            return;
        }

        public FileInArchive GetByHash(long key)
        {
            if (_files.ContainsKey(key))
                return _files[key];
            return null;
        }

        public FileInArchive GetByHash(uint ph, uint sh)
        {
            long key = ((long)ph << 32) + sh;
            if (_files.ContainsKey(key))
                return _files[key];
            return null;
        }

        public FileInArchive GetByFilename(string filename)
        {
            uint ph = 0, sh = 0;

            HashWAR(filename, 0xDEADBEEF, out ph, out sh);

            long key = ((long)ph << 32) + sh;
            if (_files.ContainsKey(key))
                return _files[key];
            return null;
        }

        private void Write(Stream stream, FileInArchive file, MemoryStream ms)
        {
            stream.Seek((long)file.Descriptor.FileTableEntryPosition + 12, SeekOrigin.Begin);
            int lowMSLength = (int)(ms.Length & 0xFFFFFFFF);
            stream.Write(BitConverter.GetBytes(lowMSLength), 0, 4);

            stream.Seek((long)file.Descriptor.FileTableEntryPosition + 16, SeekOrigin.Begin);
            lowMSLength = (int)(file.Descriptor.UncompressedSize & 0xFFFFFFFF);
            stream.Write(BitConverter.GetBytes(lowMSLength), 0, 4);

            stream.Seek((long)file.Descriptor.FileTableEntryPosition + 32, SeekOrigin.Begin);
            byte[] bArray = new byte[1];
            bArray[0] = (byte)file.CompressionMethod;
            stream.Write(bArray, 0, 1);


            byte[] tmp_bytearray = ms.GetBuffer();
            if (ms.Length <= file.Descriptor.CompressedSize)
            {
                stream.Seek((long)(file.Descriptor.StartingPosition + file.Descriptor.FileHeaderSize), SeekOrigin.Begin);
                stream.Write(tmp_bytearray, 0, (int)ms.Length);
            }
            else
            {
                long fileSize = stream.Length;

                stream.Seek(0, SeekOrigin.End);
                if (file.Descriptor.metadata != null)
                {
                    stream.Write(file.Descriptor.metadata, 0, file.Descriptor.metadata.Length);
                }
                else
                {
                    byte[] fakeMetadata = new byte[file.Descriptor.FileHeaderSize];
                    stream.Write(fakeMetadata, 0, fakeMetadata.Length);
                    //Should throw an exception, but not all files have meta data so...
                }
                stream.Seek(0, SeekOrigin.End);
                stream.Write(tmp_bytearray, 0, (int)ms.Length);

                stream.Seek((long)file.Descriptor.FileTableEntryPosition, SeekOrigin.Begin);
                stream.Write(BitConverter.GetBytes((int)(fileSize & 0xFFFFFFFF)), 0, 4);

                stream.Seek((long)file.Descriptor.FileTableEntryPosition + 4, SeekOrigin.Begin);
                stream.Write(BitConverter.GetBytes((int)((fileSize >> 32) & 0xFFFFFFFF)), 0, 4);

                file.Descriptor.StartingPosition = (int)(fileSize & 0xFFFFFFFF);
            }

            file.Descriptor.CompressedSize = (uint)ms.Length;
            tmp_bytearray = null;

        }
    }
}
