using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DedicabUtility.Client.Annotations;
using DedicabUtility.Client.Models;
using StepmaniaUtils.Core;

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
    }
}