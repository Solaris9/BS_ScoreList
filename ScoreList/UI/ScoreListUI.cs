using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using ScoreList.Scores;
using System;
using System.Collections.Generic;
using SiraUtil.Logging;
using Zenject;

namespace ScoreList.UI {
    internal class ScoreListCoordinator : FlowCoordinator {
        public static ScoreListCoordinator Instance;

        [Inject]
        private readonly SiraLog _siraLog;
        private readonly ScoreManager _scoreManager;

        private FlowCoordinator _parentFlowCoordinator;
        private ScoreViewController _scoreView;
        private DetailViewController _detailView;
        private FilterViewController _filterView;
        private ConfigViewController _configView;

        public ScoreListCoordinator(SiraLog siraLog, ScoreManager scoreManager)
        {
            _siraLog = siraLog;
            _scoreManager = scoreManager;
        }

        public void ShowFilteredScores(List<BaseFilter> filters) => _scoreView.FilterScores(filters);

        public void Awake()
        {
            if (_scoreView != null) return;
            
            _scoreView = BeatSaberUI.CreateViewController<ScoreViewController>();
            _detailView = BeatSaberUI.CreateViewController<DetailViewController>();
            _filterView = BeatSaberUI.CreateViewController<FilterViewController>();
            _configView = BeatSaberUI.CreateViewController<ConfigViewController>();

            _scoreView.DidSelectSong += HandleDidSelectSong;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            try
            {
                if (!firstActivation) return;
                
                Instance = this;

                SetTitle("ScoreList");
                showBackButton = true;

                ProvideInitialViewControllers(_scoreView, _filterView, _detailView, _configView);
            } catch (Exception ex) {
                _siraLog.Error(ex);
            }
        }

        private async void HandleDidSelectSong(int scoreId)
        {
            var score = await _scoreManager.GetScore(scoreId);
            await _detailView.Load(score);
        }

        protected override void BackButtonWasPressed(ViewController _) => 
            _parentFlowCoordinator.DismissFlowCoordinator(this);
       
        public void SetParentFlowCoordinator(FlowCoordinator parent) => _parentFlowCoordinator = parent;
    }

    internal class ScoreListUI : PersistentSingleton<ScoreListUI> {
        private MenuButton _scoreListMenuButton;
        private ScoreListCoordinator _scoreListFlowCoordinator;

        internal void Setup() {
            _scoreListMenuButton = new MenuButton("ScoreList", "Sort and filter all your scores to view your best and worst!", ScoreListMenuButtonPressed);
            MenuButtons.instance.RegisterButton(_scoreListMenuButton);
        }

        private void ScoreListMenuButtonPressed() {
            if (_scoreListFlowCoordinator == null)
                _scoreListFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<ScoreListCoordinator>();

            _scoreListFlowCoordinator.SetParentFlowCoordinator(BeatSaberUI.MainFlowCoordinator);

            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(_scoreListFlowCoordinator, null, ViewController.AnimationDirection.Horizontal, true);
        }
    }
}
