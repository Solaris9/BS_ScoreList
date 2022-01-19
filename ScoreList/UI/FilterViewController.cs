using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Components;
using System.Collections.Generic;
using ScoreList.Scores;
using UnityEngine.UI;
using System.Linq;
using System;
using IPA.Utilities;
using Newtonsoft.Json;
using ScoreList.Configuration;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

#pragma warning disable CS0649
#pragma warning disable CS0414

namespace ScoreList.UI
{
    public class FilterListCellWrapper
    {
        readonly FilterViewController _controller;
        internal readonly DisplayBaseFilter _filter;
        [UIValue("data")] readonly string _data;

        [UIAction("DeleteFilter")]
        internal void DeleteFilter()
        {
            _controller.Filters.Filters.Remove(_filter);
            _controller._filtersList.data.Remove(this);
            _controller._filtersList.tableView.ReloadData();

            if (_controller.Filters.Filters.Count == 0) _controller.ToggleNoFiltersText(true);
            
            _controller._scoreView.FilterScores(_controller.Filters.Values());
        }

        public FilterListCellWrapper(FilterViewController controller, DisplayBaseFilter filter) 
        {
            _controller = controller;
            _filter = filter; 
            _data = filter.Display();
        }
    }
    
    public class PresetListCellWrapper
    {
        private static List<Type> _filterTypes = new List<Type>
            {
                typeof(OrderFilter),
                typeof(SortAccuracyFilter),
                typeof(SortMissedNotesFilter),
                typeof(SortTimeSetFilter),
                typeof(SortPpFilter),
                typeof(SortRankFilter),
                typeof(SortStarsFilter),
                typeof(RankedFilter),
                typeof(DownloadedFilter),
                typeof(StarsFilter),
                typeof(DateFilter),
                typeof(MissesFilter),
                typeof(AccuracyFilter),
                typeof(PpFilter),
            };
        
        readonly FilterViewController _controller;
        readonly PluginConfig _config;
        
        [UIValue("name")] readonly string _name;

        [UIAction("DeletePreset")]
        internal void DeletePreset()
        {
            var preset = _config.Presets.Find(p => p.Name == _name);
            
            _config.Presets.Remove(preset);
            _controller._presetsList.data.Remove(this);
            _controller._presetsList.tableView.ReloadData();

            if (_config.Presets.Count == 0) _controller.ToggleNoPresetsText(true);
        }

        [UIAction("ApplyPreset")]
        internal void ApplyPreset()
        {
            var values = _config.Presets.Find(p => p.Name == _name).Filters;
            var filters = new List<BaseFilter>();

            foreach (var key in values.Keys)
            {
                var filter = _filterTypes.Find(f => f.Name == key);
                var json = JsonConvert.SerializeObject(values[key]);
                filters.Add(JsonConvert.DeserializeObject(json, filter) as BaseFilter);
            }

            _controller._scoreView.FilterScores(filters);
        }

        public PresetListCellWrapper(FilterViewController controller, string name, PluginConfig config) 
        {
            _controller = controller;
            _config = config;
            _name = name;
        }
    }

    [HotReload(RelativePathToLayout = @"Views\ScoreFilters.bsml")]
    [ViewDefinition("ScoreList.UI.Views.ScoreFilters.bsml")]
    public class FilterViewController : BSMLAutomaticViewController
    {
        internal class FiltersObject
        {
            public BaseFilter Order;
            public BaseFilter Sort;
            public readonly List<BaseFilter> Filters = new List<BaseFilter>();

            public List<BaseFilter> Values()
            {
                var list = new List<BaseFilter>();

                list.AddRange(Filters);
                list.Add(Sort);
                list.Add(Order);

                return list;
            }

            public void Clear()
            {
                Sort = new SortPpFilter();
                Order = new OrderFilter(true);
                
                Filters.Clear();
                Filters.Add(new RankedFilter(true));
                // Filters.Add(new DownloadedFilter(true));
            }
        }
        
        [Inject] internal readonly ScoreViewController _scoreView;
        [Inject] readonly ScoreManager _scoresManager;
        [Inject] readonly PluginConfig _config;
        [Inject] readonly SiraLog _siraLog;

        internal readonly FiltersObject Filters = new FiltersObject();
        
        // alter components

        private void AlterToggle(ToggleSetting toggle)
        {
            var toggleTransform = toggle.toggle.transform;
            var originalTransform = toggleTransform.position;
            toggleTransform.position = new Vector3(-1.78f, originalTransform.y, originalTransform.z);
        }

        [UIAction("#post-parse")]
        async void SetupUI()
        {
            AlterToggle(_downloadToggle);
            AlterToggle(_rankedToggle);

            var count = await _scoresManager.TotalRanked();

            if (count > 0)
            {
                // max pp

                var ppFilters = new List<BaseFilter> {new RankedFilter(true), new SortPpFilter(), new OrderFilter(true)};
                var ppScores =  await _scoresManager.Query(ppFilters);
                var pp = (float) Math.Ceiling(ppScores.First().Pp);
                
                _filterPpMinimum.slider.maxValue = pp;
                _filterPpMaximum.slider.maxValue = pp;
                _filterPpMaximum.slider.value = pp;
                
                _scoresManager.Clean();

                // max stars

                var starsFilters = new List<BaseFilter> {new RankedFilter(true), new SortStarsFilter(), new OrderFilter(true)};
                var starsScores = await _scoresManager.Query(starsFilters);

                var id = starsScores.First().LeaderboardId;
                var info = await _scoresManager.GetLeaderboard(id);
                var stars = (float) Math.Ceiling(info.Stars);
                
                _filterStarsMinimum.slider.maxValue = stars;
                _filterStarsMaximum.slider.maxValue = stars;
                _filterStarsMaximum.slider.value = stars;
                
                _scoresManager.Clean();
            }

            foreach (var preset in _config.Presets)
            {
                var wrapper = new PresetListCellWrapper(this, preset.Name, _config);
                _presetsList.data.Add(wrapper);
            }
            
            _presetsList.tableView.ReloadData();
        }
        
        // components
        
        [UIComponent("stars-tab")] readonly Tab _starsTab;
        [UIComponent("accuracy-tab")] readonly Tab _accuracyTab;
        [UIComponent("pp-tab")] readonly Tab _ppTab;
        
        [UIComponent("no-filters-text")] readonly LayoutElement _noFiltersText;
        [UIComponent("no-presets-text")] readonly LayoutElement _noPresetsText;
        
        [UIComponent("filters-list")] internal readonly CustomCellListTableData _filtersList;
        [UIComponent("presets-list")] internal readonly CustomCellListTableData _presetsList;

        // sort & order components

        [UIComponent("sort")] readonly DropDownListSetting _sort;
        [UIComponent("order")] readonly DropDownListSetting _order;
        [UIComponent("download-toggle")] readonly ToggleSetting _downloadToggle;
        [UIComponent("ranked-toggle")] readonly ToggleSetting _rankedToggle;

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
        internal void ToggleNoPresetsText(bool value) => _noPresetsText.gameObject.SetActive(value);

        // main  functions

        [UIAction("CreatePreset")]
        internal void CreatePreset()
        {
            var presetName = "preset" + _config.Presets.Count;
            var presetValues = Filters.Values().ToDictionary(f => f.GetType().Name);
            var present = new FilterConfig { Name = presetName, Filters = presetValues };
            
            _config.Presets.Add(present);

            var wrapper = new PresetListCellWrapper(this, presetName, _config);
            
            _presetsList.data.Add(wrapper);
            _presetsList.tableView.ReloadData();
            
            ToggleNoPresetsText(false);
        }

        [UIAction("ResetFilters")]
        public void ResetFilters()
        {
            Filters.Clear();
            _filtersList.data.Clear();
            _filtersList.tableView.ReloadData();
            
            _sort.values = SortChoices;
            _sort.UpdateChoices();
            _sort.Value = "PP";
            _sort.ApplyValue();
            
            _order.Value = "DESC";
            _order.ApplyValue();
            
            _rankedToggle.Value = true;
            _rankedToggle.ApplyValue();
            
            /*_downloadToggle.Value = true;
            _downloadToggle.ApplyValue();*/

            _scoreView.FilterScores(Filters.Values());
            ToggleNoFiltersText(true);
        }

        [UIAction("CreateFilter")]
        private void CreateFilter()
        {
            var displayFilters = Filters.Filters
                .Where(f => f is DisplayBaseFilter)
                .Cast<DisplayBaseFilter>();
            if (displayFilters.Any(f => f.Name == _filterChoice)) return;

            var filter = TryCreateFilter();
            if (filter == null) return;
            
            var wrapper = new FilterListCellWrapper(this,  filter);
            
            Filters.Filters.Add(filter);
            _filtersList.data.Add(wrapper);
            _filtersList.tableView.ReloadData();

            _scoreView.FilterScores(Filters.Values());
            
            ToggleNoFiltersText(false);
        }
        
        // this is to apply the filter you're on
        // ReSharper disable once UnusedParameter.Local
        [UIAction("FilterSelect")]
        void FilterSelect(object _, int index) => _filterChoice = (string)FilterChoices[index];
        
        // on change functions

        [UIAction("SortChanged")]
        void SortChanged(string sort)
        {
            Filters.Sort = GetSortFilter(sort);
            _scoreView.FilterScores(Filters.Values());
        }

        [UIAction("OrderChanged")]
        void OrderChanged(string order)
        {
            Filters.Order = new OrderFilter(order == "DESC");
            _scoreView.FilterScores(Filters.Values());
        }

        [UIAction("DownloadedChanged")]
        void DownloadedChanged(bool downloaded)
        {
            var existing = Filters.Filters.Find(f => f.GetType().Name == nameof(DownloadedFilter));
            if (existing != null) Filters.Filters.Remove(existing);
            
            var filter = new DownloadedFilter(downloaded);
            Filters.Filters.Add(filter);
            
            _scoreView.FilterScores(Filters.Values());
        }

        [UIAction("RankedChanged")]
        void RankedChanged(bool ranked)
        {
            _starsTab.IsVisible = ranked;
            _accuracyTab.IsVisible = ranked;
            _ppTab.IsVisible = ranked;

            var previous = _sort.Value;

            _sort.values = ranked ?  SortChoices : new List<object> { "Rank", "TimeSet", "MissedNotes" };
            _sort.UpdateChoices();
            _sort.Value = _sort.values.Contains(previous) ? _sort.Value : "TimeSet";
            _sort.ApplyValue();

            var rankedSortFiltersTypes = new[] { typeof(SortAccuracyFilter), typeof(SortPpFilter), typeof(SortStarsFilter) };
            var rankedFiltersTypes = new[] { typeof(StarsFilter), typeof(StarsFilter), typeof(AccuracyFilter), typeof(PpFilter) };

            if (!ranked) {
                if (rankedSortFiltersTypes.Any(t => t.IsInstanceOfType(Filters.Sort))) Filters.Sort = new SortTimeSetFilter();
                
                Filters.Filters.RemoveAll(f => rankedFiltersTypes.Any(t => t.IsInstanceOfType(f)));
                _filtersList.data.RemoveAll(w =>   rankedFiltersTypes.Any(t => t.IsInstanceOfType(((FilterListCellWrapper) w)._filter)));
                
                _filtersList.tableView.ReloadData();
            }

            var rankedFilter = Filters.Filters.Find(f => f.GetType().IsAssignableFrom(typeof(RankedFilter)));
            
            Filters.Filters.Remove(rankedFilter);
            Filters.Filters.Add(new RankedFilter(ranked));

            _scoreView.FilterScores(Filters.Values());
        }
        
        // choice

        [UIValue("filter-choice")] string _filterChoice = "Stars";

        // choices

        [UIValue("order-choices")]
        List<object> OrderChoices => new List<object> { "DESC", "ASC" };

        [UIValue("sort-choices")]
        List<object> SortChoices => new List<object>
        {
            "Rank", "TimeSet", "MissedNotes", "PP", "Stars", "Accuracy"
        };

        [UIValue("filter-choices")]
        List<object> FilterChoices => new List<object>
        {
            "Stars", "PP", "Accuracy", "Misses", "Date"
        };

        // formatters

        List<string> MonthChoices => new List<string>
        {
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

        static BaseFilter GetSortFilter(string sortBy)
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
        
        private DisplayBaseFilter TryCreateFilter()
        {
            switch (_filterChoice)
            {
                case "Stars":
                    float? starsMaximum = null;
                    if (_filterStarsMaximum.Value != _filterStarsMaximum.slider.maxValue) starsMaximum = (int) _filterStarsMaximum.Value;

                    float? starsMinimum = null;
                    if (_filterStarsMinimum.Value != 0f) starsMinimum =  (int) _filterStarsMinimum.Value;
                    
                    if (starsMaximum != null || starsMinimum != null)
                        return new StarsFilter(starsMinimum, starsMaximum);
                    break;

                case "PP":
                    int? ppMaximum = null;
                    if ((int)_filterPpMaximum.Value != _filterPpMaximum.slider.maxValue) ppMaximum = (int)_filterPpMaximum.Value;

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
    }
}
