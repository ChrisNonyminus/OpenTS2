﻿using System;
using System.Linq;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// A scenegraph cAnimResoureConst block. This represents a single animation and maybe some events on it like
    /// sound effects. The const here refers to the fact that the data in here cannot be changed in the sims, it
    /// is a read-only structure.
    /// </summary>
    public class AnimResourceConstBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0xfb00791e;
        public const string BLOCK_NAME = "cAnimResourceConst";
        public override string BlockName => BLOCK_NAME;

        /// <summary>
        /// This is the constant used by Sims 2 to convert between tick durations in the animations to their number of
        /// frames.
        /// </summary>
        public const double FramesPerTick = 0.03;

        public ScenegraphResource ScenegraphResource { get; }

        public byte LocomotionType { get; }
        public float HeadingOffset { get; }
        public float TurnRotation { get; }

        public float LocomotionDistance { get; }

        /// <summary>
        /// Units are likely meters per tick.
        /// </summary>
        public float Velocity { get; }

        public AnimTarget[] AnimTargets { get; }

        public AnimResourceConstBlock(PersistTypeInfo blockTypeInfo, ScenegraphResource scenegraphResource,
            byte locomotionType, float headingOffset, float locomotionDistance, float velocity, float turnRotation,
            AnimTarget[] animTargets) : base(blockTypeInfo)
        {
            ScenegraphResource = scenegraphResource;
            LocomotionType = locomotionType;
            HeadingOffset = headingOffset;
            TurnRotation = turnRotation;
            AnimTargets = animTargets;
            Velocity = velocity;
            LocomotionDistance = locomotionDistance;
        }

        /// <summary>
        /// A single morph target or bone to be controlled by the animation. Each target can have multiple channels
        /// inside that control rotation, position, etc.
        /// </summary>
        public class AnimTarget
        {
            public string TagName;

            public int NumSharedChannels;
            public SharedChannel[] Channels;

            public int NumIKChains;
            public IKChain[] IKChains;
        }

        /// <summary>
        /// Inverse kinematics chain.
        /// </summary>
        public class IKChain
        {
            public int NumIkTargets;
            public Target[] IkTargets;

            public uint NameCrc;
            public uint NameMirrorCrc;
            public uint BeginBoneCrc;
            public uint BeginBoneMirrorCrc;
            public IKStrategy IkStrategy;
            public uint EndBoneCrc;
            public uint EndBoneMirrorCrc;
            public uint TwistVectorCrc;
            public uint TwistVectorBoneCrc;
            public uint TwistVectorMirrorBoneCRC;
            /// <summary>
            /// CRC of the channel name that determines the IK's weight.
            /// </summary>
            public uint IkWeightCRC;

            public class Target
            {
                public uint BoneCrc;
                public uint BoneMirrorCrc;
                public uint RotationCrc;
                public uint Rotation2Crc;
                public uint TranslationCrc;
                public uint ContactCrc;
            }
        }

        public enum IKStrategy : uint
        {
            SevenDegreesOfFreedom = 0x48ee7a41,
            LookAt = 0x9f9e87e,
            LookAtDog = 0xfa99547,
        }

        /// <summary>
        /// Controls a single aspect of a single target's animation such as the rotation of a single bone.
        /// </summary>
        public class SharedChannel
        {
            public string ChannelName;

            public uint BoneHash;

            #region ChannelFlagsAndDerivedProperties
            public uint ChannelFlags;


            public uint DurationTicks => ChannelFlags & 0x7FFF; // Bottom 15 bits give the channel duration.

            private uint AnimatedAttributeIdx => (ChannelFlags >> 17) & 0b11111; // Bits 1 to 6 of the 3rd byte.
            /// <summary>
            /// Which attribute of the bone this channel animates, e.g the translation or the rotation.
            ///
            /// For IK channels, this can be more complicated such as the IK weight, the IK rotation etc.
            /// </summary>
            public AnimatedAttribute AnimatedAttribute => Enum.IsDefined(typeof(AnimatedAttribute), AnimatedAttributeIdx)
                ? (AnimatedAttribute)AnimatedAttributeIdx
                : throw new InvalidCastException($"Animated attribute idx has invalid value: {AnimatedAttributeIdx}");

            private uint ChannelTypeBits => (ChannelFlags >> 22) & 0b111; // Top 2 bits of 3rd byte and 1st bit of 4th byte.
            // What type of data this channel carries.
            public ChannelType Type => Enum.IsDefined(typeof(ChannelType), ChannelTypeBits)
                ? (ChannelType)ChannelTypeBits
                : throw new InvalidCastException($"Channel type has invalid value: {ChannelTypeBits}");

            private uint IKChainIdxBits => (ChannelFlags >> 25) & 0b1111; // Bits 1 to 5 of the most-significant byte.
            public int IKChainIdx => IKChainIdxBits == 0b1111 ? -1 : (int)IKChainIdxBits;

            public uint NumComponents => (ChannelFlags >> 29) & 0b111; // Top 3 bits of the most-significant byte.

            #endregion

            public ChannelComponent[] Components;
        }

        public enum ChannelType : uint
        {
            Float1 = 0,
            Float3 = 1,
            Float4 = 2,
            // This is called "ChannelTypeQ" for quaternion but the data is encoded as euler angles...
            EulerRotation = 3,
            TransformXYZ = 4,
            Float2 = 5,
        }

        public enum AnimatedAttribute : uint
        {
            // Attribute names and their GUIDs.
            Rotation = 0,    // 9B5B3506 - cTransformNode rotation
            Transform = 1,   // FB5B3383 - cTransformNode transform
            MorphWeight = 2, // 69C7EB40 - kMorphWeight
            ContactIk = 3,   // E9C7F1A6
            WeightIk = 4,    // E9C7F1AD
            Unknown5 = 5,    // 6B312A30
            MaterialFloat1 = 6, // 09F5FDB0
            MaterialFloat2 = 7, // 09F5FDB6
            Unknown8 = 8,    // 09F5FDBC
            Unknown9 = 9,    // 09F5FDC0
            ShapeColor = 10, // 8C798726
            Unknown11 = 11,  // E9D24D51
            Unknown12 = 12,  // 69DE427A
            Unknown13 = 13,  // 29DE3049
            Unknown14 = 14,  // 49DE5331
            Unknown15 = 15,  // 29DE5328
            Unknown16 = 16,  // 4D40C014
            Unknown17 = 17,  // 4D40C00F
            Unknown18 = 18,  // 49E37821
            Unknown19 = 19,  // B73235D7
        }

        /// <summary>
        /// This is where the actual animation keyframes live. Each animation channel can have multiple of these.
        /// </summary>
        public class ChannelComponent
        {
            public DataType Type;
            public CurveType TangentCurveType;
            public int NumKeyFrames;
            public IKeyFrame[] KeyFrames;

            internal void ReadKeyFrames(IoBuffer reader)
            {
                KeyFrames = new IKeyFrame[NumKeyFrames];

                for (var i = 0; i < NumKeyFrames; i++)
                {
                    if (TangentCurveType == CurveType.BakedTangents)
                    {
                        var keyframe = new IKeyFrame.BakedKeyFrame
                        {
                            Data = ReadKeyFrameData(reader, Type)
                        };
                        KeyFrames[i] = keyframe;
                    } else if (TangentCurveType == CurveType.ContinuousTangents)
                    {
                        var time = reader.ReadUInt16();
                        var data = ReadKeyFrameData(reader, ref time, Type);
                        var tangentOut = reader.ReadUInt16();

                        var keyframe = new IKeyFrame.ContinuousKeyFrame
                        {
                            // In and out-tangents are always 5.10 fixed points.
                            Time = time,
                            Data = data,
                            TangentIn = ConvertFixedPointToFloatingPoint(DataType.FixedPoint5_10, tangentOut)
                        };

                        KeyFrames[i] = keyframe;
                    } else if (TangentCurveType == CurveType.DiscontinuousTangents)
                    {
                        var time = reader.ReadUInt16();
                        var data = ReadKeyFrameData(reader, ref time, Type);
                        var tangentIn = reader.ReadUInt16();
                        var tangentOut = reader.ReadUInt16();

                        var keyframe = new IKeyFrame.DiscontinuousKeyFrame
                        {
                            Time = time,
                            Data = data,
                            TangentIn = ConvertFixedPointToFloatingPoint(DataType.FixedPoint5_10, tangentIn),
                            TangentOut = ConvertFixedPointToFloatingPoint(DataType.FixedPoint5_10, tangentOut)
                        };

                        KeyFrames[i] = keyframe;
                    }
                }
            }


            public enum CurveType
            {
                BakedTangents = 0,
                ContinuousTangents = 1,
                DiscontinuousTangents = 2,
            }

            internal static CurveType ChannelCurveTypeFromPackedByte(byte packedByte)
            {
                if (!Enum.IsDefined(typeof(CurveType), packedByte & 0b11))
                {
                    throw new ArgumentException($"Invalid curve type: {packedByte & 0b11}");
                }
                return (CurveType)(packedByte & 0b11);
            }

            public enum DataType
            {
                /// 8.7 fixed point
                FixedPoint8_7,
                /// 9.7 fixed point
                FixedPoint9_7,

                /// 5.10 Fixed point
                FixedPoint5_10,
                /// 5.11 fixed point
                FixedPoint5_11,

                /// 3.12 fixed point
                FixedPoint3_12,
                /// 3.13 fixed point
                FixedPoint3_13,

                /// plain floats.
                FloatingPoint32,
            }

            internal static DataType ChannelDataTypeFromSerializedPackedByte(byte packedByte) {
                // 5th bit set is float.
                if ((packedByte & 0b10000) != 0)
                {
                    return DataType.FloatingPoint32;
                }

                // Next check the top 3 (out of 5) bits and the curve type.
                var topThreeBits = (packedByte >> 2 & 0b111);
                var curveType = ChannelCurveTypeFromPackedByte(packedByte);
                return topThreeBits switch
                {
                    0b000 when curveType == CurveType.BakedTangents => DataType.FixedPoint8_7,
                    0b000 => DataType.FixedPoint9_7,
                    0b001 when curveType == CurveType.BakedTangents => DataType.FixedPoint5_10,
                    0b001 => DataType.FixedPoint5_11,
                    0b011 when curveType == CurveType.BakedTangents => DataType.FixedPoint3_12,
                    0b011 => DataType.FixedPoint3_13,
                    _ => throw new ArgumentException($"Invalid data type: {packedByte}")
                };
            }

            /// <summary>
            /// Reads a data point from the keyframe with the given type, converting fixed point numbers to floats.
            ///
            /// One weird gotcha, some fixed point types "steal" a bit from the short that came before it. Hence, we
            /// need to pass a reference to the short read before the data so it can be masked off.
            /// </summary>
            private static float ReadKeyFrameData(IoBuffer reader, ref ushort precedingShort, DataType type)
            {
                if (type == DataType.FloatingPoint32)
                {
                    return reader.ReadFloat();
                }

                int fixedPoint;
                if (NumBitsForFixedPointDataType(type) == 16)
                {
                    // Need to steal a bit from the preceding short.
                    fixedPoint = reader.ReadInt16();
                    fixedPoint = (fixedPoint << 1) | ((precedingShort >> 15) & 0b1);
                    precedingShort &= 0x7FFF;
                }
                else
                {
                    fixedPoint = reader.ReadUInt16();
                }

                return ConvertFixedPointToFloatingPoint(type, fixedPoint);
            }

            /// <summary>
            /// Reads a data point from the keyframe with the given type, converting fixed point numbers to floats.
            ///
            /// One weird gotcha, some fixed point types "steal" a bit from the short that came before it. Hence, we
            /// need to pass a reference to the short read before the data so it can be masked off.
            /// </summary>
            private static float ReadKeyFrameData(IoBuffer reader, DataType type)
            {
                if (type == DataType.FloatingPoint32)
                {
                    return reader.ReadFloat();
                }
                if (NumBitsForFixedPointDataType(type) == 16)
                {
                    throw new ArgumentException($"{type} needs 16 bits but no preceding short givenn");
                }
                return ConvertFixedPointToFloatingPoint(type, reader.ReadUInt16());
            }

            private static int NumBitsForFixedPointDataType(DataType type)
            {
                return type switch
                {
                    DataType.FixedPoint8_7 => 15,
                    DataType.FixedPoint9_7 => 16,
                    DataType.FixedPoint5_10 => 15,
                    DataType.FixedPoint5_11 => 16,
                    DataType.FixedPoint3_12 => 15,
                    DataType.FixedPoint3_13 => 16,
                    _ => throw new ArgumentOutOfRangeException($"Not a known fixed point type: {type}")
                };
            }

            private static float ConvertFixedPointToFloatingPoint(DataType type, int fixedPoint)
            {
                var numBits = NumBitsForFixedPointDataType(type);
                // Check if the numBits-th bit is set, as that is the sign bit.
                var isNegative = ((1 << numBits) & fixedPoint) != 0;
                // If the number is negative, make the whole fixedPoint int negative, basically performing a sign
                // extension.
                // -1 represented with a 17-bit fixed point will look like 0xFFFF, however fixedPoint will be
                // 0x0000FFFF, we want it to be 0xFFFFFFFF.
                if (isNegative)
                {
                    fixedPoint |= 0x7FFF_0000;
                    // Shift the sign bit up to make the number negative.
                    fixedPoint <<= 1;
                    // Shift everything back, this will be an arithmetic-shift so the sign bit will be preserved.
                    fixedPoint >>= 1;
                }

                var divisor = type switch
                {
                    DataType.FixedPoint8_7 => 128.0, // Divided by 2^7
                    DataType.FixedPoint9_7 => 128.0, // Divided by 2^7
                    DataType.FixedPoint5_10 => 1024.0, // Divided by 2^10
                    DataType.FixedPoint5_11 => 2048.0, // Divided by 2^11
                    DataType.FixedPoint3_12 => 4096.0, // Divided by 2^12
                    DataType.FixedPoint3_13 => 8192.0, // Divided by 2^13
                    _ => throw new ArgumentOutOfRangeException($"Not a known fixed point type: {type}")
                };
                return (float)(fixedPoint / divisor);
            }
        }
    }

    public interface IKeyFrame
    {
        public struct BakedKeyFrame : IKeyFrame
        {
            public float Data;

            public override string ToString()
            {
                return $"BakedKeyFrame (Data={Data})";
            }
        }

        // Has only a tangent out but comes with time tick information.
        public struct ContinuousKeyFrame : IKeyFrame
        {
            public ushort Time;
            public float Data;
            public float TangentIn;

            public override string ToString()
            {
                return $"ContinuousKeyFrame (Data={Data} Time={Time} TangentIn={TangentIn})";
            }
        }

        // Has both tangent in and out information.
        public struct DiscontinuousKeyFrame : IKeyFrame
        {
            public ushort Time;
            public float Data;
            public float TangentIn;
            public float TangentOut;

            public override string ToString()
            {
                return $"DiscontinuousKeyFrame (t={Time} Data={Data} tangentIn={TangentIn} tangentOut={TangentOut})";
            }
        }
    }

    public class AnimResourceConstBlockReader : IScenegraphDataBlockReader<AnimResourceConstBlock>
    {
        public AnimResourceConstBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            Debug.Assert(blockTypeInfo.Version > 5);
            var resource = ScenegraphResource.Deserialize(reader);

            // The game treats the data as a free byte array and basically just overlays some structs on it. This is
            // probably for optimization but we read it normally here so we can ignore this field.
            var serializedSize = reader.ReadUInt32();

            var durationTicks = reader.ReadUInt16();
            var numAnimTargets = reader.ReadUInt16();
            var numEventKeys = reader.ReadUInt16();
            var dataStringLength = reader.ReadByte();
            var version = reader.ReadByte();

            var flags = reader.ReadByte();
            var priority = reader.ReadByte();
            var locomotionType = reader.ReadByte();
            var skeletonTagLength = reader.ReadByte();

            // 4 ignored uint32s.
            reader.ReadBytes(4 * 4);

            var headingOffset = reader.ReadFloat();
            var locomotionDistance = reader.ReadFloat();
            var locomotionCyclePortionCompleted = reader.ReadFloat();
            var locomotionStrideLength = reader.ReadFloat();
            var locomotionStrideVelocityMPT = reader.ReadFloat();
            var velocityMPT = reader.ReadFloat();
            var turnRotation = reader.ReadFloat();
            var preStretchFactor = reader.ReadFloat();
            var rootStretchDisplacement = reader.ReadFloat();

            // We need to note the stream position here because string data is aligned on 4-byte boundaries sometimes.
            var initialStreamPosition = reader.Stream.Position;

            var skeletonTag = reader.ReadNullTerminatedString();
            Debug.Assert(skeletonTag.Length == skeletonTagLength);
            var dataString = reader.ReadNullTerminatedString();
            Debug.Assert(dataString.Length == dataStringLength);

            ReadPadding(reader, reader.Stream.Position - initialStreamPosition);

            var animTargets = new AnimResourceConstBlock.AnimTarget[numAnimTargets];
            for (var i = 0; i < numAnimTargets; i++)
            {
                animTargets[i] = new AnimResourceConstBlock.AnimTarget();

                // 2 ignored uint32s
                reader.ReadBytes(4 * 2);

                var animType = reader.ReadUInt16();
                animTargets[i].NumSharedChannels = reader.ReadUInt16();
                animTargets[i].NumIKChains = reader.ReadByte();
                var channelType = reader.ReadByte();
                var tagLengthMaybe = reader.ReadUInt16();

                // 3 ignored uint32s.
                reader.ReadBytes(4 * 3);

                animTargets[i].Channels = new AnimResourceConstBlock.SharedChannel[animTargets[i].NumSharedChannels];
            }

            // Another aligned section... the names of each animation target's targetted tag.
            initialStreamPosition = reader.Stream.Position;
            for (var i = 0; i < numAnimTargets; i++)
            {
                animTargets[i].TagName = reader.ReadNullTerminatedString();
            }
            ReadPadding(reader, reader.Stream.Position - initialStreamPosition);

            for (var i = 0; i < numAnimTargets; i++)
            {
                var target = animTargets[i];
                for (var j = 0; j < target.NumSharedChannels; j++)
                {
                    var channel = new AnimResourceConstBlock.SharedChannel();
                    target.Channels[j] = channel;

                    // 2 ignored uint32s
                    reader.ReadBytes(4 * 2);
                    channel.BoneHash = reader.ReadUInt32();
                    reader.ReadUInt32(); // 1 ignored uint32.
                    channel.ChannelFlags = reader.ReadUInt32();
                    reader.ReadUInt32(); // 1 ignored uint32.
                }
            }

            // Another aligned section... the names of each of the animation channels.
            initialStreamPosition = reader.Stream.Position;
            for (var i = 0; i < numAnimTargets; i++)
            {
                var target = animTargets[i];
                for (var j = 0; j < target.NumSharedChannels; j++)
                {
                    target.Channels[j].ChannelName = reader.ReadNullTerminatedString();
                }
            }
            ReadPadding(reader, reader.Stream.Position - initialStreamPosition);

            // Next up the number of animation components in each channel and its keyframe types.
            foreach (var target in animTargets)
            {
                foreach (var channel in target.Channels)
                {
                    var numComponents = channel.NumComponents;
                    channel.Components = new AnimResourceConstBlock.ChannelComponent[numComponents];

                    for (var k = 0; k < numComponents; k++)
                    {
                        var component = new AnimResourceConstBlock.ChannelComponent();
                        channel.Components[k] = component;

                        component.NumKeyFrames = reader.ReadUInt16();
                        var dataType = reader.ReadByte();
                        reader.ReadBytes(1 + 4); // 1 ignored byte and 1 ignored uint32.

                        component.Type =
                            AnimResourceConstBlock.ChannelComponent.ChannelDataTypeFromSerializedPackedByte(dataType);
                        component.TangentCurveType =
                            AnimResourceConstBlock.ChannelComponent.ChannelCurveTypeFromPackedByte(dataType);
                    }
                }
            }

            // Now the actual keyframe data.
            foreach (var target in animTargets)
            {
                foreach (var channel in target.Channels)
                {
                    foreach (var component in channel.Components)
                    {
                        component.ReadKeyFrames(reader);
                    }
                }
            }

            // IK chain info for each animation target.
            foreach (var target in animTargets)
            {
                target.IKChains = new AnimResourceConstBlock.IKChain[target.NumIKChains];
                for (var i = 0; i < target.NumIKChains; i++)
                {
                    var ikChain = new AnimResourceConstBlock.IKChain();
                    target.IKChains[i] = ikChain;

                    // 2 ignored uint32s.
                    reader.ReadBytes(2 * 4);

                    var animTargetIdx = reader.ReadUInt16();
                    ikChain.NumIkTargets = reader.ReadUInt16();

                    ikChain.NameCrc = reader.ReadUInt32();
                    reader.ReadBytes(1); // ignored uint8
                    // (these are technically null terminators so the crc32 can be treated as a string in game.)
                    ikChain.NameMirrorCrc = reader.ReadUInt32();
                    reader.ReadBytes(1); // ignored uint8

                    ikChain.IkStrategy = (AnimResourceConstBlock.IKStrategy) reader.ReadUInt32();

                    ikChain.BeginBoneCrc = reader.ReadUInt32();
                    reader.ReadBytes(1); // ignored uint8
                    ikChain.BeginBoneMirrorCrc = reader.ReadUInt32();
                    reader.ReadBytes(1); // ignored uint8
                    ikChain.EndBoneCrc = reader.ReadUInt32();
                    reader.ReadBytes(1); // ignored uint8
                    ikChain.EndBoneMirrorCrc = reader.ReadUInt32();
                    reader.ReadBytes(1); // ignored uint8
                    ikChain.TwistVectorCrc = reader.ReadUInt32();
                    reader.ReadBytes(1); // ignored uint8
                    ikChain.TwistVectorBoneCrc = reader.ReadUInt32();
                    reader.ReadBytes(1); // ignored uint8
                    ikChain.TwistVectorMirrorBoneCRC = reader.ReadUInt32();
                    reader.ReadBytes(1); // ignored uint8
                    ikChain.IkWeightCRC = reader.ReadUInt32();
                    reader.ReadBytes(1); // ignored uint8
                    reader.ReadUInt32(); // 1 ignored uint32
                }
            }

            // Details of each ik chain target.
            foreach (var animTarget in animTargets)
            {
                foreach (var ikChain in animTarget.IKChains)
                {
                    ikChain.IkTargets = new AnimResourceConstBlock.IKChain.Target[ikChain.NumIkTargets];
                    for (var i = 0; i < ikChain.NumIkTargets; i++)
                    {
                        var target = new AnimResourceConstBlock.IKChain.Target();
                        ikChain.IkTargets[i] = target;

                        // 2 ignored uint32s.
                        reader.ReadBytes(2 * 4);

                        var animTargetIdx = reader.ReadUInt16();
                        var ikChainIdx = reader.ReadUInt16();
                        var targetType = reader.ReadByte();

                        target.BoneCrc = reader.ReadUInt32();
                        reader.ReadBytes(1); // ignored uint8
                        target.BoneMirrorCrc = reader.ReadUInt32();
                        reader.ReadBytes(1); // ignored uint8
                        target.RotationCrc = reader.ReadUInt32();
                        reader.ReadBytes(1); // ignored uint8
                        target.Rotation2Crc = reader.ReadUInt32();
                        reader.ReadBytes(1); // ignored uint8
                        target.TranslationCrc = reader.ReadUInt32();
                        reader.ReadBytes(1); // ignored uint8
                        target.ContactCrc = reader.ReadUInt32();
                        reader.ReadBytes(1); // ignored uint8
                    }
                }
            }

            for (var i = 0; i < numEventKeys; i++)
            {
                reader.ReadUInt32(); // ignored uint32, points to the vtable internally.

                var ticks = reader.ReadUInt16();
                var type = reader.ReadInt16();
                var eventDataStringLength = reader.ReadUInt16();
                
                reader.ReadUInt32(); // ignored uint32, points to the the data string internally.
            }

            for (var i = 0; i < numEventKeys; i++)
            {
                var eventDataString = reader.ReadNullTerminatedString();
            }

            return new AnimResourceConstBlock(blockTypeInfo, resource, locomotionType, headingOffset, locomotionDistance,
                velocityMPT, turnRotation, animTargets);
        }

        // For some reason this format pads to 4-byte boundaries sometimes...we deal with that here.
        private static void ReadPadding(IoBuffer reader, long length)
        {
            for (var i = 0; i < (length % 4); i++)
            {
                var paddingByte = reader.ReadByte();
                Debug.Assert(i == paddingByte);
            }
        }
    }
}