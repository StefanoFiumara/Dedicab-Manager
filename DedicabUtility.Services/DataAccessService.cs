using StepmaniaUtils.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DedicabUtility.Services
{
    public class DataAccessService
    {
        private readonly DirectoryInfo _stepmaniaRoot;

        public DataAccessService(DirectoryInfo stepmaniaRoot)
        {
            this._stepmaniaRoot = stepmaniaRoot;
        }

        public List<SmFile> GetSongData()
        {
            string songsFolderPath = Path.Combine(this._stepmaniaRoot.FullName, @"Songs");

            return
                Directory.EnumerateFiles(songsFolderPath, "*.sm", SearchOption.AllDirectories)
                .Select(f => new SmFile(new FileInfo(f)))
                .ToList();
        }
    }
}
