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
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;

namespace LevelDB
{
    [TestFixture]
    public class DBTests
    {
        DB Database { get; set; }
        string DatabasePath { get; set; }

        [SetUp]
        public void SetUp()
        {
            var tempPath = Path.GetTempPath();
            var randName = Path.GetRandomFileName();
            DatabasePath = Path.Combine(tempPath, randName);
            var options = new Options() {
                CreateIfMissing = true
            };
            Database = new DB(options, DatabasePath);
        }

        [TearDown]
        public void TearDown()
        {
            Database.Dispose();
            if (Directory.Exists(DatabasePath)) {
                Directory.Delete(DatabasePath, true);
            }
        }

        [Test]
        public void Constructor()
        {
            // NOOP, SetUp calls ctor for us
        }

        [Test]
        public void Close()
        {
            // test double close
            Database.Dispose();
        }

        [Test]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void DisposeChecks()
        {
            Database.Dispose();
            Database.Get("key1");
        }

        [Test]
        public void Put()
        {
            Database.Put(null, "key1", "value1");
            Database.Put(null, "key2", "value2");
            Database.Put(null, "key3", "value3");
        }

        [Test]
        public void Get()
        {
            Database.Put(null, "key1", "value1");
            var value1 = Database.Get(null, "key1");
            Assert.AreEqual("value1", value1);

            Database.Put(null, "key2", "value2");
            var value2 = Database.Get(null, "key2");
            Assert.AreEqual("value2", value2);

            Database.Put(null, "key3", "value3");
            var value3 = Database.Get(null, "key3");
            Assert.AreEqual("value3", value3);
        }

        [Test]
        public void Delete()
        {
            Database.Put("key1", "value1");
            var value1 = Database.Get(null, "key1");
            Assert.AreEqual("value1", value1);
            Database.Delete(null, "key1");
            value1 = Database.Get(null, "key1");
            Assert.IsNull(value1);
        }

        [Test]
        public void Enumerator()
        {
            Database.Put(null, "key1", "value1");
            Database.Put(null, "key2", "value2");
            Database.Put(null, "key3", "value3");

            var entries = new List<KeyValuePair<string, string>>();
            foreach (var entry in Database) {
                entries.Add(entry);
            }

            Assert.AreEqual(3, entries.Count);
            Assert.AreEqual("key1", entries[0].Key);
            Assert.AreEqual("value1", entries[0].Value);
            Assert.AreEqual("key2", entries[1].Key);
            Assert.AreEqual("value2", entries[1].Value);
            Assert.AreEqual("key3", entries[2].Key);
            Assert.AreEqual("value3", entries[2].Value);
        }

        [Test]
        public void Cache()
        {
            Database.Dispose();

            // open the DB with a cache that is not owned by LevelDB, then
            // close DB and then free the cache
            var options = new Options() {
                BlockCache = new Cache(64)
            };
            Database = new DB(options, DatabasePath);
            options = null;
            GC.Collect();
            Database.Put("key1", "value1");
            Database.Dispose();
            GC.Collect();
        }
    }
}
