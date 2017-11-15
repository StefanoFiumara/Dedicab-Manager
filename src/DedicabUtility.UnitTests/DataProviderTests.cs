using System;
using System.Collections;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using DedicabUtility.Client.Models;
using DedicabUtility.Client.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using DedicabUtility.Client.Exceptions;

namespace DedicabUtility.UnitTests
{
    [TestClass]
    public class DataProviderTests
    {
        private DedicabDataService DataService { get; set; }

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

        [TestMethod]
        [DeploymentItem(@"TestData\ITG", @"VerifySongDataIsLoaded\Songs\ITG")]
        public void VerifySongDataIsLoaded()
        {
            var stepmaniaRoot = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "VerifySongDataIsLoaded"));
            var groups = this.DataService.GetUpdatedSongData(stepmaniaRoot, new Progress<string>(s => { }));

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

            var newSongGroup = this.DataService.AddNewSongs(stepmaniaRoot, newSongs, @"NewPack", new Progress<string>(s => { }));
            Assert.AreEqual(4, newSongGroup.Songs.Count());
            Assert.AreEqual("NewPack", newSongGroup.Name);

            //TODO: This test cannot verify that all the related sm file content (mp3/ogg, banners) is copied alongside the .sm to the proper directory.
            //TODO: We may need some dummy files to ensure this requirement is covered.
            var allGroups = this.DataService.GetUpdatedSongData(stepmaniaRoot, new Progress<string>(s => { }));
            Assert.AreEqual(2, allGroups.Count);
            Assert.AreEqual(72, allGroups.SelectMany(g => g.Songs).Count());

        }
        
        [TestMethod]
        [DeploymentItem(@"TestData\ITG",      @"VerifyAddingSongDataWithExistingPackNameThrowsException\Songs\ITG")]
        [DeploymentItem(@"TestData\NewSongs", @"VerifyAddingSongDataWithExistingPackNameThrowsException\NewSongs\NewPack")]
        [ExpectedException(typeof(DuplicateSongPackException))]
        public void VerifyAddingSongDataWithExistingPackNameThrowsException()
        {
            var stepmaniaRoot = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "VerifyAddingSongDataWithExistingPackNameThrowsException"));
            var newSongs = Directory.EnumerateFiles(Path.Combine(stepmaniaRoot.FullName, "NewSongs", "NewPack"), "*.sm", SearchOption.AllDirectories).Select(s => new FileInfo(s));

            this.DataService.AddNewSongs(stepmaniaRoot, newSongs, @"ITG", new Progress<string>(s => { }));
        }

        [TestMethod]
        [DeploymentItem(@"TestData\ITG", @"VerifyAddSongDataDoesNotCopySscFile\Songs\ITG")]
        [DeploymentItem(@"TestData\SSC", @"VerifyAddSongDataDoesNotCopySscFile\NewSongs\NewPack")]
        public void VerifyAddSongDataDoesNotCopySscFile()
        {
            var stepmaniaRoot = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "VerifyAddSongDataDoesNotCopySscFile"));

            var newSongs = Directory.EnumerateFiles(Path.Combine(stepmaniaRoot.FullName, "NewSongs", "NewPack"), "*.sm", SearchOption.AllDirectories)
                                    .Select(s => new FileInfo(s));
            

            this.DataService.AddNewSongs(stepmaniaRoot, newSongs, @"SSC", new Progress<string>(s => { }));

            var sscFiles = Directory.EnumerateFiles(Path.Combine(stepmaniaRoot.FullName, @"Songs", @"SSC"));

            Assert.IsFalse(sscFiles.Any());
        }
    }
}
