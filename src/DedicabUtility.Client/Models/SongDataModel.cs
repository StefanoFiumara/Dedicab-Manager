using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using StepmaniaUtils.Core;
using StepmaniaUtils.Enums;

namespace DedicabUtility.Client.Models
{
    public class SongDataModel
    { 
        private const string DefaultBannerPath = @"pack://application:,,,/DedicabUtility.Client;component/Images/defaultbanner.png";
        private static readonly Uri DefaultBannerUri = new Uri(DefaultBannerPath);

        private readonly SmFile _smFile;

        public string SongName => _smFile.SongTitle;
        public string Group => _smFile.Group;
        public string Directory => _smFile.Directory;

        public BitmapImage SongBanner { get; set; }

        public Dictionary<SongDifficulty, int> DifficultySingles { get; set; }
        public Dictionary<SongDifficulty, int> DifficultyDoubles { get; set; }

        public SongDataModel(SmFile smFile)
        {
            _smFile = smFile;

            SongBanner = ExtractBannerImage();
            ExtractDifficultyRatings();
        }
        
        private BitmapImage ExtractBannerImage()
        {
            BitmapImage image = new BitmapImage();

            try
            {
                var filePath = Path.Combine(_smFile.Directory, _smFile.BannerPath);

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
                    return new BitmapImage(DefaultBannerUri);
                }
            }
            catch (Exception)
            {
                Console.WriteLine($@"Could not load banner image for {_smFile.FilePath}");
                Console.WriteLine($@"Banner path: {_smFile.BannerPath}");
                return new BitmapImage(DefaultBannerUri);
            }

            return image;
        }
        
        private void ExtractDifficultyRatings()
        {
            DifficultySingles = new Dictionary<SongDifficulty, int>
            {
                [SongDifficulty.Beginner] = _smFile.ChartMetadata.GetSteps(PlayStyle.Single, SongDifficulty.Beginner)?.DifficultyRating ?? -1,
                [SongDifficulty.Easy] = _smFile.ChartMetadata.GetSteps(PlayStyle.Single, SongDifficulty.Easy)?.DifficultyRating ?? -1,
                [SongDifficulty.Medium] = _smFile.ChartMetadata.GetSteps(PlayStyle.Single, SongDifficulty.Medium)?.DifficultyRating ?? -1,
                [SongDifficulty.Hard] = _smFile.ChartMetadata.GetSteps(PlayStyle.Single, SongDifficulty.Hard)?.DifficultyRating ?? -1,
                [SongDifficulty.Challenge] = _smFile.ChartMetadata.GetSteps(PlayStyle.Single, SongDifficulty.Challenge)?.DifficultyRating ?? -1
            };


            DifficultyDoubles = new Dictionary<SongDifficulty, int>
            {
                [SongDifficulty.Beginner] = _smFile.ChartMetadata.GetSteps(PlayStyle.Double, SongDifficulty.Beginner)?.DifficultyRating ?? -1,
                [SongDifficulty.Easy] = _smFile.ChartMetadata.GetSteps(PlayStyle.Double, SongDifficulty.Easy)?.DifficultyRating ?? -1,
                [SongDifficulty.Medium] = _smFile.ChartMetadata.GetSteps(PlayStyle.Double, SongDifficulty.Medium)?.DifficultyRating ?? -1,
                [SongDifficulty.Hard] = _smFile.ChartMetadata.GetSteps(PlayStyle.Double, SongDifficulty.Hard)?.DifficultyRating ?? -1,
                [SongDifficulty.Challenge] = _smFile.ChartMetadata.GetSteps(PlayStyle.Double, SongDifficulty.Challenge)?.DifficultyRating ?? -1
            };
        }
    }
}