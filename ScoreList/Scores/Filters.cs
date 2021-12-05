using System.Linq;
using System.Collections.Generic;

namespace ScoreList.Scores {
    public class SearchQuery
    {
        public string SortBy = "TimeSet";
        public string Order = "DESC";
        public List<Filter> Filters = new List<Filter>();

        public static Dictionary<string, string> Sorting = new Dictionary<string, string> {
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

            var requiresJoin = Filters.Any(f => f.RequiresJoin) || new string[] { "Stars", "Accuracy" }.Contains(SortBy);
            if (requiresJoin) query += "\nJOIN leaderboards\nWHERE scores.LeaderboardId = leaderboards.LeaderboardId";

            if (Filters.Count > 0)
            {
                if (!requiresJoin) query += "\nWHERE ";
                else query += "\nAND ";

                query += string.Join("\nAND ", Filters.Select(f => f.ToString()));
            }

            if (requiresJoin) query += "\nGROUP BY scores.ScoreId";
            if (SortBy != null) query += $"\nORDER BY {Sorting[SortBy]} {Order}";

            return query;
        }
    }

    public abstract class Filter
    {
        public abstract bool RequiresJoin { get; }
        public abstract string Name { get; }

        public abstract (object, object) GetValues();
    }

    public class StarsFilter : Filter
    {
        public override string Name => "Stars";
        public override bool RequiresJoin => true;

        private float? start;
        private float? end;

        public StarsFilter(float? start, float? end)
        {
            this.start = start;
            this.end = end;
        }

        public override (object, object) GetValues() => (start, end);

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

        private string start;
        private string end;

        public DateFilter(string start, string end)
        {
            this.start = start;
            this.end = end;
        }

        public override (object, object) GetValues() => (start, end);

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

        private int? start;
        private int? end;

        public MissesFilter(int? start, int? end)
        {
            this.start = start;
            this.end = end;
        }

        public override (object, object) GetValues() => (start, end);

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

        private float? start;
        private float? end;

        public AccuracyFilter(float? start, float? end)
        {
            this.start = start;
            this.end = end;
        }

        public override (object, object) GetValues() => (start, end);

        public override string ToString()
        {
            if (start != null && end == null) return $"(100.0 * scores.BaseScore / leaderboards.MaxScore) > {start}";
            else if (start == null && end != null) return $"(100.0 * scores.BaseScore / leaderboards.MaxScore) < {end}";
            return $"(100.0 * scores.BaseScore / leaderboards.MaxScore) BETWEEN {start} AND {end}";
        }
    }
}
