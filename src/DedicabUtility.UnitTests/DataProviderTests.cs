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
        private DedicabDataProvider DataProvider { get; set; }

        [AssemblyInitialize]
        public static void InitPackUriHelper(TestContext context)
        {

            PackUriHelper.Create(new Uri("reliable://0"));
        }
        
        [TestInitialize]
        public void TestInit()
        {    
            this.DataProvider = new DedicabDataProvider();
        }

        [TestMethod]
        [DeploymentItem(@"TestData\ITG", @"Songs\ITG")]
        public void GetSongDataTest()
        {
            var stepmaniaRoot = new DirectoryInfo(Directory.GetCurrentDirectory());
            var groups = this.DataProvider.GetUpdatedSongData(stepmaniaRoot, new Progress<string>(s => { }));

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(68, groups.Single().Songs.Count());
        }

        [TestMethod]
        [DeploymentItem(@"TestData\ITG", @"Songs\ITG")]
        [DeploymentItem(@"TestData\NewSongs", @"NewSongs\NewPack")]
        public void AddSongDataTest()
        {
            var newSongs = Directory.EnumerateFiles(@"NewSongs\NewPack", "*.sm", SearchOption.AllDirectories).Select(s => new FileInfo(s));
            var stepmaniaRoot = new DirectoryInfo(Directory.GetCurrentDirectory());
            
            var newSongGroup = this.DataProvider.AddNewSongs(stepmaniaRoot, newSongs, @"NewPack", new Progress<string>(s => { }));
            Assert.AreEqual(4, newSongGroup.Songs.Count());
            Assert.AreEqual("NewPack", newSongGroup.Name);

            //TODO: This test cannot verify that all the related sm file content (mp3/ogg, banners) is copied alongside the .sm to the proper directory.
            var allGroups = this.DataProvider.GetUpdatedSongData(stepmaniaRoot, new Progress<string>(s => { }));
            Assert.AreEqual(2, allGroups.Count);
            Assert.AreEqual(72, allGroups.SelectMany(g => g.Songs).Count());

        }


        [TestMethod]
        [DeploymentItem(@"TestData\ITG", @"Songs\ITG")]
        [DeploymentItem(@"TestData\NewSongs", @"NewSongs\NewPack")]
        [ExpectedException(typeof(DuplicateSongPackException))]
        public void AddSongDataWithExistingPackNameTest()
        {
            var newSongs = Directory.EnumerateFiles(@"NewSongs\NewPack", "*.sm", SearchOption.AllDirectories).Select(s => new FileInfo(s));
            var stepmaniaRoot = new DirectoryInfo(Directory.GetCurrentDirectory());

            this.DataProvider.AddNewSongs(stepmaniaRoot, newSongs, @"ITG", new Progress<string>(s => { }));
        }
    }
}
