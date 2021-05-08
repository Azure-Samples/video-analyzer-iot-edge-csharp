// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Buffers;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace GrpcExtension.Core
{
    /// <summary>
    /// Allows for random access reads and writes on a memory mapped file.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    public sealed class MemoryMappedFileMemoryManager<T> : MemoryManager<T>
        where T : unmanaged
    {
        private const string LinuxSharedMemoryDirectory = "/dev/shm";

        private readonly int _length;
        private readonly MemoryMappedFile _file;
        private readonly MemoryMappedViewAccessor _accessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryMappedFileMemoryManager"/> class.
        /// </summary>
        /// <param name="mapName">Memory map name of path.</param>
        /// <param name="length">Length of the map determined in number of items.</param>
        /// <param name="createIfNew">If true forces a new file to be created if one doesn't exist.</param>
        /// <param name="desiredAccess">File mode access.</param>
        public unsafe MemoryMappedFileMemoryManager(
            string mapName,
            int length,
            bool createIfNew = false,
            MemoryMappedFileAccess desiredAccess = MemoryMappedFileAccess.Read)
        {
            if (string.IsNullOrWhiteSpace(mapName))
            {
                throw new ArgumentNullException(nameof(mapName));
            }

            _length = length;

            var size = _length * sizeof(T);

            // Create the memory-mapped file.
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                mapName = Path.Combine(LinuxSharedMemoryDirectory, mapName);
                _file = MemoryMappedFile.CreateFromFile(mapName, createIfNew ? FileMode.OpenOrCreate : FileMode.Open, null, size);
            }
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                _file = createIfNew
                    ? MemoryMappedFile.CreateOrOpen(mapName, size, desiredAccess)
                    : MemoryMappedFile.OpenExisting(mapName, desiredAccess.ToMemoryMappedFileRights());
            } else
            {
                throw new Exception($"The platform {Environment.OSVersion.Platform} is not supported.");
            }

            _accessor = _file.CreateViewAccessor(0, size, desiredAccess);
        }

        /// Free resources
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _accessor.Dispose();
                _file.Dispose();
            }
        }

        /// <summary>
        /// Returns a memory span that wraps the underlying memory buffer.
        /// </summary>
        /// <returns></returns>
        public unsafe override Span<T> GetSpan()
        {
            var viewHandle = _accessor.SafeMemoryMappedViewHandle;
            var viewPointer = viewHandle.DangerousGetHandle();

            return new Span<T>((void*)viewPointer, _length);
        }

        /// <summary>
        /// Returns a handle to the memory that has been pinned and whose address can be taken.
        /// </summary>
        /// <param name="elementIndex">The offset to the element in the memory buffer at which the returned MemoryHandle points.</param>
        /// <returns></returns>
        public unsafe override MemoryHandle Pin(int elementIndex = 0)
        {
            var viewHandle = _accessor.SafeMemoryMappedViewHandle;
            var viewPointer = viewHandle.DangerousGetHandle() + elementIndex * sizeof(T);

            return new MemoryHandle((void*)viewPointer);
        }

        /// <summary>
        /// No-op
        /// </summary>
        public override void Unpin()
        {
        }
    }
}
