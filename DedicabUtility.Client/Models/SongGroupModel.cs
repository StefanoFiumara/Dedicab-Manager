using System;
using System.Collections.Generic;
using System.Linq;

namespace DedicabUtility.Client.Models
{
    public class SongGroupModel : IEquatable<SongGroupModel>
    {
        public Guid GroupId { get; }

        public string Name { get; }
        public IEnumerable<SongDataModel> Songs { get; }

        public SongGroupModel(string name, IEnumerable<SongDataModel> songs)
        {
            this.Name = name;
            this.Songs = songs;
            this.GroupId = Guid.NewGuid();
        }

        public SongGroupModel(SongGroupModel copy)
        {
            this.Name = copy.Name;
            this.Songs = copy.Songs.Select(s => new SongDataModel(s)).ToList();
            this.GroupId = copy.GroupId;
        }

        public bool Equals(SongGroupModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return this.GroupId.Equals(other.GroupId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SongGroupModel) obj);
        }

        public override int GetHashCode()
        {
            return this.GroupId.GetHashCode();
        }
    }
}