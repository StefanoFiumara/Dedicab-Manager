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
        private const string DefaultBannerPath = @"pack://application:,,,/DedicabUtility.Client;component/Images/defaultbanner.png";
        private readonly SmFile _smFile;

        public string SongName => _smFile.SongName;
        public string Group => _smFile.Group;
        public BitmapImage SongBanner { get; set; }

        public Dictionary<SongDifficulty, int> DifficultySingles { get; set; }
        public Dictionary<SongDifficulty, int> DifficultyDoubles { get; set; }

        public SongDataModel(SmFile smFile, ChartData smFileMetadata = null)
        {
            _smFile = smFile;

            SongBanner = ExtractBannerImage();

            if (smFileMetadata != null)
            {
                ExtractDifficultyRatings(smFileMetadata);
                smFileMetadata.Dispose();
            }
            else
            {
                using (var data = _smFile.ExtractChartData(extractAllStepData: false))
                {
                    ExtractDifficultyRatings(data);
                }
            }
        }
        
        private BitmapImage ExtractBannerImage()
        {
            var filePath = Path.Combine(_smFile.Directory, _smFile.BannerPath);

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
                    var uriSource = new Uri(DefaultBannerPath);
                    return new BitmapImage(uriSource);
                }
                catch (Exception)
                {
                    return image;
                }
            }

            return image;
        }
        
        private void ExtractDifficultyRatings(ChartData data)
        {
            DifficultySingles = new Dictionary<SongDifficulty, int>
            {
                [SongDifficulty.Beginner] = data.GetSteps(PlayStyle.Single, SongDifficulty.Beginner)?.DifficultyRating ?? -1,
                [SongDifficulty.Easy] = data.GetSteps(PlayStyle.Single, SongDifficulty.Easy)?.DifficultyRating ?? -1,
                [SongDifficulty.Medium] = data.GetSteps(PlayStyle.Single, SongDifficulty.Medium)?.DifficultyRating ?? -1,
                [SongDifficulty.Hard] = data.GetSteps(PlayStyle.Single, SongDifficulty.Hard)?.DifficultyRating ?? -1,
                [SongDifficulty.Challenge] = data.GetSteps(PlayStyle.Single, SongDifficulty.Challenge)?.DifficultyRating ?? -1
            };


            DifficultyDoubles = new Dictionary<SongDifficulty, int>
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