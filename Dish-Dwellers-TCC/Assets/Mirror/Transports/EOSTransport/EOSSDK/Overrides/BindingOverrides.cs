// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

#if DEBUG
	#define EOS_DEBUG
#endif

#if UNITY_EDITOR
	#define EOS_EDITOR
#endif

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_PS4 || UNITY_XBOXONE || UNITY_SWITCH || UNITY_IOS || UNITY_ANDROID
	#define EOS_UNITY
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || PLATFORM_64BITS || PLATFORM_32BITS
	#if UNITY_EDITOR_WIN || UNITY_64 || PLATFORM_64BITS
		#define EOS_PLATFORM_WINDOWS_64
	#else
		#define EOS_PLATFORM_WINDOWS_32
	#endif

#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	#define EOS_PLATFORM_OSX

#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
	#define EOS_PLATFORM_LINUX

#elif UNITY_PS4
	#define EOS_PLATFORM_PS4

#elif UNITY_XBOXONE
	#define EOS_PLATFORM_XBOXONE

#elif UNITY_SWITCH
	#define EOS_PLATFORM_SWITCH

#elif UNITY_IOS || __IOS__
	#define EOS_PLATFORM_IOS

#elif UNITY_ANDROID || __ANDROID__
	#define EOS_PLATFORM_ANDROID

#endif

#if EOS_EDITOR
	#define EOS_DYNAMIC_BINDINGS
#endif

#if EOS_DYNAMIC_BINDINGS
	#if EOS_PLATFORM_WINDOWS_32
		#define EOS_DYNAMIC_BINDINGS_NAME_TYPE3
	#elif EOS_PLATFORM_OSX
		//#define EOS_DYNAMIC_BINDINGS_NAME_TYPE2
		#define EOS_DYNAMIC_BINDINGS_NAME_TYPE1
	#else
		#define EOS_DYNAMIC_BINDINGS_NAME_TYPE1
	#endif
#endif


using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices
{
	public static partial class Bindings
	{

#if EOS_DYNAMIC_BINDINGS
		[UnmanagedFunctionPointer(Config.LibraryCallingConvention)]
		internal delegate Result EOS_P2P_ReceivePacketDelegate(System.IntPtr handle, ref P2P.ReceivePacketOptionsInternal options, ref System.IntPtr outPeerId, System.IntPtr outSocketId, ref byte outChannel, System.IntPtr outData, ref uint outBytesWritten);
		internal static EOS_P2P_ReceivePacketDelegate EOS_P2P_ReceivePacket;
#endif

#if !EOS_DYNAMIC_BINDINGS
		[DllImport(Config.LibraryName)]
		internal static extern Result EOS_P2P_ReceivePacket(System.IntPtr handle, ref P2P.ReceivePacketOptionsInternal options, ref System.IntPtr outPeerId, System.IntPtr outSocketId, ref byte outChannel, System.IntPtr outData, ref uint outBytesWritten);
#endif
	}
}