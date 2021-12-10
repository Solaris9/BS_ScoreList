using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using IPA.Utilities;
using ScoreList.Scores;
using System;
using System.Collections.Generic;
using SiraUtil.Logging;
using Zenject;

namespace ScoreList.UI {
    class ScoreListCoordinator : FlowCoordinator {
        public static ScoreListCoordinator Instance;

        [Inject]
        private readonly SiraLog _siraLog;
        private readonly ScoreManager _scoreManager;

        private FlowCoordinator _parentFlowCoordinator;
        private ScoreViewController _scoreView;
        private DetailViewController _detailView;
        private FilterViewController _filterView;

        public ScoreListCoordinator(SiraLog siraLog, ScoreManager scoreManager)
        {
            _siraLog = siraLog;
            _scoreManager = scoreManager;
        }

        public void ShowFilteredScores(List<BaseFilter> filters) => _scoreView.FilterScores(filters);

        public void Awake()
        {
            if (_scoreView == null)
            {
                _scoreView = BeatSaberUI.CreateViewController<ScoreViewController>();
                _detailView = BeatSaberUI.CreateViewController<DetailViewController>();
                _filterView = BeatSaberUI.CreateViewController<FilterViewController>();

                _scoreView.didSelectSong += HandleDidSelectSong;
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            try {
                if (firstActivation) {
                    Instance = this;

                    SetTitle("ScoreList");
                    showBackButton = true;

                    ProvideInitialViewControllers(_scoreView, _filterView, _detailView);
                }
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

    class ScoreListUI : PersistentSingleton<ScoreListUI> {
        public MainMenuViewController.MenuButton scoreListMenuButton;
        internal ScoreListCoordinator scoreListFlowCooridinator;

        internal void Setup() {
            scoreListMenuButton = new MainMenuViewController.MenuButton("ScoreList", "Sort and filter all your scores to view your best and worst!", ScoreListMenuButtonPressed, true);
            MenuButtons.instance.RegisterButton(scoreListMenuButton);
        }

        private void ScoreListMenuButtonPressed() {
            if (scoreListFlowCooridinator == null)
                scoreListFlowCooridinator = BeatSaberUI.CreateFlowCoordinator<ScoreListCoordinator>();

            scoreListFlowCooridinator.SetParentFlowCoordinator(BeatSaberUI.MainFlowCoordinator);

            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(scoreListFlowCooridinator, null, ViewController.AnimationDirection.Horizontal, true);
        }
    }
}
