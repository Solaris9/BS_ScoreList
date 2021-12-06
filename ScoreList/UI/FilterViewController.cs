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

namespace ScoreList.UI {
    public class FilterListCellWrapper
    {
        private readonly FilterViewController controller;

        [UIValue("name")] private readonly string name;

        [UIValue("data")] private readonly string data;

        [UIAction("DeleteFilter")]
        internal void DeleteFilter()
        {
            var filter = controller.filters.Find(f => f.Name == name);
            controller.filters.Remove(filter);

            controller.list.data.Remove(this);
            controller.list.tableView.ReloadData();

            if (controller.filters.Count == 0) controller.ToggleNoFiltersText(true);
        }

        public FilterListCellWrapper(FilterViewController controller, string name, string data) 
        {
            this.controller = controller;
            this.name = name;
            this.data = data;
        }
    }

    [HotReload(RelativePathToLayout = @"Views\ScoreFilters.bsml")]
    [ViewDefinition("ScoreList.UI.Views.ScoreFilters.bsml")]
    public class FilterViewController : BSMLAutomaticViewController
    {
        internal FilterViewController()
        {
            var query = "SELECT * FROM scores ORDER BY scores.PP DESC LIMIT 1";
            var score = DatabaseManager.Client.Query<LeaderboardScore>(query).GetAwaiter().GetResult();
            maxPP = (int)score.First().PP;
        }
        
        // components

        [UIComponent("stars-tab")] public Tab starsTab;
        [UIComponent("accuracy-tab")] public Tab accuracyTab;
        [UIComponent("pp-tab")] public Tab ppTab;
        
        [UIComponent("no-filters-text")] public LayoutElement noFiltersText;
        [UIComponent("list")] public CustomCellListTableData list;
        public readonly List<Filter> filters = new List<Filter>();

        // sort components

        [UIComponent("sort")] public DropDownListSetting sort;
        [UIComponent("order")] public DropDownListSetting order;
        [UIComponent("ranked")] public DropDownListSetting ranked;

        // pp components

        [UIComponent("filter-pp-minimum")] public SliderSetting filterPPMinimum;
        [UIComponent("filter-pp-maximum")] public SliderSetting filterPPMaximum;

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
            var query = new SearchQuery
            {
                Order = (string)order.Value,
                SortBy = (string)sort.Value,
                Filters = filters,
                Ranked = (string)ranked.Value == "Ranked"
            };
            
            ScoreListCoordinator.Instance.ShowFilteredScores(query);
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
            filters.Clear();
            list.data.Clear();
            list.tableView.ReloadData();
        }

        [UIAction("ResetFilters")]
        internal void ResetFilters() {
            filters.Clear();
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

        private Filter TryCreateFilter() {
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
                    if ((int)filterPPMaximum.Value != maxPP) ppMaximum = (int)filterPPMaximum.Value;

                    int? ppMinimum = null;
                    if ((int)filterPPMinimum.Value != 0) ppMinimum = (int)filterPPMinimum.Value;

                    if (ppMaximum != null || ppMinimum != null)
                        return new PPFilter(ppMinimum, ppMaximum);
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
            if (filters.Any(f => f.Name == filterChoice)) return;

            var filter = TryCreateFilter();
            if (filter == null) return;

            var wrapper = new FilterListCellWrapper(this, filterChoice, filter.Display());

            filters.Add(filter);
            list.data.Add(wrapper);
            list.tableView.ReloadData();

            ToggleNoFiltersText(false);
        }

        // choice

        [UIValue("filter-choice")] public string filterChoice = "Stars";

        // choices

        [UIValue("order-choices")]
        public List<object> orderChoices = new List<object> { "DESC", "ASC" };

        [UIValue("ranked-choices")]
        public List<object> rankedChoices => new List<object> { "Ranked", "Unranked" };

        [UIValue("sort-choices")]
        public List<object> sortChoices = new List<object> {
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

        public List<string> monthChoices = new List<string> {
            "January ", "February", "March",
            "April",    "May",      "June",
            "July",     "August",   "September",
            "October",  "November", "December"
        };

        // formatters

        [UIAction("FormatMonth")]
        internal string FormatMonth(int index) => monthChoices[index - 1];

        // utils

        [UIValue("max-year")] public int maxYear = DateTime.Today.Year;
        [UIValue("max-month")] public int maxMonth = 12;
        [UIValue("max-stars")] public float maxStars = 14f;
        [UIValue("max-accuracy")] public int maxAccuracy = 100;
        [UIValue("max-misses")] public int maxMisses = 100;
        [UIValue("max-pp")] public int maxPP;
    }
}
