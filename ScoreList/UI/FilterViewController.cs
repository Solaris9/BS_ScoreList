using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Components;
using System.Collections.Generic;
using ScoreList.Scores;
using UnityEngine.UI;
using System.Linq;
using System;
using Zenject;

#pragma warning disable CS0649
#pragma warning disable CS0414

namespace ScoreList.UI
{
    public class FilterListCellWrapper
    {
        readonly FilterViewController _controller;

        [UIValue("name")] readonly string _name;

        [UIValue("data")] readonly string _data;

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
            _controller = controller;
            _name = name;
            _data = data;
        }
    }

    [HotReload(RelativePathToLayout = @"Views\ScoreFilters.bsml")]
    [ViewDefinition("ScoreList.UI.Views.ScoreFilters.bsml")]
    public class FilterViewController : BSMLAutomaticViewController
    {
        [Inject] readonly ScoreManager _scoresManager;

        [UIAction("#post-parse")]
        void SetupUI()
        {
            var data = _scoresManager.Read().GetAwaiter().GetResult();
            var scores = data.Scores;

            if (scores.Count > 0)
            {
                new RankedFilter(true).Apply(ref scores, data);
                // max pp
                
                new SortPpFilter().Apply(ref scores, null);
                _maxPp = (int)scores.First().Pp;
                
                // max stars

                new SortStarsFilter().Apply(ref scores, data);
                var id = scores.First().LeaderboardId;
                var info = _scoresManager.GetLeaderboard(id).GetAwaiter().GetResult();
                _maxStars = (float) info.Stars;

                _scoresManager.Clean();
            }
        }
        
        // components

        [UIComponent("stars-tab")] readonly Tab _starsTab;
        [UIComponent("accuracy-tab")] readonly Tab _accuracyTab;
        [UIComponent("pp-tab")] readonly Tab _ppTab;
        
        [UIComponent("no-filters-text")] readonly LayoutElement _noFiltersText;
        [UIComponent("list")] internal readonly CustomCellListTableData list;
        internal readonly List<BaseFilter> Filters = new List<BaseFilter>();

        // sort components

        [UIComponent("sort")] readonly DropDownListSetting _sort;
        [UIComponent("order")] readonly DropDownListSetting _order;
        [UIComponent("ranked")] readonly DropDownListSetting _ranked;

        // pp components

        [UIComponent("filter-pp-minimum")] readonly SliderSetting _filterPpMinimum;
        [UIComponent("filter-pp-maximum")] readonly SliderSetting _filterPpMaximum;

        // star components

        [UIComponent("filter-stars-minimum")] readonly SliderSetting _filterStarsMinimum;
        [UIComponent("filter-stars-maximum")] readonly SliderSetting _filterStarsMaximum;

        // date components

        [UIComponent("filter-date-after-month")] readonly SliderSetting _filterDateAfterMonth;
        [UIComponent("filter-date-after-year")] readonly SliderSetting _filterDateAfterYear;

        [UIComponent("filter-date-before-month")] readonly SliderSetting _filterDateBeforeMonth;
        [UIComponent("filter-date-before-year")] readonly SliderSetting _filterDateBeforeYear;

        // misses components

        [UIComponent("filter-misses-minimum")] readonly SliderSetting _filterMissesMinimum;
        [UIComponent("filter-misses-maximum")] readonly SliderSetting _filterMissesMaximum;

        // accuracy components

        [UIComponent("filter-accuracy-minimum")] readonly SliderSetting _filterAccuracyMinimum;
        [UIComponent("filter-accuracy-maximum")] readonly SliderSetting _filterAccuracyMaximum;

        internal void ToggleNoFiltersText(bool value) => _noFiltersText.gameObject.SetActive(value);

        // main functions

        [UIAction("ApplyFilters")]
        void ApplyFilters()
        {
            Filters.AddRange(new[]{
                new RankedFilter((string) _ranked.Value == "Ranked"),
                GetFilter((string) _sort.Value),
                new OrderFilter((string) _order.Value),
            });

            ScoreListCoordinator.Instance.ShowFilteredScores(Filters);
        }

        static BaseFilter GetFilter(string sortBy)
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
        
        // ReSharper disable once UnusedParameter.Local
        [UIAction("FilterSelect")]
        void FilterSelect(object _, int index) => _filterChoice = (string)FilterChoices[index];

        [UIAction("TypeChanged")]
        void TypeChanged(string type)
        {
            _starsTab.IsVisible = type == "Ranked";
            _accuracyTab.IsVisible = type == "Ranked";
            _ppTab.IsVisible = type == "Ranked";

            _sort.values = type == "Ranked" ?  SortChoices : new List<object> { "Rank", "TimeSet", "MissedNotes" };
            _sort.UpdateChoices();

            // clear filters, TODO: figure out a better way to clear opposite filters
            Filters.Clear();
            list.data.Clear();
            list.tableView.ReloadData();
        }

        [UIAction("ResetFilters")]
        void ResetFilters()
        {
            Filters.Clear();
            list.data.Clear();
            list.tableView.ReloadData();
            
            _sort.Value = "PP";
            _order.Value = "DESC";
            _ranked.Value = "Ranked";
            _sort.values = SortChoices;
            _sort.UpdateChoices();

            ToggleNoFiltersText(true);
        }

        DisplayBaseFilter TryCreateFilter()
        {
            switch (_filterChoice)
            {
                case "Stars":
                    float? starsMaximum = null;
                    if (_filterStarsMaximum.Value != _maxStars) starsMaximum = _filterStarsMaximum.Value;

                    float? starsMinimum = null;
                    if (_filterStarsMinimum.Value != 0f) starsMinimum = _filterStarsMinimum.Value;

                    if (starsMaximum != null || starsMinimum != null)
                        return new StarsFilter(starsMinimum, starsMaximum);
                    break;

                case "PP":
                    int? ppMaximum = null;
                    if ((int)_filterPpMaximum.Value != _maxPp) ppMaximum = (int)_filterPpMaximum.Value;

                    int? ppMinimum = null;
                    if ((int)_filterPpMinimum.Value != 0) ppMinimum = (int)_filterPpMinimum.Value;

                    if (ppMaximum != null || ppMinimum != null)
                        return new PpFilter(ppMinimum, ppMaximum);
                    break;

                case "Date":
                    int? dateAfterMonth = null;
                    int? dateAfterYear = null;

                    if ((int)_filterDateAfterMonth.Value != 1) dateAfterMonth = (int)_filterDateAfterMonth.Value;
                    if ((int)_filterDateAfterYear.Value != 2018) dateAfterYear = (int)_filterDateAfterYear.Value;

                    int? dateBeforeMonth = null;
                    int? dateBeforeYear = null;

                    if ((int)_filterDateBeforeMonth.Value != 12) dateBeforeMonth = (int)_filterDateBeforeMonth.Value;
                    if ((int)_filterDateBeforeYear.Value != _maxYear) dateBeforeYear = (int)_filterDateBeforeYear.Value;

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
                    if ((int)_filterMissesMaximum.Value != 100) missesMaximum = (int)_filterMissesMaximum.Value;

                    int? missesMinimum = null;
                    if (_filterMissesMinimum.Value != 0) missesMinimum = (int)_filterMissesMinimum.Value;

                    if (missesMaximum != null || missesMinimum != null)
                        return new MissesFilter(missesMinimum, missesMaximum);
                    break;

                case "Accuracy":
                    float? accuracyMinimum = null;
                    if (_filterAccuracyMinimum.Value != 0f) accuracyMinimum = _filterAccuracyMinimum.Value;

                    float? accuracyMaximum = null;
                    if (_filterAccuracyMaximum.Value != 100f) accuracyMaximum = _filterAccuracyMaximum.Value;

                    if (accuracyMaximum != null || accuracyMinimum != null)
                        return new AccuracyFilter(accuracyMinimum, accuracyMaximum);
                    break;
            }

            return null;
        }

        [UIAction("CreateFilter")]
        void CreateFilter()
        {
            var displayFilters = Filters.Where(f => f is DisplayBaseFilter).Cast<DisplayBaseFilter>();
            if (displayFilters.Any(f => f.Name == _filterChoice)) return;

            var filter = TryCreateFilter();
            if (filter == null) return;

            var wrapper = new FilterListCellWrapper(this, _filterChoice, filter.Display());

            Filters.Add(filter);
            list.data.Add(wrapper);
            list.tableView.ReloadData();

            ToggleNoFiltersText(false);
        }

        // choice

        [UIValue("filter-choice")] string _filterChoice = "Stars";

        // choices

        [UIValue("order-choices")]
        List<object> OrderChoices => new List<object> { "DESC", "ASC" };

        [UIValue("ranked-choices")]
        List<object> RankedChoices => new List<object> { "Ranked", "Unranked" };

        [UIValue("sort-choices")]
        List<object> SortChoices => new List<object> {
            "Rank",
            "TimeSet",
            "MissedNotes",
            "PP",
            "Stars",
            "Accuracy"
        };

        [UIValue("filter-choices")]
        List<object> FilterChoices => new List<object> {
            "Stars",
            "PP",
            "Accuracy",
            "Misses",
            "Date"
        };

        // formatters

        List<string> MonthChoices => new List<string> {
            "January ", "February", "March",
            "April",    "May",      "June",
            "July",     "August",   "September",
            "October",  "November", "December"
        };

        [UIAction("FormatMonth")]
        string FormatMonth(int index) => MonthChoices[index - 1];

        // utils

        [UIValue("max-year")] readonly int _maxYear = DateTime.Today.Year;
        [UIValue("max-month")] readonly int _maxMonth = 12;
        [UIValue("max-accuracy")] readonly int _maxAccuracy = 100;
        [UIValue("max-misses")] readonly int _maxMisses = 100;
        [UIValue("max-stars")] float _maxStars;
        [UIValue("max-pp")] int _maxPp;
    }
}
