using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace ScoreList.Scores
{
    // children inherit attribute
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class BaseFilter
    {
        public abstract void Apply(ref List<LeaderboardScore> scores, LeaderboardData data);
    }

    public abstract class DisplayBaseFilter : BaseFilter
    {
        public abstract string Name { get; }
        public abstract string Display();
    }
    
    // sort
    
    public class OrderFilter : BaseFilter
    {
        [JsonProperty]
        private readonly bool _order;

        public OrderFilter(bool order) => _order = order;

        public override void Apply(ref List<LeaderboardScore> scores, LeaderboardData data)
        {
            if (_order) scores.Reverse();
        }
    }
    
    public class SortAccuracyFilter : BaseFilter
    {
        public override void Apply(ref List<LeaderboardScore> scores, LeaderboardData data)
        {
            scores.Sort((s1, s2) =>
            {
                var leaderboard1 = data.Leaderboards.First(l => l.LeaderboardId == s1.LeaderboardId);
                var leaderboard2 = data.Leaderboards.First(l => l.LeaderboardId == s2.LeaderboardId);
                
                var accuracy1 = (float) (100.0 * s1.BaseScore / leaderboard1.MaxScore);
                var accuracy2 = (float) (100.0 * s2.BaseScore / leaderboard2.MaxScore);
                
                return accuracy1.CompareTo(accuracy2);
            });
        }
    }
    
    public class SortMissedNotesFilter : BaseFilter
    {
        public override void Apply(ref List<LeaderboardScore> scores, LeaderboardData data) =>
            scores.Sort((s1, s2) => s1.MissedNotes - s2.MissedNotes);
    }
    
    public class SortTimeSetFilter : BaseFilter
    {
        public override void Apply(ref List<LeaderboardScore> scores, LeaderboardData data) =>
            scores.Sort((s1, s2) => s1.TimeSet.CompareTo(s2.TimeSet));
    }
    
    public class SortPpFilter : BaseFilter
    {
        public override void Apply(ref List<LeaderboardScore> scores, LeaderboardData data) =>
            scores.Sort((s1, s2) => s1.Pp.CompareTo(s2.Pp));
    }
    
    public class SortRankFilter : BaseFilter
    {
        public override void Apply(ref List<LeaderboardScore> scores, LeaderboardData data) =>
            scores.Sort((s1, s2) => s2.Rank - s1.Rank);
    }
    
    public class SortStarsFilter : BaseFilter
    {
        public override void Apply(ref List<LeaderboardScore> scores, LeaderboardData data)
        {
            scores.Sort((s1, s2) =>
            {
                var leaderboard1 = data.Leaderboards.First(l => l.LeaderboardId == s1.LeaderboardId);
                var leaderboard2 = data.Leaderboards.First(l => l.LeaderboardId == s2.LeaderboardId);

                return leaderboard1.Stars.CompareTo(leaderboard2.Stars);
            });
        }
    }
    
    // filters

    public class RankedFilter : BaseFilter
    {
        [JsonProperty]
        private readonly bool _isRanked;

        public RankedFilter(bool isRanked) => _isRanked = isRanked;

        public override void Apply(ref List<LeaderboardScore> scores, LeaderboardData data)
        {
            scores = scores.Where(s =>
            {
                var leaderboard = data.Leaderboards.First(l => l.LeaderboardId == s.LeaderboardId);
                return leaderboard.Ranked == _isRanked;
            }).ToList();
        }
    }
    
    public class DownloadedFilter : BaseFilter
    {
        [JsonProperty]
        private readonly bool _downloaded;

        public DownloadedFilter(bool downloaded) => _downloaded = downloaded;

        public override void Apply(ref List<LeaderboardScore> scores, LeaderboardData data)
        {
            scores = scores.Where(s =>
            {
                var leaderboard = data.Leaderboards.First(l => l.LeaderboardId == s.LeaderboardId);
                return SongDataCore.Plugin.Songs.Data.Songs.ContainsKey(leaderboard.SongHash) == _downloaded;
            }).ToList();
        }
    }

    public class StarsFilter : DisplayBaseFilter
    {
        public override string Name => "Stars";

        [JsonProperty]
        private readonly float? _start;
        [JsonProperty]
        private readonly float? _end;

        public StarsFilter(float? start, float? end)
        {
            _start = start;
            _end = end;
        }

        public override string Display()
        {
            if (_start != null && _end == null) return $"More than {_start} stars";
            if (_start == null && _end != null) return $"Less than {_end} stars";
            return $"Between {_start} and {_end} stars";
        }

        public override void Apply(ref List<LeaderboardScore> scores, LeaderboardData data)
        {
            scores = scores.Where(s =>
            {
                var leaderboard = data.Leaderboards.First(l => l.LeaderboardId == s.LeaderboardId);
                
                if (_start != null && _end == null) return leaderboard.Stars > _start;
                if (_start == null && _end != null) return leaderboard.Stars < _end;
                return leaderboard.Stars > _start && leaderboard.Stars < _end;
            }).ToList();
        }
    }

    public class DateFilter : DisplayBaseFilter
    {
        public override string Name => "Date";

        [JsonProperty]
        private readonly DateTime? _start;
        [JsonProperty]
        private readonly DateTime? _end;

        public DateFilter(string start, string end)
        {
            var provider = new CultureInfo("en-US");
            
            if (start != null) _start = DateTime.ParseExact(start, "yyyy-MM-dd", provider);
            if (end != null) _end = DateTime.ParseExact(end, "yyyy-MM-dd", provider);
        }

        public override string Display()
        {
            if (_start != null && _end == null) return $"Newer than {_start:MMMM yyyy}";
            if (_start == null && _end != null) return $"Older than {_end:MMMM yyyy}";
            return $"Between {_start} and {_end}";
        }

        public override void Apply(ref List<LeaderboardScore> scores, LeaderboardData data)
        {
            var start = _start ?? new DateTime(2018, 5, 1);
            var end = _end ?? DateTime.Now;
            scores = scores.Where(s =>
            {
                if (_start != null && _end == null) return s.TimeSet > start && s.TimeSet < end;
                if (_start == null && _end != null) return s.TimeSet > start && s.TimeSet < end;
                return s.TimeSet > start && s.TimeSet < end;
            }).ToList();
        }
    }

    public class MissesFilter : DisplayBaseFilter
    {
        public override string Name => "Misses";

        [JsonProperty]
        private readonly int? _start;
        [JsonProperty]
        private readonly int? _end;

        public MissesFilter(int? start, int? end)
        {
            _start = start;
            _end = end;
        }

        public override string Display()
        {
            if (_start != null && _end == null) return $"More than {_start} misses";
            if (_start == null && _end != null) return $"Less than {_end} misses";
            return $"Between {_start} and {_end} misses";
        }

        public override void Apply(ref List<LeaderboardScore> scores, LeaderboardData data)
        {
            scores = scores.Where(s =>
            {
                if (_start != null && _end == null) return s.MissedNotes > _start;
                if (_start == null && _end != null) return s.MissedNotes < _end;
                return s.MissedNotes > _start && s.MissedNotes < _end;
            }).ToList();
        }
    }

    public class AccuracyFilter : DisplayBaseFilter
    {
        public override string Name => "Accuracy";

        [JsonProperty]
        private readonly float? _start;
        [JsonProperty]
        private readonly float? _end;

        public AccuracyFilter(float? start, float? end)
        {
            _start = start;
            _end = end;
        }

        public override string Display()
        {
            if (_start != null && _end == null) return $"More than {(float)_start:#.00}%";
            if (_start == null && _end != null) return $"Less than {(float)_end:#.00}%";
            return $"Between {(float)_start:#.00}% and {(float)_end:#.00}%";
        }

        public override void Apply(ref List<LeaderboardScore> scores, LeaderboardData data)
        {
            scores = scores.Where(s =>
            {
                var leaderboard = data.Leaderboards.First(l => l.LeaderboardId == s.LeaderboardId);
                var accuracy = (float) (100.0 * s.BaseScore / leaderboard.MaxScore);
                if (_start != null && _end == null) return accuracy > _start;
                if (_start == null && _end != null) return accuracy < _end;
                return accuracy > _start && accuracy < _end;
            }).ToList();
        }
    }
    
    public class PpFilter : DisplayBaseFilter
    {
        public override string Name => "PP";

        [JsonProperty]
        private readonly int? _start;
        [JsonProperty]
        private readonly int? _end;

        public PpFilter(int? start, int? end)
        {
            _start = start;
            _end = end;
        }

        public override string Display()
        {
            if (_start != null && _end == null) return $"More than {_start} PP";
            if (_start == null && _end != null) return $"Less than {_end} PP";
            return $"Between {_start} and {_end} PP";
        }

        public override void Apply(ref List<LeaderboardScore> scores, LeaderboardData data)
        {
            scores = scores.Where(s =>
            {
                if (_start != null && _end == null) return s.Pp > _start;
                if (_start == null && _end != null) return s.Pp < _end;
                return s.Pp > _start && s.Pp < _end;
            }).ToList();
        }
    }
}
