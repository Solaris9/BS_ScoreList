using System.Linq;
using System.Collections.Generic;

namespace ScoreList.Scores {
    public class SearchQuery
    {
        public string SortBy = "TimeSet";
        public string Order = "DESC";
        public bool Ranked = true;
        public List<Filter> Filters = new List<Filter>();

        private static readonly Dictionary<string, string> Sorting = new Dictionary<string, string> {
                { "Stars", "leaderboards.Stars" },
                { "Rank", "scores.Rank" },
                { "PP", "scores.PP" },
                { "TimeSet", "scores.TimeSet" },
                { "MissedNotes", "scores.MissedNotes" },
                { "Accuracy", "(100.0 * scores.BaseScore / leaderboards.MaxScore)"}
            };

        public override string ToString()
        {
            var query = "SELECT * FROM scores";

            var requiresJoin = Filters.Any(f => f.RequiresJoin) || new[] {"Stars", "Accuracy"}.Contains(SortBy);
            if (Ranked || requiresJoin) 
                query += "\nJOIN leaderboards\nWHERE scores.LeaderboardId = leaderboards.LeaderboardId";

            if (Ranked) query += $"\nleaderboards.Ranked = {(Ranked ? 1 : 0)}";

            if (Filters.Count > 0)
            {
                if (!requiresJoin) query += "\nWHERE ";
                else query += "\nAND ";

                query += string.Join("\nAND ", Filters.Select(f => f.ToString()));
            }

            if (requiresJoin) query += "\nGROUP BY scores.ScoreId";
            return query + $"\nORDER BY {Sorting[SortBy]} {Order}";
        }
    }

    public abstract class Filter
    {
        public abstract bool RequiresJoin { get; }
        public abstract string Name { get; }

        public abstract string Display();
    }

    public class StarsFilter : Filter
    {
        public override string Name => "Stars";
        public override bool RequiresJoin => true;

        private readonly float? _start;
        private readonly float? _end;

        public StarsFilter(float? start, float? end)
        {
            this._start = start;
            this._end = end;
        }

        public override string Display()
        {
            if (_start != null && _end == null) return $"Bigger than {_start} stars";
            if (_start == null && _end != null) return $"Smaller than {_end} stars";
            return $"Between {_start} and {_end} stars";
        }

        public override string ToString()
        {
            if (_start != null && _end == null) return $"leaderboards.Stars > {_start}";
            else if (_start == null && _end != null) return $"leaderboards.Stars < {_end}";
            return $"leaderboards.Stars BETWEEN {_start} AND {_end}";
        }
    }

    public class DateFilter : Filter
    {
        public override string Name => "Date";
        public override bool RequiresJoin => false;

        private readonly string _start;
        private readonly string _end;

        public DateFilter(string start, string end)
        {
            this._start = start;
            this._end = end;
        }

        public override string Display()
        {
            if (_start != null && _end == null) return $"Newer than {_start}";
            if (_start == null && _end != null) return $"Older than {_end}";
            return $"Between {_start} and {_end}";
        }

        public override string ToString()
        {
            if (_start != null && _end == null) return $"scores.TimeSet > {_start}";
            else if (_start == null && _end != null) return $"scores.TimeSet < {_end}";
            return $"scores.TimeSet BETWEEN {_start} AND {_end}";
        }
    }

    public class MissesFilter : Filter
    {
        public override string Name => "Misses";
        public override bool RequiresJoin => false;

        private readonly int? _start;
        private readonly int? _end;

        public MissesFilter(int? start, int? end)
        {
            this._start = start;
            this._end = end;
        }

        public override string Display()
        {
            if (_start != null && _end == null) return $"More than {_start} misses";
            if (_start == null && _end != null) return $"Smaller than {_end} misses";
            return $"Between {_start} and {_end} misses";
        }

        public override string ToString()
        {
            if (_start != null && _end == null) return $"scores.MissedNotes > {_start}";
            else if (_start == null && _end != null) return $"scores.MissedNotes < {_end}";
            return $"scores.MissedNotes BETWEEN {_start} AND {_end}";
        }
    }

    public class AccuracyFilter : Filter
    {
        public override string Name => "Accuracy";
        public override bool RequiresJoin => true;

        private readonly float? _start;
        private readonly float? _end;

        public AccuracyFilter(float? start, float? end)
        {
            this._start = start;
            this._end = end;
        }

        public override string Display()
        {
            if (_start != null && _end == null) return $"Bigger than {(float)_start:#.00}%";
            if (_start == null && _end != null) return $"Smaller than {(float)_end:#.00}%";
            return $"Between {(float)_start:#.00}% and {(float)_end:#.00}%";
        }

        public override string ToString()
        {
            if (_start != null && _end == null) return $"(100.0 * scores.BaseScore / leaderboards.MaxScore) > {_start}";
            if (_start == null && _end != null) return $"(100.0 * scores.BaseScore / leaderboards.MaxScore) < {_end}";
            return $"(100.0 * scores.BaseScore / leaderboards.MaxScore) BETWEEN {_start} AND {_end}";
        }
    }

    public class PPFilter : Filter
    {
        public override string Name => "PP";
        public override bool RequiresJoin => true;

        private readonly int? _start;
        private readonly int? _end;

        public PPFilter(int? start, int? end)
        {
            this._start = start;
            this._end = end;
        }

        public override string Display()
        {
            if (_start != null && _end == null) return $"Bigger than {_start} PP";
            if (_start == null && _end != null) return $"Smaller than {_end} PP";
            return $"Between {_start} and {_end} PP";
        }

        public override string ToString()
        {
            if (_start != null && _end == null) return $"scores.PP > {_start}";
            if (_start == null && _end != null) return $"scores.PP < {_end}";
            return $"scores.PP BETWEEN {_start} AND {_end}";
        }
    }
}
