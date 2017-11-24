using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using StepmaniaUtils.Core;
using StepmaniaUtils.Enums;
using StepmaniaUtils.StepChart;

namespace DedicabUtility.Client.Models
{
    public class SongDataModel
    { 
        private const string DEFAULT_BANNER_PATH = @"pack://application:,,,/DedicabUtility.Client;component/Images/defaultbanner.png";
        private readonly SmFile _smFile;

        public string SongName => this._smFile.SongName;
        public string Group => this._smFile.Group;
        public BitmapImage SongBanner { get; set; }

        public Dictionary<SongDifficulty, int> DifficultySingles { get; set; }
        public Dictionary<SongDifficulty, int> DifficultyDoubles { get; set; }

        public SongDataModel(SmFile smFile)
        {
            this._smFile = smFile;

            this.SongBanner = this.ExtractBannerImage();
            

            using (var data = this._smFile.ExtractChartData())
            {
                this.ExtractDifficultyRatings(data);
            }
        }

        public SongDataModel(SongDataModel copy)
        {
            this._smFile = copy._smFile;

            this.SongBanner = this.ExtractBannerImage();

            this.DifficultySingles = copy.DifficultySingles;
            this.DifficultyDoubles = copy.DifficultyDoubles;

        }

        private BitmapImage ExtractBannerImage()
        {
            var filePath = Path.Combine(this._smFile.Directory, this._smFile.BannerPath);

            BitmapImage image = new BitmapImage();
            if (File.Exists(filePath))
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                }
            }
            else
            {
                try
                {
                    var uriSource = new Uri(SongDataModel.DEFAULT_BANNER_PATH);
                    return new BitmapImage(uriSource);
                }
                catch (Exception e)
                {
                    return image;
                }
            }

            return image;
        }
        
        private void ExtractDifficultyRatings(ChartData data)
        {
            this.DifficultySingles = new Dictionary<SongDifficulty, int>
            {
                [SongDifficulty.Beginner] = data.GetSteps(PlayStyle.Single, SongDifficulty.Beginner)?.DifficultyRating ?? -1,
                [SongDifficulty.Easy] = data.GetSteps(PlayStyle.Single, SongDifficulty.Easy)?.DifficultyRating ?? -1,
                [SongDifficulty.Medium] = data.GetSteps(PlayStyle.Single, SongDifficulty.Medium)?.DifficultyRating ?? -1,
                [SongDifficulty.Hard] = data.GetSteps(PlayStyle.Single, SongDifficulty.Hard)?.DifficultyRating ?? -1,
                [SongDifficulty.Challenge] = data.GetSteps(PlayStyle.Single, SongDifficulty.Challenge)?.DifficultyRating ?? -1
            };


            this.DifficultyDoubles = new Dictionary<SongDifficulty, int>
            {
                [SongDifficulty.Beginner] = data.GetSteps(PlayStyle.Double, SongDifficulty.Beginner)?.DifficultyRating ?? -1,
                [SongDifficulty.Easy] = data.GetSteps(PlayStyle.Double, SongDifficulty.Easy)?.DifficultyRating ?? -1,
                [SongDifficulty.Medium] = data.GetSteps(PlayStyle.Double, SongDifficulty.Medium)?.DifficultyRating ?? -1,
                [SongDifficulty.Hard] = data.GetSteps(PlayStyle.Double, SongDifficulty.Hard)?.DifficultyRating ?? -1,
                [SongDifficulty.Challenge] = data.GetSteps(PlayStyle.Double, SongDifficulty.Challenge)?.DifficultyRating ?? -1
            };
        }
    }
}