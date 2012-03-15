//  leveldb-sharp
// 
//  Copyright (c) 2012, Mirco Bauer <meebey@meebey.net>
//  All rights reserved.
// 
//  Redistribution and use in source and binary forms, with or without
//  modification, are permitted provided that the following conditions are
//  met:
// 
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
//       copyright notice, this list of conditions and the following disclaimer
//       in the documentation and/or other materials provided with the
//       distribution.
//     * Neither the name of Google Inc. nor the names of its
//       contributors may be used to endorse or promote products derived from
//       this software without specific prior written permission.
// 
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
//  OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
//  LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
//  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
//  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;

namespace LevelDB
{
    public class Options
    {
        Cache f_BlockCache;

        public IntPtr Handle { get; private set; }

        // const Comparator* comparator;
        // bool create_if_missing;
        public bool CreateIfMissing {
            set {
                Native.leveldb_options_set_create_if_missing(Handle, value);
            }
        }

        // bool error_if_exists;
        public bool ErrorIfExists {
            set {
                Native.leveldb_options_set_error_if_exists(Handle, value);
            }
        }

        // bool paranoid_checks;
        public bool ParanoidChecks {
            set {
                Native.leveldb_options_set_paranoid_checks(Handle, value);
            }
        }

        // Env* env;
        // Logger* info_log;
        // size_t write_buffer_size;

        // int max_open_files;
        public int MaxOpenFiles {
            set {
                Native.leveldb_options_set_max_open_files(Handle, value);
            }
        }

        // Cache* block_cache;
        public Cache BlockCache {
            set {
                // keep a reference to Cache so it doesn't get GCed
                f_BlockCache = value;
                if (value == null) {
                    Native.leveldb_options_set_cache(Handle, IntPtr.Zero);
                } else {
                    Native.leveldb_options_set_cache(Handle, value.Handle);
                }
            }
        }

        // size_t block_size;
        // int block_restart_interval;

        // CompressionType compression;
        public CompressionType Compression {
            set {
                Native.leveldb_options_set_compression(Handle, (int) value);
            }
        }

        public Options()
        {
            Handle = Native.leveldb_options_create();
        }

        ~Options()
        {
            Native.leveldb_options_destroy(Handle);
        }
    }
}
