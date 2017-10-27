using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DedicabUtility.Client.Annotations;
using DedicabUtility.Client.Exceptions;
using DedicabUtility.Client.Models;
using StepmaniaUtils.Core;
using StepmaniaUtils.Enums;

namespace DedicabUtility.Client.Services
{
    public sealed class DedicabDataProvider : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //TODO: We need to reconsider where we put this model, this is a service.
        private ObservableCollection<SongGroupModel> _songGroups;
        public ObservableCollection<SongGroupModel> SongGroups
        {
            get => this._songGroups;
            set
            {
                if (Equals(value, this._songGroups)) return;
                this._songGroups = value;
                this.OnPropertyChanged();
            }
        }

        public DedicabDataProvider()
        {
            this.SongGroups = new ObservableCollection<SongGroupModel>();
        }

        public List<SongGroupModel> GetUpdatedSongData(DirectoryInfo stepmaniaRoot, IProgress<string> progress)
        {
            var songsPath = Path.Combine(stepmaniaRoot.FullName, @"Songs");

            progress.Report("Enumerating *.sm files...");
            var files = Directory.EnumerateFiles(songsPath, "*.sm", SearchOption.AllDirectories).ToList();

            int progressPercent = 0;
            int totalFiles = files.Count;
            int processedFiles = 0;

            progress.Report("Parsing simfile metadata... 0%");

            var smFiles = new List<SmFile>();
            foreach (string file in files)
            {
                var smFile = new SmFile(new FileInfo(file));
                smFiles.Add(smFile);

                processedFiles++;

                float percent = ((float)processedFiles / totalFiles) * 100f;
                if ((int) percent != progressPercent)
                {
                    progressPercent = (int)percent;
                    progress.Report($"Parsing simfile metadata... {progressPercent}%");
                }
            }

            var groups = smFiles
                .GroupBy(s => s.Group)
                .ToList();

            int totalGroups = groups.Count;
            
            progressPercent = 0;
            processedFiles = 0;

            var songGroupModels = new List<SongGroupModel>();
            
            progress.Report("Parsing chart data... 0%");
            foreach (var group in groups)
            {
                var songDataModels = group.Select(s => new SongDataModel(s)).ToList();

                var groupModel = new SongGroupModel(group.Key, songDataModels.OrderBy(s => s.SongName));
                songGroupModels.Add(groupModel);

                processedFiles++;
                float percent = ((float)processedFiles / totalGroups) * 100f;

                if ((int)percent != progressPercent)
                {
                    progressPercent = (int)percent;
                    progress.Report($"Parsing chart data... {progressPercent}%");
                }
            }

            return new List<SongGroupModel>(songGroupModels.OrderBy(m => m.Name));
        }

        public SongGroupModel AddNewSongs(DirectoryInfo stepmaniaRoot, IEnumerable<FileInfo> newSongs, string newPackName, IProgress<string> progress)
        {
            //TODO: Progress Reports
            progress.Report("Test");
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

            foreach (var smFile in smFiles)
            {
                using (var chartData = smFile.ExtractChartData())
                {
                    if (chartData.GetSteps(PlayStyle.Lights, SongDifficulty.Easy) == null)
                    {
                        var referenceChart = chartData.GetSteps(PlayStyle.Single, SongDifficulty.Hard)
                                             ?? chartData.GetSteps(PlayStyle.Single, SongDifficulty.Challenge)
                                             ?? chartData.GetSteps(PlayStyle.Single, chartData.GetHighestChartedDifficulty(PlayStyle.Single))
                                             ?? chartData.GetSteps(PlayStyle.Double, SongDifficulty.Hard)
                                             ?? chartData.GetSteps(PlayStyle.Double, SongDifficulty.Challenge)
                                             ?? chartData.GetSteps(PlayStyle.Double, chartData.GetHighestChartedDifficulty(PlayStyle.Double));

                        var lightsChart = StepChartBuilder.GenerateLightsChart(referenceChart);

                        chartData.AddNewStepchart(lightsChart);
                    }
                }

                var relatedStepFiles = Directory.EnumerateFiles(smFile.Directory).Select(f => new FileInfo(f));
                var songPath = Path.Combine(newPackPath.FullName, smFile.SongName);

                Directory.CreateDirectory(songPath);

                foreach (var file in relatedStepFiles)
                {
                    file.CopyTo(Path.Combine(songPath, file.Name));
                }
            }

            //TODO: The SongDataModel should hold the smFile from the new path, not the old path.
            //return the group model instead so this can be called from a separate thread.
            return new SongGroupModel(newPackName, smFiles.Select(sm => new SongDataModel(sm)));
        }
    }
}