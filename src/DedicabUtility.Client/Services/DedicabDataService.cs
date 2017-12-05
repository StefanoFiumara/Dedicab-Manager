using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DedicabUtility.Client.Exceptions;
using DedicabUtility.Client.Models;
using StepmaniaUtils.Core;
using StepmaniaUtils.Enums;
using StepmaniaUtils.StepChart;

namespace DedicabUtility.Client.Services
{
    public class SongMetadata
    {
        public SmFile SmFile { get; set; }
        public ChartData ChartData { get; set; }

        public SongMetadata(string smFilePath)
        {
            SmFile = new SmFile(smFilePath);
            ChartData = SmFile.ExtractChartData(extractAllStepData: false);
        }
    }
    public sealed class DedicabDataService
    {
        public List<IGrouping<string, SongMetadata>> ScanSongData(string stepmaniaRootPath, IProgress<string> progress)
        {
            var songsPath = Path.Combine(stepmaniaRootPath, @"Songs");

            progress.Report("Scanning Song Library...");
            return Directory.EnumerateFiles(songsPath, "*.sm", SearchOption.AllDirectories)
                            .AsParallel()
                            .Select(f =>  new SongMetadata(f))
                            .GroupBy(s => s.SmFile.Group)
                            .ToList();
        }

        public SongGroupModel AddNewSongs(string stepmaniaRoot, IEnumerable<string> newSongs, string newPackName, IProgress<string> progress)
        {
            progress.Report("Creating Directory...");
            var newPackPath = Path.Combine(stepmaniaRoot, @"Songs", newPackName);

            if (!Directory.Exists(newPackPath))
            {
                Directory.CreateDirectory(newPackPath);
            }
            else
            {
                throw new DuplicateSongPackException();
            }

            var smFiles = newSongs.Where(File.Exists).Select(f => new SmFile(f));

            progress.Report("Copying to Stepmania Installation...");

            Parallel.ForEach(smFiles, smFile =>
            {
                using (var chartData = smFile.ExtractChartData())
                {
                    if (chartData.GetSteps(PlayStyle.Lights, SongDifficulty.Easy) == null)
                    {
                        var referenceChart =    chartData.GetSteps(PlayStyle.Single, SongDifficulty.Hard)
                                             ?? chartData.GetSteps(PlayStyle.Single, SongDifficulty.Challenge)
                                             ?? chartData.GetSteps(PlayStyle.Single, chartData.GetHighestChartedDifficulty(PlayStyle.Single))
                                             ?? chartData.GetSteps(PlayStyle.Double, SongDifficulty.Hard)
                                             ?? chartData.GetSteps(PlayStyle.Double, SongDifficulty.Challenge)
                                             ?? chartData.GetSteps(PlayStyle.Double, chartData.GetHighestChartedDifficulty(PlayStyle.Double));

                        var lightsChart = StepChartBuilder.GenerateLightsChart(referenceChart);

                        chartData.AddNewStepchart(lightsChart);
                    }
                }

                var relatedStepFiles = Directory.EnumerateFiles(smFile.Directory).Where(f => !f.EndsWith(".ssc"));

                //Strip out invalid file name chars
                string songName = Path.GetInvalidFileNameChars()
                    .Aggregate(smFile.SongName, (current, c) => current.Replace(c.ToString(), ""));

                var songPath = Path.Combine(newPackPath, songName);

                Directory.CreateDirectory(songPath);

                foreach (var file in relatedStepFiles)
                {
                    string fileName = file.Split(Path.DirectorySeparatorChar).Last();
                    File.Copy(file, Path.Combine(songPath, fileName), overwrite: true);
                }
            });

            //Reassign SmFiles to the newly copied location.
            progress.Report("Reading new metadata...");
            var songDataModels = Directory.EnumerateFiles(newPackPath, "*.sm", SearchOption.AllDirectories)
                               .Select(s => new SmFile(s))
                               .Select(sm => new SongDataModel(sm))
                               .OrderBy(sm => sm.SongName)
                               .ToList();

            return new SongGroupModel(newPackName, songDataModels);
        }

        public void RemoveSongPack(string stepmaniaRoot, string songPackName, IProgress<string> progress)
        {
            var songPackPath = Path.Combine(stepmaniaRoot, "Songs", songPackName);
            var removedSongsCache = Path.Combine(Directory.GetCurrentDirectory(), "RemovedSongsCache");

            if (string.IsNullOrEmpty(songPackName))
            {
                throw new ArgumentNullException(nameof(songPackName));
            }

            if (!Directory.Exists(songPackPath))
            {
                throw new SongPackNotFoundException();
            }

            if (!Directory.Exists(removedSongsCache))
            {
                Directory.CreateDirectory(removedSongsCache);
            }

            progress.Report("Removing Songs...");
            var filesToRemove = Directory.EnumerateFiles(songPackPath, "*.*", SearchOption.AllDirectories);

            Parallel.ForEach(filesToRemove, file =>
            {
                var directoryName = Path.GetDirectoryName(file)?.Split(Path.DirectorySeparatorChar).Last();
                var fileName = file.Split(Path.DirectorySeparatorChar).Last();
                var cache = new FileInfo(Path.Combine(removedSongsCache, directoryName ?? string.Empty, fileName));
                if (cache.Directory?.Exists == false)
                {
                    cache.Directory.Create();
                }

                File.Copy(file, cache.FullName, overwrite: true);
                File.SetAttributes(cache.FullName, FileAttributes.Normal);

                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            });

            Directory.Delete(songPackPath, recursive: true);
        }
    }
}