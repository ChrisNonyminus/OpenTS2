using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using OpenTS2.Files.Utils;
using OpenTS2.Content;
using OpenTS2.Common;
using OpenTS2.Content.DBPF;
using OpenTS2.Game.Reimpl;
using System.Text;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.LOT)]
    public class LOTCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var asset = new LotAsset();
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            asset.FileName = /*reader.ReadSevenBitString();*/sourceFile.FilePath;
            reader.Seek(SeekOrigin.Begin, 64);
            asset.Version = reader.ReadInt16();
            asset.LotSize1 = reader.ReadUInt32();
            asset.LotSize2 = reader.ReadUInt32();
            asset.LotType = (TSBaseLotInfo.ZoneType)reader.ReadByte();
            asset.Roads = reader.ReadByte();
            asset.Rotation = reader.ReadByte();
            asset.Flags = reader.ReadUInt32();
            asset.LotName = reader.ReadSevenBitString();
            asset.LotDescription = reader.ReadSevenBitString();
            return asset;
        }
    }
    [Codec(TypeIDs.OBJT)]
    public class OBJTCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var asset = new Game.Reimpl.TSSG.Object();
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            reader.Seek(SeekOrigin.Begin, 64);
            asset.RecordType = reader.ReadUInt32();
            asset.Version = reader.ReadUInt32();
            asset.TypeName = reader.ReadPascalString();
            //Debug.Assert(asset.TypeName == "cObject");
            if (asset.TypeName == "cObject")
            {
                asset.ModelName = reader.ReadSevenBitString();
                uint numEntries = reader.ReadUInt32();
                asset.Entries = new List<Game.Reimpl.TSSG.Object.Entry>();
                for (int i = 0; i < numEntries; i++)
                {
                    var entry = new Game.Reimpl.TSSG.Object.Entry();
                    entry.Name = reader.ReadPascalString();
                    if (asset.Version == 0x11)
                        entry.NameType = reader.ReadUInt32();
                    entry.SubEntries = new List<Game.Reimpl.TSSG.Object.Entry.SubEntry>();
                    var numSubEntries = reader.ReadUInt32();
                    for (int j = 0; j < numSubEntries; j++)
                    {
                        var subEntry = new Game.Reimpl.TSSG.Object.Entry.SubEntry();
                        subEntry.LongName = reader.ReadPascalString();
                        subEntry.ShortName = reader.ReadPascalString();

                        entry.SubEntries.Add(subEntry);
                    }
                    asset.Entries.Add(entry);
                }
                var mainCoords = new Game.Reimpl.TSSG.Object.MainCoords();
                mainCoords.XCoord = reader.ReadFloat();
                mainCoords.YCoord = reader.ReadFloat();
                mainCoords.Height = reader.ReadFloat();
                mainCoords.Rotation = new Vector4(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
                var numCresNodes = reader.ReadUInt32();
                mainCoords.CRESNodeEntries = new List<Game.Reimpl.TSSG.Object.MainCoords.CRESNodeEntryMaybe>();
                for (int i = 0; i < numCresNodes; i++)
                {
                    var cresNodeEntry = new Game.Reimpl.TSSG.Object.MainCoords.CRESNodeEntryMaybe();
                    cresNodeEntry.NodeName = reader.ReadPascalString();
                    cresNodeEntry.XCoord = reader.ReadFloat();
                    cresNodeEntry.YCoord = reader.ReadFloat();
                    cresNodeEntry.Height = reader.ReadFloat();
                    cresNodeEntry.Rotation = new Vector4(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
                    mainCoords.CRESNodeEntries.Add(cresNodeEntry);
                }
                if (asset.Version >= 0x10)
                {
                    var numBlends = reader.ReadUInt32();
                    mainCoords.BlendPairEntries = new List<Game.Reimpl.TSSG.Object.MainCoords.BlendPairEntry>();
                    for (int i = 0; i < numBlends; i++)
                    {
                        var blendPairEntry = new Game.Reimpl.TSSG.Object.MainCoords.BlendPairEntry();
                        blendPairEntry.BlendName = reader.ReadPascalString();
                        blendPairEntry.BlendPartner = reader.ReadPascalString();
                        blendPairEntry.TerminalZeroes = reader.ReadUInt32();
                        mainCoords.BlendPairEntries.Add(blendPairEntry);
                    }
                    asset.Coords = mainCoords;
                }
            }
            return asset;
        }
    }
}
