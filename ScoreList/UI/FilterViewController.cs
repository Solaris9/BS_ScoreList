using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Components;
using System.Collections.Generic;
using ScoreList.Scores;
using UnityEngine.UI;
using System.Linq;
using System;
using ScoreList.Utils;
using Zenject;

namespace ScoreList.UI
{
    public class FilterListCellWrapper
    {
        private readonly FilterViewController _controller;

        [UIValue("name")] private readonly string _name;

        [UIValue("data")] private readonly string _data;

        [UIAction("DeleteFilter")]
        internal void DeleteFilter()
        {
            var filter = _controller.Filters
                .Where(f => f is DisplayBaseFilter)
                .Cast<DisplayBaseFilter>()
                .ToList()
                .Find(f => f.Name == _name);
            
            _controller.Filters.Remove(filter);

            _controller.list.data.Remove(this);
            _controller.list.tableView.ReloadData();

            if (_controller.Filters.Count == 0) _controller.ToggleNoFiltersText(true);
        }

        public FilterListCellWrapper(FilterViewController controller, string name, string data) 
        {
            this._controller = controller;
            this._name = name;
            this._data = data;
        }
    }

    [HotReload(RelativePathToLayout = @"Views\ScoreFilters.bsml")]
    [ViewDefinition("ScoreList.UI.Views.ScoreFilters.bsml")]
    public class FilterViewController : BSMLAutomaticViewController
    {
        private readonly ScoreManager _scoresManager;

        [Inject]
        public FilterViewController(ScoreManager scoresManager)
        {
            _scoresManager = scoresManager;

            var baseFilters = new List<BaseFilter> { new SortPpFilter() };
            var score = _scoresManager.Query(baseFilters).GetAwaiter().GetResult();
            if (score.Count > 0) maxPp = (int)score.First().PP;
        }
        
        // components

        [UIComponent("stars-tab")] public Tab starsTab;
        [UIComponent("accuracy-tab")] public Tab accuracyTab;
        [UIComponent("pp-tab")] public Tab ppTab;
        
        [UIComponent("no-filters-text")] public LayoutElement noFiltersText;
        [UIComponent("list")] public CustomCellListTableData list;
        public readonly List<BaseFilter> Filters = new List<BaseFilter>();

        // sort components

        [UIComponent("sort")] public DropDownListSetting sort;
        [UIComponent("order")] public DropDownListSetting order;
        [UIComponent("ranked")] public DropDownListSetting ranked;

        // pp components

        [UIComponent("filter-pp-minimum")] public SliderSetting filterPpMinimum;
        [UIComponent("filter-pp-maximum")] public SliderSetting filterPpMaximum;

        // star components

        [UIComponent("filter-stars-minimum")] public SliderSetting filterStarsMinimum;
        [UIComponent("filter-stars-maximum")] public SliderSetting filterStarsMaximum;

        // date components

        [UIComponent("filter-date-after-month")] public SliderSetting filterDateAfterMonth;
        [UIComponent("filter-date-after-year")] public SliderSetting filterDateAfterYear;

        [UIComponent("filter-date-before-month")] public SliderSetting filterDateBeforeMonth;
        [UIComponent("filter-date-before-year")] public SliderSetting filterDateBeforeYear;

        // misses components

        [UIComponent("filter-misses-minimum")] public SliderSetting filterMissesMinimum;
        [UIComponent("filter-misses-maximum")] public SliderSetting filterMissesMaximum;

        // accuracy components

        [UIComponent("filter-accuracy-minimum")] public SliderSetting filterAccuracyMinimum;
        [UIComponent("filter-accuracy-maximum")] public SliderSetting filterAccuracyMaximum;

        public void ToggleNoFiltersText(bool value) => noFiltersText.gameObject.SetActive(value);

        // main functions

        [UIAction("ApplyFilters")]
        internal void ApplyFilters()
        {
            Filters.AddRange(new[]{
                new OrderFilter((string) order.Value),
                new RankedFilter((string) ranked.Value == "Ranked"),
                GetFilter((string) sort.Value)
            });

            ScoreListCoordinator.Instance.ShowFilteredScores(Filters);
        }

        private static BaseFilter GetFilter(string sortBy)
        {
            switch (sortBy)
            {
                case "Rank": return new SortRankFilter();
                case "TimeSet": return new SortTimeSetFilter();
                case "MissedNotes": return new SortMissedNotesFilter();
                case "PP": return new SortPpFilter();
                case "Stars": return new SortStarsFilter();
                case "Accuracy": return new SortAccuracyFilter();
            }

            return null;
        }
        
        [UIAction("FilterSelect")]
        internal void FilterSelect(object _, int index) => filterChoice = (string)filterChoices[index];

        [UIAction("TypeChanged")]
        internal void TypeChanged(string type)
        {
            starsTab.IsVisible = type == "Ranked";
            accuracyTab.IsVisible = type == "Ranked";
            ppTab.IsVisible = type == "Ranked";

            
            
            if (type == "Ranked") sortChoices.AddRange(new[] { "PP", "Stars", "Accuracy" });
            else sortChoices.RemoveRange(new object[] { "PP", "Stars", "Accuracy" });

            sort.values = sortChoices;
            sort.UpdateChoices();

            // clear filters, TODO: figure out a better way to clear opposite filters
            Filters.Clear();
            list.data.Clear();
            list.tableView.ReloadData();
        }

        [UIAction("ResetFilters")]
        internal void ResetFilters() {
            Filters.Clear();
            list.data.Clear();
            list.tableView.ReloadData();
            
            if ((string)ranked.Value == "Ranked") sortChoices.AddRange(new[] { "PP", "Stars", "Accuracy" });
            else sortChoices.RemoveRange(new object[] { "PP", "Stars", "Accuracy" });

            sort.Value = "PP";
            order.Value = "DESC";
            ranked.Value = "Ranked";
            sort.values = sortChoices;
            sort.UpdateChoices();

            ToggleNoFiltersText(true);
        }

        private DisplayBaseFilter TryCreateFilter() {
            // messy
            switch (filterChoice) {
                case "Stars":
                    float? starsMaximum = null;
                    if (filterStarsMaximum.Value != 14f) starsMaximum = filterStarsMaximum.Value;

                    float? starsMinimum = null;
                    if (filterStarsMinimum.Value != 0f) starsMinimum = filterStarsMinimum.Value;

                    if (starsMaximum != null || starsMinimum != null)
                        return new StarsFilter(starsMinimum, starsMaximum);
                    break;

                case "PP":
                    int? ppMaximum = null;
                    if ((int)filterPpMaximum.Value != maxPp) ppMaximum = (int)filterPpMaximum.Value;

                    int? ppMinimum = null;
                    if ((int)filterPpMinimum.Value != 0) ppMinimum = (int)filterPpMinimum.Value;

                    if (ppMaximum != null || ppMinimum != null)
                        return new PpFilter(ppMinimum, ppMaximum);
                    break;

                case "Date":
                    int? dateAfterMonth = null;
                    int? dateAfterYear = null;

                    if ((int)filterDateAfterMonth.Value != 1) dateAfterMonth = (int)filterDateAfterMonth.Value;
                    if ((int)filterDateAfterYear.Value != 2018) dateAfterYear = (int)filterDateAfterYear.Value;

                    int? dateBeforeMonth = null;
                    int? dateBeforeYear = null;

                    if ((int)filterDateBeforeMonth.Value != 12) dateBeforeMonth = (int)filterDateBeforeMonth.Value;
                    if ((int)filterDateBeforeYear.Value != maxYear) dateBeforeYear = (int)filterDateBeforeYear.Value;

                    string after = null;
                    if (dateAfterMonth != null || dateAfterYear != null)
                        after = $"{dateAfterYear ?? 2018}-{dateAfterMonth ?? 1}-1";

                    string before = null;
                    if (dateBeforeYear != null || dateBeforeMonth != null)
                        before = $"{dateBeforeYear ?? 2018}-{dateBeforeMonth ?? 1}-1";

                    if (after != null || before != null)
                        return new DateFilter(after, before);
                    break;

                case "Misses":
                    int? missesMaximum = null;
                    if ((int)filterMissesMaximum.Value != 100) missesMaximum = (int)filterMissesMaximum.Value;

                    int? missesMinimum = null;
                    if (filterMissesMinimum.Value != 0) missesMinimum = (int)filterMissesMinimum.Value;

                    if (missesMaximum != null || missesMinimum != null)
                        return new MissesFilter(missesMinimum, missesMaximum);
                    break;

                case "Accuracy":
                    float? accuracyMinimum = null;
                    if (filterAccuracyMinimum.Value != 0f) accuracyMinimum = filterAccuracyMinimum.Value;

                    float? accuracyMaximum = null;
                    if (filterAccuracyMaximum.Value != 100f) accuracyMaximum = filterAccuracyMaximum.Value;

                    if (accuracyMaximum != null || accuracyMinimum != null)
                        return new AccuracyFilter(accuracyMinimum, accuracyMaximum);
                    break;
            }

            return null;
        }

        [UIAction("CreateFilter")]
        internal void CreateFilter()
        {
            var displayFilters = Filters.Where(f => f is DisplayBaseFilter).Cast<DisplayBaseFilter>();
            if (displayFilters.Any(f => f.Name == filterChoice)) return;

            var filter = TryCreateFilter();
            if (filter == null) return;

            var wrapper = new FilterListCellWrapper(this, filterChoice, filter.Display());

            Filters.Add(filter);
            list.data.Add(wrapper);
            list.tableView.ReloadData();

            ToggleNoFiltersText(false);
        }

        // choice

        [UIValue("filter-choice")] public string filterChoice = "Stars";

        // choices

        [UIValue("order-choices")]
        public List<object> orderChoices => new List<object> { "DESC", "ASC" };

        [UIValue("ranked-choices")]
        public List<object> rankedChoices => new List<object> { "Ranked", "Unranked" };

        [UIValue("sort-choices")]
        public List<object> sortChoices => new List<object> {
            "Rank",
            "TimeSet",
            "MissedNotes",
            "PP",
            "Stars",
            "Accuracy",
        };

        [UIValue("filter-choices")]
        public List<object> filterChoices => new List<object> {
            "Stars",
            "PP",
            "Accuracy",
            "Misses",
            "Date"
        };

        // formatters

        public List<string> monthChoices => new List<string> {
            "January ", "February", "March",
            "April",    "May",      "June",
            "July",     "August",   "September",
            "October",  "November", "December"
        };

        [UIAction("FormatMonth")]
        internal string FormatMonth(int index) => monthChoices[index - 1];

        // utils

        [UIValue("max-year")] public int maxYear = DateTime.Today.Year;
        [UIValue("max-month")] public int maxMonth = 12;
        [UIValue("max-stars")] public float maxStars = 14f;
        [UIValue("max-accuracy")] public int maxAccuracy = 100;
        [UIValue("max-misses")] public int maxMisses = 100;
        [UIValue("max-pp")] public int maxPp;
    }
}
