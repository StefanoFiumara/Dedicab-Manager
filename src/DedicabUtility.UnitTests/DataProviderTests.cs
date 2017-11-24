using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using DedicabUtility.Client.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DedicabUtility.Client.Exceptions;

namespace DedicabUtility.UnitTests
{
    [TestClass]
    public class DataProviderTests
    {
        private DedicabDataService DataService { get; set; }
        private Progress<string> EmptyProgressNotifier { get; } = new Progress<string>(s => { });

        [AssemblyInitialize]
        public static void InitPackUriHelper(TestContext context)
        {
            PackUriHelper.Create(new Uri("reliable://0"));
        }
        
        [TestInitialize]
        public void TestInit()
        {    
            this.DataService = new DedicabDataService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            string removedSongsCachePath = Path.Combine(Directory.GetCurrentDirectory(), "RemovedSongsCache");
            if (Directory.Exists(removedSongsCachePath))
            {
                Directory.Delete(removedSongsCachePath, recursive: true);
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestData\ITG", @"VerifySongDataIsLoaded\Songs\ITG")]
        public void VerifySongDataIsLoaded()
        {
            var stepmaniaRoot = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "VerifySongDataIsLoaded"));
            var groups = this.DataService.GetUpdatedSongData(stepmaniaRoot, this.EmptyProgressNotifier);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(68, groups.Single().Songs.Count());
        }

        [TestMethod]
        [DeploymentItem(@"TestData\ITG",      @"VerifyNewSongDataIsAdded\Songs\ITG")]
        [DeploymentItem(@"TestData\NewSongs", @"VerifyNewSongDataIsAdded\NewSongs\NewPack")]
        public void VerifyNewSongDataIsAdded()
        {
            var stepmaniaRoot = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "VerifyNewSongDataIsAdded"));
            var newSongs = Directory.EnumerateFiles(Path.Combine(stepmaniaRoot.FullName, "NewSongs","NewPack"), "*.sm", SearchOption.AllDirectories)
                                    .Select(s => new FileInfo(s));

            var newSongGroup = this.DataService.AddNewSongs(stepmaniaRoot, newSongs, @"NewPack", this.EmptyProgressNotifier);
            Assert.AreEqual(4, newSongGroup.Songs.Count());
            Assert.AreEqual("NewPack", newSongGroup.Name);
            
            var allGroups = this.DataService.GetUpdatedSongData(stepmaniaRoot, this.EmptyProgressNotifier);
            Assert.AreEqual(2, allGroups.Count);
            Assert.AreEqual(72, allGroups.SelectMany(g => g.Songs).Count());
        }

        [TestMethod]
        [DeploymentItem(@"TestData\ITG", @"VerifyNewSongDataCopiesAdditionalFiles\Songs\ITG")]
        [DeploymentItem(@"TestData\NewSongs", @"VerifyNewSongDataCopiesAdditionalFiles\NewSongs\NewPack")]
        public void VerifyNewSongDataCopiesAdditionalFiles()
        {
            var stepmaniaRoot = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "VerifyNewSongDataCopiesAdditionalFiles"));
            var newSongs = Directory.EnumerateFiles(Path.Combine(stepmaniaRoot.FullName, "NewSongs", "NewPack"), "*.sm", SearchOption.AllDirectories)
                .Select(s => new FileInfo(s));

            var newSongGroup = this.DataService.AddNewSongs(stepmaniaRoot, newSongs, @"NewPack", this.EmptyProgressNotifier);
            Assert.AreEqual(4, newSongGroup.Songs.Count());
            Assert.AreEqual("NewPack", newSongGroup.Name);

            var newPackDirectory = new DirectoryInfo(Path.Combine(stepmaniaRoot.FullName, "Songs", "NewPack"));
            foreach (var newSongDirectory in newPackDirectory.EnumerateDirectories())
            {
                //Expect 5 files in each directory for the .sm backup created by the lights builder.
                Assert.AreEqual(5, newSongDirectory.EnumerateFiles().Count());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestData\ITG",      @"VerifyAddingSongDataWithExistingPackNameThrowsException\Songs\ITG")]
        [DeploymentItem(@"TestData\NewSongs", @"VerifyAddingSongDataWithExistingPackNameThrowsException\NewSongs\NewPack")]
        [ExpectedException(typeof(DuplicateSongPackException))]
        public void VerifyAddingSongDataWithExistingPackNameThrowsException()
        {
            var stepmaniaRoot = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "VerifyAddingSongDataWithExistingPackNameThrowsException"));
            var newSongs = Directory.EnumerateFiles(Path.Combine(stepmaniaRoot.FullName, "NewSongs", "NewPack"), "*.sm", SearchOption.AllDirectories).Select(s => new FileInfo(s));

            this.DataService.AddNewSongs(stepmaniaRoot, newSongs, @"ITG", this.EmptyProgressNotifier);
        }

        [TestMethod]
        [DeploymentItem(@"TestData\ITG", @"VerifyAddSongDataDoesNotCopySscFile\Songs\ITG")]
        [DeploymentItem(@"TestData\SSC", @"VerifyAddSongDataDoesNotCopySscFile\NewSongs\NewPack")]
        public void VerifyAddSongDataDoesNotCopySscFile()
        {
            var stepmaniaRoot = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "VerifyAddSongDataDoesNotCopySscFile"));

            var newSongs = Directory.EnumerateFiles(Path.Combine(stepmaniaRoot.FullName, "NewSongs", "NewPack"), "*.sm", SearchOption.AllDirectories)
                                    .Select(s => new FileInfo(s));
            

            this.DataService.AddNewSongs(stepmaniaRoot, newSongs, @"SSC", this.EmptyProgressNotifier);

            var sscFiles = Directory.EnumerateFiles(Path.Combine(stepmaniaRoot.FullName, @"Songs", @"SSC"), "*.ssc", SearchOption.AllDirectories);

            Assert.IsFalse(sscFiles.Any());
        }

        [TestMethod]
        [DeploymentItem(@"TestData\ITG", @"VerifySongsAreRemoved\Stepmania\Songs\ITG")]
        public void VerifySongsAreRemoved()
        {
            var stepmaniaRoot = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "VerifySongsAreRemoved", "Stepmania"));

            this.DataService.RemoveSongPack(stepmaniaRoot, "ITG", this.EmptyProgressNotifier);

            var songs = this.DataService.GetUpdatedSongData(stepmaniaRoot, this.EmptyProgressNotifier);

            Assert.AreEqual(0, songs.Count);
        }

        [TestMethod]    
        [DeploymentItem(@"TestData\ITG", @"VerifyRemovedSongsAreCached\Stepmania\Songs\ITG")]
        public void VerifyRemovedSongsAreCached()
        {
            var programRoot = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "VerifyRemovedSongsAreCached"));
            var stepmaniaRoot = new DirectoryInfo(Path.Combine(programRoot.FullName, "Stepmania"));

            this.DataService.RemoveSongPack(stepmaniaRoot, "ITG", this.EmptyProgressNotifier);
            
            var removedSongsCachePath = new DirectoryInfo(Path.Combine(programRoot.Parent.FullName, "RemovedSongsCache"));
            var removedSongs = Directory.EnumerateFiles(removedSongsCachePath.FullName, "*.*", SearchOption.AllDirectories);

            Assert.IsTrue(removedSongsCachePath.Exists);
            Assert.IsTrue(removedSongs.Any());
        }

        [TestMethod]
        [DeploymentItem(@"TestData\ITG", @"VerifySongsAreRemoved\Stepmania\Songs\ITG")]
        [ExpectedException(typeof(SongPackNotFoundException))]
        public void VerifyRemovingUnknownSongPackThrowsException()
        {
            var stepmaniaRoot = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "VerifySongsAreRemoved", "Stepmania"));

            this.DataService.RemoveSongPack(stepmaniaRoot, "ITG2", this.EmptyProgressNotifier);
        }

    }
}