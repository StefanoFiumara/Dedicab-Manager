using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DedicabUtility.Client.Exceptions;
using DedicabUtility.Client.Models;
using Fano.Logging.Core;
using StepmaniaUtils.Core;
using StepmaniaUtils.Enums;
using StepmaniaUtils.StepGenerator;

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
        private readonly ILogger _log;

        public DedicabDataService(ILogger log)
        {
            _log = log;
        }

        public async Task<List<IGrouping<string, SongMetadata>>> ScanSongDataAsync(string stepmaniaRootPath, IProgress<string> progress)
        {
            var fileQueue = new BlockingCollection<string>();
            var result = new ConcurrentBag<SongMetadata>();

            _log.Info($"{nameof(ScanSongDataAsync)} - Scanning Song Library");
            progress.Report("Scanning Song Library: 0");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var producer = Task.Run(() =>
            {
                var songsPath = Path.Combine(stepmaniaRootPath, @"Songs");
                foreach (var file in Directory.EnumerateFiles(songsPath, "*.sm", SearchOption.AllDirectories).AsParallel())
                {
                    fileQueue.Add(file);
                }

                fileQueue.CompleteAdding();
            });

            _log.Info($"{nameof(ScanSongDataAsync)} - Creating consumer queue, workers: {Environment.ProcessorCount}");

            var logEntries = new ConcurrentBag<LogEntry>();

            var consumers = Enumerable.Range(0, Environment.ProcessorCount)
                .Select(_ => Task.Run(() =>
                {
                    foreach (string file in fileQueue.GetConsumingEnumerable())
                    {
                        try
                        {
                            var song = new SongMetadata(file);
                            result.Add(song);

                            progress.Report($"Scanning Song Library: {result.Count}");
                        }
                        catch (Exception e)
                        {
                            var logEntry = new LogEntry
                            {
                                Timestamp = DateTime.Now,
                                Message = $"{nameof(ScanSongDataAsync)} - Could not load file at: {file}",
                                LogLevel = LogLevel.Error
                            };
                            var exceptionEntry = new LogEntry
                            {
                                Timestamp = DateTime.Now.AddMilliseconds(1),
                                Message = $"{e}",
                                LogLevel = LogLevel.Error
                            };

                            logEntries.Add(logEntry);
                            logEntries.Add(exceptionEntry);
                        }
                    }
                }));
            
            await Task.WhenAll(consumers);
            
            stopwatch.Stop();
            LogAsyncEntries(logEntries);

            _log.Info($"{nameof(ScanSongDataAsync)} - Songs Loaded: {result.Count}");
            _log.Info($"{nameof(ScanSongDataAsync)} - Loading time: {stopwatch.ElapsedMilliseconds} ms");
            return result.GroupBy(s => s.SmFile.Group).ToList();
        }

        public List<IGrouping<string, SongMetadata>> ScanSongData(string stepmaniaRootPath, IProgress<string> progress)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var songsPath = Path.Combine(stepmaniaRootPath, @"Songs");

            _log.Info($"{nameof(ScanSongData)} - Scanning Song Library");
            progress.Report("Scanning Song Library...");
            var songGroups = Directory.EnumerateFiles(songsPath, "*.sm", SearchOption.AllDirectories)
                .AsParallel()
                .Select(f =>
                {
                    try
                    {
                        return new SongMetadata(f);
                    }
                    catch (Exception e)
                    {
                        _log.Error($"{nameof(ScanSongData)} - Could not load file at: {f}");
                        _log.Error($"{e}");
                        return null;
                    }

                })
                .Where(s => s != null)
                .GroupBy(s => s.SmFile.Group)
                .ToList();

            stopwatch.Stop();

            _log.Info($"{nameof(ScanSongData)} - Loading time: {stopwatch.ElapsedMilliseconds} ms");

            return songGroups;
        }

        private void LogAsyncEntries(IEnumerable<LogEntry> logEntries)
        {
            foreach (var entry in logEntries.OrderBy(t => t.Timestamp))
            {
                switch (entry.LogLevel)
                {
                    case LogLevel.None:
                    case LogLevel.Info:
                        _log.Info(entry.Message);
                        break;
                    case LogLevel.Warning:
                        _log.Warning(entry.Message);
                        break;
                    case LogLevel.Error:
                        _log.Error(entry.Message);
                        break;
                }
            }
        }

        public SongGroupModel AddNewSongs(string stepmaniaRoot, IEnumerable<string> newSongs, string selectedDirectory, IProgress<string> progress)
        {            
            string newPackName = selectedDirectory.Split(Path.DirectorySeparatorChar).Last();
            string newPackPath = Path.Combine(stepmaniaRoot, @"Songs", newPackName);

            _log.Info($"{nameof(AddNewSongs)} - Begin");

            progress.Report("Creating Directory...");
            CreateSongPackDirectory(newPackPath);

            progress.Report("Copying Group Level Files...");
            CopyGroupFiles(selectedDirectory, newPackPath);

            progress.Report("Copying Song Level Files...");
            foreach (string file in newSongs.Where(File.Exists))
            {
                var smFile = new SmFile(file);
                CopySongFiles(smFile, newPackPath);
            }
            
            //Reassign SmFiles to the newly copied location.
            progress.Report("Reading new metadata...");
            var songDataModels = Directory.EnumerateFiles(newPackPath, "*.sm", SearchOption.AllDirectories)
                .Select(s => new SmFile(s))
                .Select(sm => new SongDataModel(sm))
                .OrderBy(sm => sm.SongName);

            return new SongGroupModel(newPackName, songDataModels);
        }

        private void CreateSongPackDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                throw new DuplicateSongPackException();
            }
        }

        private void CopyGroupFiles(string selectedDirectory, string newPackPath)
        {
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
                _log.Error($"{nameof(CopyGroupFiles)} - failed to copy group level files related to new song group.");
                _log.Error($"{e}");
            }
        }

        private void CopySongFiles(SmFile smFile, string path)
        {
            if (smFile.ChartMetadata.GetSteps(PlayStyle.Lights, SongDifficulty.Easy) == null)
            {
                var lightsChart = StepChartBuilder.GenerateLightsChart(smFile);
                smFile.AddLightsChart(lightsChart);
            }

            var relatedFiles = Directory.EnumerateFiles(smFile.Directory).Where(f => !f.EndsWith(".ssc"));

            //Strip out invalid file name chars
            string songName = RemoveInvalidPathChars(smFile.SongTitle);

            var songPath = Path.Combine(path, songName);

            try
            {
                Directory.CreateDirectory(songPath);
                foreach (var f in relatedFiles)
                {
                    string fileName = f.Split(Path.DirectorySeparatorChar).Last();
                    File.Copy(f, Path.Combine(songPath, fileName), overwrite: true);
                }
            }
            catch (Exception e)
            {
                _log.Error($@"{nameof(CopySongFiles)} - Failed copying files related to song {songName}");
                _log.Error($"{e}");
                Directory.Delete(songPath);
            }
        }

        private string RemoveInvalidPathChars(string str)
        {
            return Path.GetInvalidFileNameChars()
                       .Aggregate(str, (current, c) => current.Replace(c.ToString(), ""));
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