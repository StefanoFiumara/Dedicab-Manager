﻿using System;
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
            Name = name;
            Songs = songs;
            GroupId = Guid.NewGuid();
        }
        
        public bool Equals(SongGroupModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return GroupId.Equals(other.GroupId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SongGroupModel) obj);
        }

        public override int GetHashCode()
        {
            return GroupId.GetHashCode();
        }
    }
}