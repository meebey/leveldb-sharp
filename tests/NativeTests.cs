// leveldb-sharp
//
// Copyright (c) 2012-2013, Mirco Bauer <meebey@meebey.net>
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//    * Redistributions of source code must retain the above copyright
//      notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above
//      copyright notice, this list of conditions and the following disclaimer
//      in the documentation and/or other materials provided with the
//      distribution.
//    * Neither the name of Google Inc. nor the names of its
//      contributors may be used to endorse or promote products derived from
//      this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;

namespace LevelDB
{
    [TestFixture]
    public class NativeTests
    {
        IntPtr Database { get; set; }
        string DatabasePath { get; set; }

        [SetUp]
        public void SetUp()
        {
            var tempPath = Path.GetTempPath();
            var randName = Path.GetRandomFileName();
            DatabasePath = Path.Combine(tempPath, randName);
            var options = Native.leveldb_options_create();
            Native.leveldb_options_set_create_if_missing(options, true);
            Database = Native.leveldb_open(options, DatabasePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Database != IntPtr.Zero) {
                Native.leveldb_close(Database);
                Database = IntPtr.Zero;
            }
            if (Directory.Exists(DatabasePath)) {
                Directory.Delete(DatabasePath, true);
            }
        }

        [Test]
        public void Open()
        {
            // NOOP, SetUp calls open for us
        }

        [Test]
        public void Reopen()
        {
            Native.leveldb_close(Database);
            Database = IntPtr.Zero;

            var options = Native.leveldb_options_create();
            Database = Native.leveldb_open(options, DatabasePath);
            var readOptions = Native.leveldb_readoptions_create();
            Native.leveldb_get(Database, readOptions, "key1");
            Native.leveldb_readoptions_destroy(readOptions);
        }

        [Test]
        public void Put()
        {
            var options = Native.leveldb_writeoptions_create();
            Native.leveldb_put(Database, options, "key1", "value1");
            Native.leveldb_put(Database, options, "key2", "value2");
            Native.leveldb_put(Database, options, "key3", "value3");

            // sync
            Native.leveldb_writeoptions_set_sync(options, true);
            Native.leveldb_put(Database, options, "key4", "value4");
        }

        [Test]
        public void Get()
        {
            var options = Native.leveldb_readoptions_create();

            Native.leveldb_put(Database, options, "key1", "value1");
            var value1 = Native.leveldb_get(Database, options, "key1");
            Assert.AreEqual("value1", value1);

            Native.leveldb_put(Database, options, "key2", "value2");
            var value2 = Native.leveldb_get(Database, options, "key2");
            Assert.AreEqual("value2", value2);

            Native.leveldb_put(Database, options, "key3", "value3");
            var value3 = Native.leveldb_get(Database, options, "key3");
            Assert.AreEqual("value3", value3);

            // verify checksums
            Native.leveldb_readoptions_set_verify_checksums(options, true);
            value1 = Native.leveldb_get(Database, options, "key1");
            Assert.AreEqual("value1", value1);

            // no fill cache
            Native.leveldb_readoptions_set_fill_cache(options, false);
            value2 = Native.leveldb_get(Database, options, "key2");
            Assert.AreEqual("value2", value2);

            Native.leveldb_readoptions_destroy(options);
        }

        [Test]
        public void Delete()
        {
            var writeOptions = Native.leveldb_writeoptions_create();
            Native.leveldb_put(Database, writeOptions, "key1", "value1");

            var readOptions = Native.leveldb_readoptions_create();
            var value1 = Native.leveldb_get(Database, readOptions, "key1");
            Assert.AreEqual("value1", value1);
            Native.leveldb_delete(Database, writeOptions, "key1");
            value1 = Native.leveldb_get(Database, readOptions, "key1");
            Assert.IsNull(value1);

            Native.leveldb_writeoptions_destroy(writeOptions);
            Native.leveldb_readoptions_destroy(readOptions);
        }

        [Test]
        public void WriteBatch()
        {
            var writeOptions = Native.leveldb_writeoptions_create();
            Native.leveldb_put(Database, writeOptions, "key1", "value1");

            var writeBatch = Native.leveldb_writebatch_create();
            Native.leveldb_writebatch_delete(writeBatch, "key1");
            Native.leveldb_writebatch_put(writeBatch, "key2", "value2");
            Native.leveldb_write(Database, writeOptions, writeBatch);

            var readOptions = Native.leveldb_readoptions_create();
            var value1 = Native.leveldb_get(Database, readOptions, "key1");
            Assert.IsNull(value1);
            var value2 = Native.leveldb_get(Database, readOptions, "key2");
            Assert.AreEqual("value2", value2);

            Native.leveldb_writebatch_delete(writeBatch, "key2");
            Native.leveldb_writebatch_clear(writeBatch);
            Native.leveldb_write(Database, writeOptions, writeBatch);
            value2 = Native.leveldb_get(Database, readOptions, "key2");
            Assert.AreEqual("value2", value2);

            Native.leveldb_writebatch_destroy(writeBatch);
            Native.leveldb_writeoptions_destroy(writeOptions);
            Native.leveldb_writeoptions_destroy(readOptions);
        }

        [Test]
        public void Enumerator()
        {
            var writeOptions = Native.leveldb_writeoptions_create();
            Native.leveldb_put(Database, writeOptions, "key1", "value1");
            Native.leveldb_put(Database, writeOptions, "key2", "value2");
            Native.leveldb_put(Database, writeOptions, "key3", "value3");

            var entries = new List<KeyValuePair<string, string>>();
            var readOptions = Native.leveldb_readoptions_create();
            IntPtr iter = Native.leveldb_create_iterator(Database, readOptions);
            for (Native.leveldb_iter_seek_to_first(iter);
                 Native.leveldb_iter_valid(iter);
                 Native.leveldb_iter_next(iter)) {
                string key = Native.leveldb_iter_key(iter);
                string value = Native.leveldb_iter_value(iter);
                var entry = new KeyValuePair<string, string>(key, value);
                entries.Add(entry);
            }
            Native.leveldb_iter_destroy(iter);
            Native.leveldb_readoptions_destroy(readOptions);

            Assert.AreEqual(3, entries.Count);
            Assert.AreEqual("key1",   entries[0].Key);
            Assert.AreEqual("value1", entries[0].Value);
            Assert.AreEqual("key2",   entries[1].Key);
            Assert.AreEqual("value2", entries[1].Value);
            Assert.AreEqual("key3",   entries[2].Key);
            Assert.AreEqual("value3", entries[2].Value);

            Native.leveldb_writeoptions_destroy(writeOptions);
        }

        [Test]
        public void Cache()
        {
            Native.leveldb_close(Database);
            Database = IntPtr.Zero;

            // open the DB with a cache that is not owned by LevelDB, then
            // close DB and then free the cache
            var options = Native.leveldb_options_create();
            var cache = Native.leveldb_cache_create_lru((UIntPtr) 64);
            Native.leveldb_options_set_cache(options, cache);
            Database = Native.leveldb_open(options, DatabasePath);
            Native.leveldb_close(Database);
            Database = IntPtr.Zero;

            Native.leveldb_cache_destroy(cache);
            Native.leveldb_options_destroy(options);
        }

        [Test]
        public void Snapshot()
        {
            // modify db
            var writeOptions = Native.leveldb_writeoptions_create();
            Native.leveldb_put(Database, writeOptions, "key1", "value1");
            Native.leveldb_writeoptions_destroy(writeOptions);

            // create snapshot
            var snapshot = Native.leveldb_create_snapshot(Database);

            // modify db again
            writeOptions = Native.leveldb_writeoptions_create();
            Native.leveldb_put(Database, writeOptions, "key2", "value2");
            Native.leveldb_writeoptions_destroy(writeOptions);

            // read from snapshot
            var readOptions = Native.leveldb_readoptions_create();
            Native.leveldb_readoptions_set_snapshot(readOptions, snapshot);
            var val1 = Native.leveldb_get(Database, readOptions, "key1");
            Assert.AreEqual("value1", val1);
            var val2 = Native.leveldb_get(Database, readOptions, "key2");
            Assert.IsNull(val2);
            Native.leveldb_readoptions_destroy(readOptions);

            // release snapshot
            Native.leveldb_release_snapshot(Database, snapshot);
            snapshot = IntPtr.Zero;
        }

        [Test]
        public void Version()
        {
            var major = Native.leveldb_major_version();
            Assert.Greater(major, 0);
            var minor = Native.leveldb_minor_version();
            Assert.Greater(minor, 0);
            Console.WriteLine("LevelDB version: {0}.{1}", major, minor);
        }

        [Test]
        public void CompactRange()
        {
            Native.leveldb_compact_range(Database, null, null);
        }

        [Test]
        public void Destroy()
        {
            Native.leveldb_close(Database);
            Database = IntPtr.Zero;

            var options = Native.leveldb_options_create();
            Native.leveldb_destroy_db(options, DatabasePath);
            Native.leveldb_options_destroy(options);
        }

        [Test]
        public void Repair()
        {
            Native.leveldb_close(Database);
            Database = IntPtr.Zero;

            var options = Native.leveldb_options_create();
            Native.leveldb_repair_db(options, DatabasePath);
            Native.leveldb_options_destroy(options);
        }

        [Test]
        public void Property()
        {
            var property = Native.leveldb_property_value(Database, "leveldb.stats");
            Assert.IsNotNull(property);
            Console.WriteLine("LevelDB stats: {0}", property);
        }
    }
}
