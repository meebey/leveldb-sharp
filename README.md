leveldb-sharp is a portable C# binding for the C API of the [LevelDB library].

To support the most reach leveldb-sharp is available under the 3-clause BSD
license, which is the same license LevelDB uses.

leveldb-sharp uses the C API instead of the C++ API for portability. Using
C++/CLI would have been the direct route but it is currently not portable
outside of Windows. As I use the binding for [Smuxi] which targets Linux,
Windows and OS X, the C API was the better pick.

## Features ##

leveldb-sharp offers:

 * [low-level function calls] to LevelDB
 * [high-level object oriented API] with .NET enrichments (IEnumerable, IDisposable)
   * DB API
   * Options API
   * Cache API
 * Allows combined use of low-level and high-level APIs
 * NUnit test-case coverage

## Limitations ##

Currently leveldb-sharp lacks:

 * high-level iterator API
 * writebatch API
 * comparator API
 * compact range API

## Download ##

 * Source tarball: [leveldb-sharp-1.2.0.tar.gz]
 * Git repository on github: [meebey/leveldb-sharp][github]

## Projects using leveldb-sharp ##

 * [Smuxi IRC Client][Smuxi]

 [LevelDB library]: http://code.google.com/p/leveldb/
 [low-level function calls]: https://github.com/meebey/leveldb-sharp/blob/master/Native.cs
 [high-level object oriented API]: https://github.com/meebey/leveldb-sharp/blob/master/DB.cs
 [leveldb-sharp-1.2.0.tar.gz]: http://www.meebey.net/projects/leveldb-sharp/downloads/leveldb-sharp-1.2.0.tar.gz
 [github]: https://github.com/meebey/leveldb-sharp
 [Smuxi]: http://www.smuxi.org/ "Smuxi IRC Client"

