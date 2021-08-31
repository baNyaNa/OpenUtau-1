﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenUtau.Core.Ustx;

namespace OpenUtau.Core
{
    public abstract class TrackCommand : UCommand
    {
        public UProject project;
        public UTrack track;
        public void UpdateTrackNo()
        {
            Dictionary<int, int> trackNoRemapTable = new Dictionary<int, int>();
            for (int i = 0; i < project.tracks.Count; i++)
            {
                if (project.tracks[i].TrackNo != i)
                {
                    trackNoRemapTable.Add(project.tracks[i].TrackNo, i);
                    project.tracks[i].TrackNo = i;
                }
            }

            foreach (var part in project.parts)
            {
                if (trackNoRemapTable.Keys.Contains(part.trackNo))
                    part.trackNo = trackNoRemapTable[part.trackNo];
            }
        }
    }

    public class AddTrackCommand : TrackCommand
    {
        public AddTrackCommand(UProject project, UTrack track) { this.project = project; this.track = track; }
        public override string ToString() { return "Add track"; }
        public override void Execute()
        {
            if (track.TrackNo < project.tracks.Count) project.tracks.Insert(track.TrackNo, track);
            else project.tracks.Add(track);
            UpdateTrackNo();
        }
        public override void Unexecute() { project.tracks.Remove(track); UpdateTrackNo(); }
    }

    public class RemoveTrackCommand : TrackCommand
    {
        public List<UPart> removedParts = new List<UPart>();
        public RemoveTrackCommand(UProject project, UTrack track)
        {
            this.project = project;
            this.track = track;
            foreach (var part in project.parts)
            {
                if (part.trackNo == track.TrackNo)
                    removedParts.Add(part);
            }
        }
        public override string ToString() { return "Remove track"; }
        public override void Execute() {
            project.tracks.Remove(track);
            foreach (var part in removedParts)
            {
                project.parts.Remove(part);
                part.trackNo = -1;
            }
            UpdateTrackNo();
        }
        public override void Unexecute()
        {
            if (track.TrackNo < project.tracks.Count)
                project.tracks.Insert(track.TrackNo, track);
            else
                project.tracks.Add(track);
            foreach (var part in removedParts)
                project.parts.Add(part);
            track.TrackNo = -1;
            UpdateTrackNo();
        }
    }

    public class TrackChangeSingerCommand : TrackCommand
    {
        readonly USinger newSinger, oldSinger;
        public TrackChangeSingerCommand(UProject project, UTrack track, USinger newSinger) {
            this.project = project;
            this.track = track;
            this.newSinger = newSinger;
            this.oldSinger = track.Singer;
        }
        public override string ToString() { return "Change singer"; }
        public override void Execute() { track.Singer = newSinger; }
        public override void Unexecute() { track.Singer = oldSinger; }
    }
}