using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DedicabUtility.Client.Exceptions;
using DedicabUtility.Client.Models;
using StepmaniaUtils.Core;
using StepmaniaUtils.Enums;

namespace DedicabUtility.Client.Services
{
    public sealed class DedicabDataService
    {
        public List<SongGroupModel> GetUpdatedSongData(DirectoryInfo stepmaniaRoot, IProgress<string> progress)
        {
            var songsPath = Path.Combine(stepmaniaRoot.FullName, @"Songs");

            progress.Report("Enumerating *.sm files...");
            var files = Directory.EnumerateFiles(songsPath, "*.sm", SearchOption.AllDirectories).ToList();

            progress.Report("Parsing simfile metadata... 0%");

            var smFiles = new List<SmFile>();
            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                var smFile = new SmFile(new FileInfo(file));
                smFiles.Add(smFile);

                progress.Report($"Parsing simfile metadata... {(int)((float)i / files.Count * 100f)}%");
            }

            var groups = smFiles
                .GroupBy(s => s.Group)
                .ToList();

            var songGroupModels = new List<SongGroupModel>();
            
            progress.Report("Parsing chart data... 0%");
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                var songDataModels = group.Select(s => new SongDataModel(s)).ToList();

                var groupModel = new SongGroupModel(group.Key, songDataModels.OrderBy(s => s.SongName));
                songGroupModels.Add(groupModel);

                progress.Report($"Parsing chart data... {(int)((float)i / groups.Count * 100f)}%");                
            }

            return new List<SongGroupModel>(songGroupModels.OrderBy(m => m.Name));
        }

        public SongGroupModel AddNewSongs(DirectoryInfo stepmaniaRoot, IEnumerable<FileInfo> newSongs, string newPackName, IProgress<string> progress)
        {
            progress.Report("Creating Directory...");
            var newPackPath = new DirectoryInfo(Path.Combine(stepmaniaRoot.FullName, @"Songs", newPackName));

            if (newPackPath.Exists == false)
            {
                newPackPath.Create();
            }
            else
            {
                throw new DuplicateSongPackException();
            }
            
            var smFiles = newSongs.Where(f => f.Exists).Select(f => new SmFile(f)).ToList();

            progress.Report("Copying to Stepmania Installation...");
            
            for (int i = 0; i < smFiles.Count; i++)
            {
                var smFile = smFiles[i];
                using (var chartData = smFile.ExtractChartData())
                {
                    if (chartData.GetSteps(PlayStyle.Lights, SongDifficulty.Easy) == null)
                    {
                        var referenceChart = chartData.GetSteps(PlayStyle.Single, SongDifficulty.Hard)
                                             ?? chartData.GetSteps(PlayStyle.Single, SongDifficulty.Challenge)
                                             ?? chartData.GetSteps(PlayStyle.Single,
                                                 chartData.GetHighestChartedDifficulty(PlayStyle.Single))
                                             ?? chartData.GetSteps(PlayStyle.Double, SongDifficulty.Hard)
                                             ?? chartData.GetSteps(PlayStyle.Double, SongDifficulty.Challenge)
                                             ?? chartData.GetSteps(PlayStyle.Double,
                                                 chartData.GetHighestChartedDifficulty(PlayStyle.Double));

                        var lightsChart = StepChartBuilder.GenerateLightsChart(referenceChart);

                        chartData.AddNewStepchart(lightsChart);
                    }
                }

                var relatedStepFiles = Directory.EnumerateFiles(smFile.Directory).Select(f => new FileInfo(f)).Where(f => f.Extension != ".ssc");

                //Strip out invalid file name chars
                string songName = Path.GetInvalidFileNameChars()
                    .Aggregate(smFile.SongName, (current, c) => current.Replace(c.ToString(), ""));

                var songPath = Path.Combine(newPackPath.FullName, songName);

                Directory.CreateDirectory(songPath);

                foreach (var file in relatedStepFiles)
                {
                    file.CopyTo(Path.Combine(songPath, file.Name), overwrite: true);
                }

                progress.Report($"Copying to Stepmania Installation... {(int)((float)i / smFiles.Count * 100f)}%");
            }

            //Reassign SmFiles to the newly copied location.
            progress.Report("Reading new metadata...");
            smFiles = Directory.EnumerateFiles(newPackPath.FullName, "*.sm", SearchOption.AllDirectories).Select(s => new SmFile(new FileInfo(s))).ToList();
            
            return new SongGroupModel(newPackName, smFiles.Select(sm => new SongDataModel(sm)).OrderBy(sm => sm.SongName));
        }

        public void RemoveSongPack(DirectoryInfo stepmaniaRoot, string songPackName, IProgress<string> progress)
        {
            if (string.IsNullOrEmpty(songPackName))
            {
                throw new ArgumentNullException(nameof(songPackName));
            }
            var songPackPath = new DirectoryInfo(Path.Combine(stepmaniaRoot.FullName, "Songs", songPackName));
            var removedSongsCache = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "RemovedSongsCache"));

            if (!songPackPath.Exists)
            {
                throw new SongPackNotFoundException();
            }

            if (!removedSongsCache.Exists)
            {
                removedSongsCache.Create();
            }

            progress.Report("Enumerating Files...");
            var filesToRemove = Directory.EnumerateFiles(songPackPath.FullName, "*.*", SearchOption.AllDirectories).Select(f => new FileInfo(f)).ToList();

            for (int i = 0; i < filesToRemove.Count; i++)
            {
                var file = filesToRemove[i];

                progress.Report($"Removing Songs...{(int)((float)i/filesToRemove.Count * 100f)}%");

                var cache = new FileInfo(Path.Combine(removedSongsCache.FullName, file.Directory.Name, file.Name));
                if (cache.Directory?.Exists == false)
                {
                    cache.Directory.Create();
                }

                File.Copy(file.FullName, cache.FullName, overwrite: true);
                File.SetAttributes(cache.FullName, FileAttributes.Normal);

                File.SetAttributes(file.FullName, FileAttributes.Normal);
                File.Delete(file.FullName);
            }

            Directory.Delete(songPackPath.FullName, recursive: true);
        }
    }
}