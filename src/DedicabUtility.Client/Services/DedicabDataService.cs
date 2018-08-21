using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DedicabUtility.Client.Exceptions;
using DedicabUtility.Client.Models;
using StepmaniaUtils.Core;
using StepmaniaUtils.Enums;

namespace DedicabUtility.Client.Services
{
    public class SongMetadata
    {
        public SmFile SmFile { get; set; }
        
        public SongMetadata(string smFilePath)
        {
            SmFile = new SmFile(smFilePath);
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
                            .Select(f =>
                                        {
                                            try
                                            {
                                                return new SongMetadata(f);
                                            }
                                            catch(Exception e)
                                            {
                                                Console.WriteLine($"Could not load file at: {f}\n {e}");
                                                return null;
                                            }
                                            
                                        })
                            .Where(s => s != null)
                            .GroupBy(s => s.SmFile.Group)
                            .ToList();
        }

        public SongGroupModel AddNewSongs(string stepmaniaRoot, IEnumerable<string> newSongs, string selectedDirectory, IProgress<string> progress)
        {
            string newPackName = selectedDirectory.Split(Path.DirectorySeparatorChar).Last();

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
            
            progress.Report("Copying to Stepmania Installation...");

            var groupFiles = Directory.EnumerateFiles(selectedDirectory, "*", SearchOption.TopDirectoryOnly);

            try
            {
                foreach (var f in groupFiles)
                {
                    string fileName = f.Split(Path.DirectorySeparatorChar).Last();
                    File.Copy(f, Path.Combine(newPackPath, fileName), overwrite: true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error copying group level files related to new song group {newPackName} Exception:\n {e}");
            }

            foreach (string file in newSongs.Where(File.Exists))
            {
                var smFile = new SmFile(file);
                if (smFile.ChartMetadata.GetSteps(PlayStyle.Lights, SongDifficulty.Easy) == null)
                {
                    var lightsChart = StepChartBuilder.GenerateLightsChart(smFile);
                    smFile.AddLightsChart(lightsChart);
                }

                var relatedStepFiles = Directory.EnumerateFiles(smFile.Directory).Where(f => !f.EndsWith(".ssc"));

                //Strip out invalid file name chars
                string songName = Path.GetInvalidFileNameChars()
                    .Aggregate(smFile.SongTitle, (current, c) => current.Replace(c.ToString(), ""));

                var songPath = Path.Combine(newPackPath, songName);
                
                try
                {
                    Directory.CreateDirectory(songPath);
                    foreach (var f in relatedStepFiles)
                    {
                        string fileName = f.Split(Path.DirectorySeparatorChar).Last();
                        File.Copy(f, Path.Combine(songPath, fileName), overwrite: true);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($@"Error copying files related to song {songName}, Exception: {e}");
                    Directory.Delete(songPath);
                }
            }
            
            //Reassign SmFiles to the newly copied location.
            progress.Report("Reading new metadata...");
            var songDataModels = Directory.EnumerateFiles(newPackPath, "*.sm", SearchOption.AllDirectories)
                .Select(s => new SmFile(s))
                .Select(sm => new SongDataModel(sm))
                .OrderBy(sm => sm.SongName);

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