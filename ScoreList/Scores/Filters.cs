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
            if (Ranked == false || requiresJoin) 
                query += "\nJOIN leaderboards\nWHERE scores.LeaderboardId = leaderboards.LeaderboardId";

            if (Ranked == false) query += $"\nleaderboards.Ranked = {(Ranked ? 1 : 0)}";

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

        private readonly float? start;
        private readonly float? end;

        public StarsFilter(float? start, float? end)
        {
            this.start = start;
            this.end = end;
        }

        public override string Display()
        {
            if (start != null && end == null) return $"Bigger than {start} stars";
            if (start == null && end != null) return $"Smaller than {end} stars";
            return $"Between {start} and {end} stars";
        }

        public override string ToString()
        {
            if (start != null && end == null) return $"leaderboards.Stars > {start}";
            else if (start == null && end != null) return $"leaderboards.Stars < {end}";
            return $"leaderboards.Stars BETWEEN {start} AND {end}";
        }
    }

    public class DateFilter : Filter
    {
        public override string Name => "Date";
        public override bool RequiresJoin => false;

        private readonly string start;
        private readonly string end;

        public DateFilter(string start, string end)
        {
            this.start = start;
            this.end = end;
        }

        public override string Display()
        {
            if (start != null && end == null) return $"Newer than {start}";
            if (start == null && end != null) return $"Older than {end}";
            return $"Between {start} and {end}";
        }

        public override string ToString()
        {
            if (start != null && end == null) return $"scores.TimeSet > {start}";
            else if (start == null && end != null) return $"scores.TimeSet < {end}";
            return $"scores.TimeSet BETWEEN {start} AND {end}";
        }
    }

    public class MissesFilter : Filter
    {
        public override string Name => "Misses";
        public override bool RequiresJoin => false;

        private readonly int? start;
        private readonly int? end;

        public MissesFilter(int? start, int? end)
        {
            this.start = start;
            this.end = end;
        }

        public override string Display()
        {
            if (start != null && end == null) return $"More than {start} misses";
            if (start == null && end != null) return $"Smaller than {end} misses";
            return $"Between {start} and {end} misses";
        }

        public override string ToString()
        {
            if (start != null && end == null) return $"scores.MissedNotes > {start}";
            else if (start == null && end != null) return $"scores.MissedNotes < {end}";
            return $"scores.MissedNotes BETWEEN {start} AND {end}";
        }
    }

    public class AccuracyFilter : Filter
    {
        public override string Name => "Accuracy";
        public override bool RequiresJoin => true;

        private readonly float? start;
        private readonly float? end;

        public AccuracyFilter(float? start, float? end)
        {
            this.start = start;
            this.end = end;
        }

        public override string Display()
        {
            if (start != null && end == null) return $"Bigger than {(float)start:#.00}%";
            if (start == null && end != null) return $"Smaller than {(float)end:#.00}%";
            return $"Between {(float)start:#.00}% and {(float)end:#.00}%";
        }

        public override string ToString()
        {
            if (start != null && end == null) return $"(100.0 * scores.BaseScore / leaderboards.MaxScore) > {start}";
            if (start == null && end != null) return $"(100.0 * scores.BaseScore / leaderboards.MaxScore) < {end}";
            return $"(100.0 * scores.BaseScore / leaderboards.MaxScore) BETWEEN {start} AND {end}";
        }
    }

    public class PPFilter : Filter
    {
        public override string Name => "PP";
        public override bool RequiresJoin => true;

        private readonly int? start;
        private readonly int? end;

        public PPFilter(int? start, int? end)
        {
            this.start = start;
            this.end = end;
        }

        public override string Display()
        {
            if (start != null && end == null) return $"Bigger than {start} PP";
            if (start == null && end != null) return $"Smaller than {end} PP";
            return $"Between {start} and {end} PP";
        }

        public override string ToString()
        {
            if (start != null && end == null) return $"scores.PP > {start}";
            if (start == null && end != null) return $"scores.PP < {end}";
            return $"scores.PP BETWEEN {start} AND {end}";
        }
    }
}
